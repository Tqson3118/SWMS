const { chromium } = require('../../../../frontend/node_modules/playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });
  const page = await context.newPage();

  console.log('Logging in as Minh Anh...');
  await page.goto('http://127.0.0.1:3000/auth', { waitUntil: 'networkidle' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await page.click('[data-testid="switch-user-secondary"]');
  await page.waitForURL('**/app**');
  console.log('Landed on:', page.url());

  // Click on MobileNav links
  const links = [
    { name: 'Khóa học', href: '/app/courses' },
    { name: 'Tài liệu', href: '/app/documents' },
    { name: 'Sổ tay', href: '/app/notes' },
    { name: 'Ôn tập', href: '/app/review' },
    { name: 'Hôm nay', href: '/app' },
  ];

  for (const item of links) {
    console.log(`Clicking MobileNav item: ${item.name} (${item.href})...`);
    const navLink = page.locator(`[data-testid="mobile-bottom-nav"] a[href="${item.href}"]`);
    await navLink.click();
    await page.waitForURL(`**${item.href}**`);
    await page.waitForTimeout(500);
    console.log(`Successfully navigated to: ${page.url()}`);
  }

  await browser.close();
  console.log('MobileNav navigation test passed completely!');
})().catch(err => {
  console.error(err);
  process.exit(1);
});
