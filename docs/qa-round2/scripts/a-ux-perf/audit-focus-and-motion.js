const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../../screenshots/a-ux-perf');
const DATA_DIR = path.resolve(__dirname, '../../data/a-ux-perf');

(async () => {
  const browser = await chromium.launch({ headless: true });

  // 1. Focus Ring Audit on Dashboard and Auth
  console.log('Testing Tab Navigation and Focus Ring...');
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const p = await context.newPage();

  // Test Auth focus
  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.waitForTimeout(500);

  const authFocusSteps = [];
  for (let i = 0; i < 8; i++) {
    await p.keyboard.press('Tab');
    const focusedInfo = await p.evaluate(() => {
      const el = document.activeElement;
      if (!el || el === document.body) return null;
      const s = window.getComputedStyle(el);
      return {
        tag: el.tagName,
        id: el.id,
        type: el.getAttribute('type'),
        role: el.getAttribute('role'),
        text: el.textContent?.trim().slice(0, 30),
        outlineStyle: s.outlineStyle,
        outlineWidth: s.outlineWidth,
        outlineColor: s.outlineColor,
        boxShadow: s.boxShadow,
        hasVisibleRing: s.outlineStyle !== 'none' || s.boxShadow !== 'none'
      };
    });
    if (focusedInfo) authFocusSteps.push(focusedInfo);
  }

  await p.screenshot({ path: path.join(OUT_DIR, 'focus-ring-auth.png') });

  // Login and test App Dashboard focus
  await p.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    p.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    p.click('[data-testid="switch-user-default"]')
  ]);
  await p.waitForURL('**/app**');
  await p.waitForTimeout(800);

  const appFocusSteps = [];
  for (let i = 0; i < 12; i++) {
    await p.keyboard.press('Tab');
    const focusedInfo = await p.evaluate(() => {
      const el = document.activeElement;
      if (!el || el === document.body) return null;
      const s = window.getComputedStyle(el);
      return {
        tag: el.tagName,
        id: el.id,
        type: el.getAttribute('type'),
        text: el.textContent?.trim().slice(0, 30),
        outlineStyle: s.outlineStyle,
        outlineWidth: s.outlineWidth,
        outlineColor: s.outlineColor,
        boxShadow: s.boxShadow,
        hasVisibleRing: s.outlineStyle !== 'none' || s.boxShadow !== 'none'
      };
    });
    if (focusedInfo) appFocusSteps.push(focusedInfo);
  }

  await p.screenshot({ path: path.join(OUT_DIR, 'focus-ring-app.png') });

  // 2. Reduced Motion Audit
  console.log('Testing prefers-reduced-motion...');
  const motionContext = await browser.newContext({
    reducedMotion: 'reduce',
    viewport: { width: 1440, height: 900 }
  });
  const motionPage = await motionContext.newPage();
  await motionPage.goto(`${BASE_URL}/`);
  await motionPage.waitForTimeout(500);

  const reducedMotionInfo = await motionPage.evaluate(() => {
    const isMatched = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    // Check if pulse animations or keyframes exist
    const styleSheets = Array.from(document.styleSheets);
    let reducedRulesCount = 0;
    try {
      for (const sheet of styleSheets) {
        for (const rule of sheet.cssRules || []) {
          if (rule.media && rule.media.mediaText.includes('prefers-reduced-motion')) {
            reducedRulesCount++;
          }
        }
      }
    } catch {}

    const animatedElements = [];
    document.querySelectorAll('*').forEach(el => {
      const s = window.getComputedStyle(el);
      if (s.animationName !== 'none' && parseFloat(s.animationDuration) > 0) {
        animatedElements.push({
          tag: el.tagName,
          animationName: s.animationName,
          animationDuration: s.animationDuration
        });
      }
    });

    return {
      mediaMatches: isMatched,
      reducedRulesInCssCount: reducedRulesCount,
      animatedElementsCount: animatedElements.length,
      animatedElements: animatedElements.slice(0, 5)
    };
  });

  const summary = {
    authFocusSteps,
    appFocusSteps,
    reducedMotionInfo
  };

  fs.writeFileSync(
    path.join(DATA_DIR, 'focus-and-motion-audit.json'),
    JSON.stringify(summary, null, 2),
    'utf-8'
  );

  console.log('=== FOCUS & MOTION AUDIT SUMMARY ===');
  console.log(JSON.stringify(summary, null, 2));

  await browser.close();
})();
