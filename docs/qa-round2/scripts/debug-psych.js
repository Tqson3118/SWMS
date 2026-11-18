const path = require('path');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  page.on('response', res => {
    if (res.status() >= 400) {
      console.log(`HTTP ${res.status()}: ${res.request().method()} ${res.url()}`);
    }
  });
  page.on('console', msg => {
    if (msg.type() === 'error') {
      console.log('CONSOLE ERROR:', msg.text());
    }
  });

  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="demo-switcher-toggle"]', { timeout: 10000 });
  const switcherVisible = await page.isVisible('[data-testid="switch-user-default"]');
  if (!switcherVisible) {
    await page.click('[data-testid="demo-switcher-toggle"]');
    await page.waitForSelector('[data-testid="switch-user-default"]', { timeout: 5000 });
  }
  await Promise.all([
    page.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    page.click('[data-testid="switch-user-default"]')
  ]);
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(600);

  const testCourse = '43C1CB70-DCDD-4858-8FAD-575D3C149CC7';
  const testLesson = 'C45B8820-EAD3-438B-BD17-8DA56AD96C05';
  console.log('Navigating to course...');
  await page.goto(`http://127.0.0.1:3000/app/courses/${testCourse}/learn?lesson=${testLesson}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);

  // Click practice tab
  const practiceBtn = page.locator('[data-testid="center-tab-practice"]');
  await practiceBtn.click();
  await page.waitForTimeout(1000);

  const recallModeBtn = page.locator('[data-testid="practice-mode-recall"]');
  console.log('recallModeBtn visible:', await recallModeBtn.isVisible());
  if (await recallModeBtn.isVisible()) {
    await recallModeBtn.click();
    await page.waitForTimeout(1000);
  }

  const roleplaySubTab = page.locator('button:has-text("Mô phỏng tình huống & Tâm lý")');
  console.log('roleplaySubTab visible:', await roleplaySubTab.isVisible());
  if (await roleplaySubTab.isVisible()) {
    await roleplaySubTab.click();
    await page.waitForTimeout(1000);
  }

  try {
    const loc1 = page.locator('[data-testid="psychology-practice-lab"]');
    console.log('loc1 [data-testid="psychology-practice-lab"] count:', await loc1.count(), 'visible:', await loc1.isVisible());
  } catch (e) {
    console.error('loc1 error:', e.message);
  }

  try {
    const loc2 = page.locator('text=Phòng Thực Hành Tâm Lý Học & Kỹ Năng Sống');
    console.log('loc2 text count:', await loc2.count(), 'visible:', await loc2.first().isVisible());
  } catch (e) {
    console.error('loc2 error:', e.message);
  }

  try {
    const locCombined = page.locator('[data-testid="psychology-practice-lab"], text=Phòng Thực Hành Tâm Lý Học & Kỹ Năng Sống');
    console.log('locCombined count:', await locCombined.count(), 'visible:', await locCombined.isVisible());
  } catch (e) {
    console.error('locCombined error:', e.message);
  }

  await browser.close();
})();
