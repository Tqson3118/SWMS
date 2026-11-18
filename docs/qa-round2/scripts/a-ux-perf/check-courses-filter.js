const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../../screenshots/a-ux-perf');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const p = await context.newPage();

  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    p.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    p.click('[data-testid="switch-user-default"]'),
  ]);
  await p.waitForURL('**/app**', { timeout: 15000 });
  await p.waitForTimeout(1000);

  // Navigate to /app/courses using Next.js client router or click on sidebar link!
  const coursesLink = p.locator('a[href="/app/courses"]').first();
  if (await coursesLink.isVisible()) {
    await coursesLink.click();
  } else {
    await p.goto(`${BASE_URL}/app/courses`, { waitUntil: 'networkidle' });
  }
  await p.waitForTimeout(1500);

  // Measure domain filter buttons and container
  const filterInfo = await p.evaluate(() => {
    const allDomainBtn = Array.from(document.querySelectorAll('button')).find(b => b.textContent?.includes('Tất cả lĩnh vực'));
    const container = allDomainBtn ? allDomainBtn.parentElement : null;
    if (!container) return { error: 'Domain container not found', bodyText: document.body.innerText.slice(0, 300) };

    const cRect = container.getBoundingClientRect();
    const buttons = Array.from(container.querySelectorAll('button')).map((btn, idx) => {
      const bRect = btn.getBoundingClientRect();
      return {
        idx,
        text: btn.textContent?.trim(),
        rect: {
          x: Math.round(bRect.x),
          y: Math.round(bRect.y),
          w: Math.round(bRect.width),
          h: Math.round(bRect.height),
          right: Math.round(bRect.right)
        },
        isClippedRight: bRect.right > cRect.right,
        isClippedLeft: bRect.left < cRect.left
      };
    });

    const lastBtn = buttons[buttons.length - 1];

    return {
      containerRect: {
        x: Math.round(cRect.x),
        y: Math.round(cRect.y),
        w: Math.round(cRect.width),
        h: Math.round(cRect.height),
        scrollWidth: container.scrollWidth,
        clientWidth: container.clientWidth
      },
      hasScrollbar: container.scrollWidth > container.clientWidth,
      buttonsCount: buttons.length,
      buttons,
      lastButton: lastBtn,
      isLastButtonClipped: lastBtn ? lastBtn.isClippedRight : false
    };
  });

  console.log('=== COURSES FILTER BAR INFO ===');
  console.log(JSON.stringify(filterInfo, null, 2));

  // Take screenshot
  await p.screenshot({ path: path.join(OUT_DIR, 'courses-filter-1440x900.png'), fullPage: false });

  await browser.close();
})();
