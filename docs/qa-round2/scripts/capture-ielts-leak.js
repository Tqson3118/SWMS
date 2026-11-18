const path = require('path');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/c0-before');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const p = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.click('[data-testid="demo-switcher-toggle"]');
  await p.click('[data-testid="switch-user-default"]');
  await p.waitForURL('**/app**');

  const courseId = '79B30ACD-0BD5-41C8-8744-271AD7B496CE';
  const lessonId = '0360C033-EBC7-453D-9FAE-C812D3BCB48A';
  await p.goto(`${BASE_URL}/app/courses/${courseId}/learn?lesson=${lessonId}`, { waitUntil: 'networkidle' });
  await p.waitForTimeout(1000);

  // Light
  await p.evaluate(() => {
    localStorage.setItem('swms_theme', 'light');
    document.documentElement.classList.remove('dark');
    document.body?.classList.remove('dark');
  });
  await p.waitForTimeout(500);
  await p.screenshot({ path: path.join(OUT_DIR, 'ielts-leak-readme-1440x900-light.png'), fullPage: true });
  console.log('Saved ielts-leak-readme-1440x900-light.png');

  // Dark
  await p.evaluate(() => {
    localStorage.setItem('swms_theme', 'dark');
    document.documentElement.classList.add('dark');
    document.body?.classList.add('dark');
  });
  await p.waitForTimeout(500);
  await p.screenshot({ path: path.join(OUT_DIR, 'ielts-leak-readme-1440x900-dark.png'), fullPage: true });
  console.log('Saved ielts-leak-readme-1440x900-dark.png');

  await browser.close();
})().catch(console.error);
