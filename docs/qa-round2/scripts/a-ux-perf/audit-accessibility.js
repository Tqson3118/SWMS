const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../../screenshots/a-ux-perf');
const DATA_DIR = path.resolve(__dirname, '../../data/a-ux-perf');

if (!fs.existsSync(OUT_DIR)) fs.mkdirSync(OUT_DIR, { recursive: true });
if (!fs.existsSync(DATA_DIR)) fs.mkdirSync(DATA_DIR, { recursive: true });

async function loginUser(page) {
  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
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
}

async function auditRoute(browser, routePath, routeName, vp, theme) {
  const context = await browser.newContext({
    viewport: { width: vp.width, height: vp.height },
    isMobile: vp.isMobile || false
  });

  const page = await context.newPage();
  if (routePath !== '/' && routePath !== '/auth') {
    await loginUser(page);
  }

  await page.goto(`${BASE_URL}${routePath}`, { waitUntil: 'networkidle' }).catch(() => {});
  await page.waitForTimeout(600);

  // Apply theme
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
  await page.waitForTimeout(400);

  // Run audit script
  const auditResult = await page.evaluate(() => {
    function getLuminance(r, g, b) {
      const a = [r, g, b].map(v => {
        v /= 255;
        return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4);
      });
      return a[0] * 0.2126 + a[1] * 0.7152 + a[2] * 0.0722;
    }

    function parseRgba(str) {
      if (!str || str === 'transparent' || str === 'rgba(0, 0, 0, 0)') return null;
      const m = str.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([\d.]+))?\)/);
      if (!m) return null;
      return {
        r: parseInt(m[1], 10),
        g: parseInt(m[2], 10),
        b: parseInt(m[3], 10),
        a: m[4] !== undefined ? parseFloat(m[4]) : 1
      };
    }

    function getEffectiveBg(el) {
      let curr = el;
      while (curr && curr !== document.documentElement) {
        const style = window.getComputedStyle(curr);
        const bg = parseRgba(style.backgroundColor);
        if (bg && bg.a > 0.5) {
          return bg;
        }
        curr = curr.parentElement;
      }
      const isDark = document.documentElement.classList.contains('dark') || document.body?.classList.contains('dark');
      return isDark ? { r: 15, g: 15, b: 18, a: 1 } : { r: 255, g: 255, b: 255, a: 1 };
    }

    // 1. Heading structure
    const h1s = Array.from(document.querySelectorAll('h1')).map(h => ({
      text: h.textContent.trim().slice(0, 60),
      visible: h.offsetParent !== null
    }));

    // 2. Icon-only buttons without accessible name
    const allButtons = Array.from(document.querySelectorAll('button, a[role="button"]'));
    const buttonsWithoutAccessibleName = [];
    allButtons.forEach(btn => {
      const accessibleName = btn.getAttribute('aria-label') ||
                             btn.getAttribute('aria-labelledby') ||
                             btn.getAttribute('title') ||
                             btn.textContent.trim();
      if (!accessibleName && btn.offsetParent !== null) {
        buttonsWithoutAccessibleName.push({
          tag: btn.tagName,
          testid: btn.getAttribute('data-testid') || '',
          className: btn.className,
          html: btn.outerHTML.slice(0, 150)
        });
      }
    });

    // 3. Contrast audit for visible text elements
    const contrastViolations = [];
    const elementsToCheck = document.querySelectorAll('p, span, h1, h2, h3, h4, label, button, a, input, [role="button"]');
    elementsToCheck.forEach(el => {
      if (el.offsetParent === null) return;
      const text = el.textContent ? el.textContent.trim() : '';
      if (!text && el.tagName !== 'INPUT') return;

      const style = window.getComputedStyle(el);
      const color = parseRgba(style.color);
      if (!color) return;

      const bg = getEffectiveBg(el);
      const L1 = getLuminance(color.r, color.g, color.b);
      const L2 = getLuminance(bg.r, bg.g, bg.b);
      const ratio = (Math.max(L1, L2) + 0.05) / (Math.min(L1, L2) + 0.05);

      const fontSize = parseFloat(style.fontSize);
      const fontWeight = parseInt(style.fontWeight, 10) || 400;
      const isLarge = fontSize >= 24 || (fontSize >= 18.66 && fontWeight >= 700);
      const requiredRatio = isLarge ? 3.0 : 4.5;

      if (ratio < requiredRatio && text.length > 0) {
        contrastViolations.push({
          selector: el.tagName + (el.id ? '#' + el.id : '') + (el.className && typeof el.className === 'string' ? '.' + el.className.split(/\s+/).slice(0, 2).join('.') : ''),
          text: text.slice(0, 50),
          ratio: +(ratio.toFixed(2)),
          requiredRatio,
          color: style.color,
          bgColor: 'rgb(' + bg.r + ',' + bg.g + ',' + bg.b + ')',
          fontSize: style.fontSize,
          fontWeight: style.fontWeight
        });
      }
    });

    // Deduplicate violations by text + selector
    const seen = new Set();
    const uniqueViolations = [];
    for (const v of contrastViolations) {
      const key = v.selector + '::' + v.text;
      if (!seen.has(key)) {
        seen.add(key);
        uniqueViolations.push(v);
      }
    }

    return {
      h1Count: h1s.length,
      h1s,
      unlabeledButtonsCount: buttonsWithoutAccessibleName.length,
      unlabeledButtons: buttonsWithoutAccessibleName.slice(0, 10),
      contrastViolationsCount: uniqueViolations.length,
      contrastViolations: uniqueViolations.slice(0, 15)
    };
  });

  // Screenshot
  const filename = `a11y-${routeName}-${vp.name}-${theme}.png`;
  await page.screenshot({ path: path.join(OUT_DIR, filename), fullPage: false });

  await page.close();
  await context.close();

  return {
    route: routePath,
    name: routeName,
    viewport: vp.name,
    theme,
    screenshot: filename,
    ...auditResult
  };
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  const viewports = [
    { name: '1440x900', width: 1440, height: 900, isMobile: false },
    { name: '390x844', width: 390, height: 844, isMobile: true }
  ];

  const themes = ['light', 'dark'];

  const routes = [
    { path: '/', name: 'landing' },
    { path: '/auth', name: 'auth' },
    { path: '/app', name: 'dashboard' },
    { path: '/app/courses', name: 'courses' },
    { path: '/app/courses/22222222-2222-4222-8222-222222222222/learn', name: 'learn' },
    { path: '/app/review', name: 'review' },
    { path: '/app/documents', name: 'documents' },
    { path: '/app/notes', name: 'notes' },
    { path: '/app/settings', name: 'settings' }
  ];

  const allAudits = [];

  for (const r of routes) {
    for (const vp of viewports) {
      for (const t of themes) {
        console.log(`Auditing ${r.name} (${vp.name}, ${t})...`);
        try {
          const res = await auditRoute(browser, r.path, r.name, vp, t);
          allAudits.push(res);
          console.log(`  H1s: ${res.h1Count}, Unlabeled btns: ${res.unlabeledButtonsCount}, Contrast issues: ${res.contrastViolationsCount}`);
        } catch (err) {
          console.error(`  Failed to audit ${r.name}:`, err.message);
        }
      }
    }
  }

  fs.writeFileSync(
    path.join(DATA_DIR, 'a11y-audit-results.json'),
    JSON.stringify(allAudits, null, 2),
    'utf-8'
  );
  console.log(`\nSaved all a11y results to ${path.join(DATA_DIR, 'a11y-audit-results.json')}`);

  await browser.close();
})();
