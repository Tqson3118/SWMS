const { chromium } = require('../../../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const SCREENSHOT_DIR = path.resolve(__dirname, '../../screenshots/a-newbie-mobile');
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

const VIEWPORTS = [
  { name: 'mobile', width: 390, height: 844 },
  { name: 'tablet', width: 768, height: 1024 },
];

const PUBLIC_ROUTES = ['/', '/auth'];
const AUTH_ROUTES = [
  '/app',
  '/app/courses',
  '/app/courses/new',
  '/app/review',
  '/app/notes',
  '/app/documents',
  '/workspace',
];

async function measureCurrentPage(page, route, vpName, vp) {
  console.log(`Auditing ${route} on ${vpName} (${vp.width}x${vp.height})...`);
  await page.waitForTimeout(600);

  const finalUrl = page.url();
  console.log(`  Arrived at: ${finalUrl}`);

  const cleanRouteName = route.replace(/\//g, '_') || 'root';
  const screenshotName = `audit-${vpName}-${cleanRouteName}.png`;
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, screenshotName), fullPage: false });

  // 1. Horizontal overflow check
  const overflowCheck = await page.evaluate((vpWidth) => {
    const docEl = document.documentElement;
    const body = document.body;
    const scrollWidth = Math.max(docEl.scrollWidth, body.scrollWidth);
    const clientWidth = docEl.clientWidth;
    const isOverflowing = scrollWidth > vpWidth + 1;

    let overflowingElements = [];
    if (isOverflowing) {
      const allEls = document.querySelectorAll('*');
      for (const el of allEls) {
        const rect = el.getBoundingClientRect();
        if (rect.right > vpWidth + 1 && rect.width > 0 && rect.height > 0) {
          overflowingElements.push({
            tagName: el.tagName.toLowerCase(),
            className: (el.className || '').toString().slice(0, 80),
            id: el.id,
            testId: el.getAttribute('data-testid') || '',
            text: (el.textContent || '').trim().slice(0, 40),
            rect: { x: Math.round(rect.x), y: Math.round(rect.y), width: Math.round(rect.width), right: Math.round(rect.right) },
          });
          if (overflowingElements.length >= 10) break;
        }
      }
    }

    return {
      scrollWidth,
      clientWidth,
      vpWidth,
      isOverflowing,
      overflowingElements,
    };
  }, vp.width);

  // 2. Measure tap targets
  const tapTargetCheck = await page.evaluate(() => {
    const candidates = Array.from(
      document.querySelectorAll('button, a[href], input, select, textarea, [role="button"], [role="tab"]')
    );

    const issues = [];
    const interactiveElements = [];

    for (let i = 0; i < candidates.length; i++) {
      const el = candidates[i];
      const style = window.getComputedStyle(el);
      if (
        style.display === 'none' ||
        style.visibility === 'hidden' ||
        style.opacity === '0' ||
        el.getAttribute('aria-hidden') === 'true'
      ) {
        continue;
      }

      const rect = el.getBoundingClientRect();
      if (rect.width === 0 || rect.height === 0) continue;

      const info = {
        index: i,
        tagName: el.tagName.toLowerCase(),
        testId: el.getAttribute('data-testid') || '',
        text: (el.innerText || el.getAttribute('aria-label') || el.getAttribute('placeholder') || el.value || '').trim().slice(0, 40),
        width: Math.round(rect.width * 10) / 10,
        height: Math.round(rect.height * 10) / 10,
        top: Math.round(rect.top),
        bottom: Math.round(rect.bottom),
        left: Math.round(rect.left),
        right: Math.round(rect.right),
      };

      interactiveElements.push(info);

      const isSub44 = rect.width < 44 || rect.height < 44;
      const isSub36 = rect.width < 36 || rect.height < 36;

      if (isSub44) {
        issues.push({
          ...info,
          severity: isSub36 ? 'Critical_Sub36' : 'Sub44',
          reason: `Kích thước tap target: ${info.width}x${info.height}px (chuẩn tối thiểu 44x44px)`,
        });
      }
    }

    // Spacing issues (< 6px between adjacent tap targets)
    const spacingIssues = [];
    for (let i = 0; i < interactiveElements.length; i++) {
      for (let j = i + 1; j < interactiveElements.length; j++) {
        const a = interactiveElements[i];
        const b = interactiveElements[j];
        const horizontalOverlap = !(a.right < b.left || a.left > b.right);
        const verticalOverlap = !(a.bottom < b.top || a.top > b.bottom);

        const edgeDistanceX = Math.max(0, Math.max(a.left, b.left) - Math.min(a.right, b.right));
        const edgeDistanceY = Math.max(0, Math.max(a.top, b.top) - Math.min(a.bottom, b.bottom));

        if (edgeDistanceX < 6 && edgeDistanceY < 6 && (horizontalOverlap || verticalOverlap)) {
          spacingIssues.push({
            elemA: { tag: a.tagName, text: a.text, testId: a.testId, rect: `${a.width}x${a.height}` },
            elemB: { tag: b.tagName, text: b.text, testId: b.testId, rect: `${b.width}x${b.height}` },
            edgeDistX: edgeDistanceX,
            edgeDistY: edgeDistanceY,
          });
          if (spacingIssues.length >= 10) break;
        }
      }
      if (spacingIssues.length >= 10) break;
    }

    return {
      totalInteractive: interactiveElements.length,
      sub44Count: issues.length,
      sub36Count: issues.filter(x => x.severity === 'Critical_Sub36').length,
      issues: issues.slice(0, 15),
      spacingIssues,
    };
  });

  // 3. Obscured by MobileNav
  const mobileNavObscurity = await page.evaluate((vpHeight) => {
    const mobileNav = document.querySelector('[data-testid="mobile-bottom-nav"]');
    if (!mobileNav) return { hasNav: false, obscuredElements: [] };

    const mainEl = document.querySelector('main');
    if (mainEl) {
      mainEl.scrollTop = mainEl.scrollHeight;
    }
    window.scrollTo(0, document.body.scrollHeight);

    const obscuredElements = [];
    const interactive = Array.from(document.querySelectorAll('button, a[href], input, [role="button"]'));
    const navTop = vpHeight - 56;

    for (const el of interactive) {
      if (mobileNav.contains(el)) continue;
      const rect = el.getBoundingClientRect();
      if (rect.height === 0 || rect.width === 0) continue;
      if (rect.bottom > navTop && rect.top < vpHeight) {
        obscuredElements.push({
          tagName: el.tagName.toLowerCase(),
          testId: el.getAttribute('data-testid') || '',
          text: (el.innerText || el.getAttribute('aria-label') || '').trim().slice(0, 40),
          rect: { top: Math.round(rect.top), bottom: Math.round(rect.bottom), height: Math.round(rect.height) },
          overlapPx: Math.round(rect.bottom - navTop),
        });
      }
    }

    return {
      hasNav: true,
      navTop,
      obscuredElements,
    };
  }, vp.height);

  return {
    route,
    finalUrl,
    vpName,
    vp,
    screenshotName,
    overflowCheck,
    tapTargetCheck,
    mobileNavObscurity,
  };
}

async function testMobileSearchCommandPalette(page) {
  console.log('--- Testing MobileNav Search Button & Command Palette on 390x844 ---');
  await page.goto('http://127.0.0.1:3000/app', { waitUntil: 'networkidle' });
  await page.waitForTimeout(600);

  const searchBtn = page.locator('[data-testid="mobile-search-btn"]');
  const isSearchVisible = await searchBtn.isVisible();
  console.log('Mobile Search Button visible:', isSearchVisible);

  if (!isSearchVisible) {
    return { error: 'mobile-search-btn not visible on 390x844' };
  }

  const searchBtnBox = await searchBtn.boundingBox();
  console.log('Search Button Bounding Box:', searchBtnBox);

  // Click search button
  await searchBtn.click();
  await page.waitForTimeout(500);

  // Check command palette modal
  const paletteModal = page.locator('[data-testid="command-palette-modal"]').first();
  const isModalVisible = await paletteModal.isVisible().catch(() => false);
  console.log('Command Palette Modal visible:', isModalVisible);

  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'search-palette-open-390.png'), fullPage: false });

  // Type keyword
  const searchInput = page.locator('input[placeholder*="Tìm" i], input[type="text"]').first();
  await searchInput.fill('khóa học');
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, 'search-palette-typed-390.png'), fullPage: false });

  const modalBox = await paletteModal.boundingBox().catch(() => null);
  console.log('Modal Bounding Box on 390px:', modalBox);

  // Test closing via ESC
  await page.keyboard.press('Escape');
  await page.waitForTimeout(400);
  const isStillVisible = await paletteModal.isVisible().catch(() => false);
  console.log('Modal closed on ESC:', !isStillVisible);

  return {
    searchBtnBox,
    isModalVisible,
    modalBox,
    closedOnEsc: !isStillVisible,
  };
}

async function registerTestUser(page, vpName) {
  const ts = Date.now();
  const email = `qa+newbie-${vpName}-${ts}@test.local`;
  const password = 'Password123!';
  const fullName = `Người Mới ${vpName.toUpperCase()}`;

  console.log(`Registering dedicated user for ${vpName}: ${email}...`);
  await page.goto('http://127.0.0.1:3000/auth?tab=register', { waitUntil: 'networkidle' });
  await page.fill('#auth-fullname', fullName);
  await page.fill('#auth-email', email);
  await page.fill('#auth-password', password);
  await page.click('button[type="submit"]');

  await page.waitForURL('**/app**', { timeout: 15000 });
  console.log(`User registered & landed on ${page.url()}`);
  return { email, password, fullName };
}

async function runAudit() {
  console.log('=== STARTING 02: REVISED TAP TARGET & LAYOUT AUDIT ===');
  const browser = await chromium.launch({ headless: true });
  const allResults = [];

  for (const vp of VIEWPORTS) {
    console.log(`\n--- Viewport: ${vp.name.toUpperCase()} (${vp.width}x${vp.height}) ---`);

    // 1. Audit public pages (unauthenticated)
    const publicContext = await browser.newContext({
      viewport: { width: vp.width, height: vp.height },
      isMobile: true,
      hasTouch: true,
      userAgent: vp.name === 'mobile'
        ? 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1'
        : 'Mozilla/5.0 (iPad; CPU OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
    });
    const publicPage = await publicContext.newPage();
    for (const route of PUBLIC_ROUTES) {
      await publicPage.goto(`http://127.0.0.1:3000${route}`, { waitUntil: 'networkidle' });
      const res = await measureCurrentPage(publicPage, route, vp.name, vp);
      allResults.push(res);
    }
    await publicContext.close();

    // 2. Audit authenticated pages with freshly registered account
    const authContext = await browser.newContext({
      viewport: { width: vp.width, height: vp.height },
      isMobile: true,
      hasTouch: true,
      userAgent: vp.name === 'mobile'
        ? 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1'
        : 'Mozilla/5.0 (iPad; CPU OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
    });
    const authPage = await authContext.newPage();
    const user = await registerTestUser(authPage, vp.name);

    for (const route of AUTH_ROUTES) {
      // Navigate via client-side routing to preserve active in-memory session
      console.log(`Navigating to ${route}...`);
      await authPage.evaluate((target) => {
        window.location.pathname = target;
      }, route);
      await authPage.waitForLoadState('networkidle');
      await authPage.waitForTimeout(600);

      const res = await measureCurrentPage(authPage, route, vp.name, vp);
      allResults.push(res);
    }

    if (vp.name === 'mobile') {
      const searchRes = await testMobileSearchCommandPalette(authPage);
      fs.writeFileSync(
        path.join(__dirname, 'search-cmd-results.json'),
        JSON.stringify(searchRes, null, 2),
        'utf-8'
      );
    }

    await authContext.close();
  }

  // Save audit data
  fs.writeFileSync(
    path.join(__dirname, 'tap-target-audit-results.json'),
    JSON.stringify(allResults, null, 2),
    'utf-8'
  );

  console.log('=== COMPLETED 02: REVISED TAP TARGET & LAYOUT AUDIT ===');
  await browser.close();
}

runAudit().catch(err => {
  console.error('Audit failed:', err);
  process.exit(1);
});
