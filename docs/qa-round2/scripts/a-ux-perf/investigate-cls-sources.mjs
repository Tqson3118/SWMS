import fs from 'node:fs';
import path from 'node:path';
import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve('docs/qa-round2/screenshots/a-ux-perf');
const DATA_DIR = path.resolve('docs/qa-round2/data/a-ux-perf');

if (!fs.existsSync(OUT_DIR)) fs.mkdirSync(OUT_DIR, { recursive: true });
if (!fs.existsSync(DATA_DIR)) fs.mkdirSync(DATA_DIR, { recursive: true });

// Check which course IDs are available
const COURSES_TO_TEST = [
  '22222222-2222-4222-8222-222222222222',
  '25f79a81-8ac2-46ae-99e1-85975ff42a8b',
  '79B30ACD-0BD5-41C8-8744-271AD7B496CE'
];

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

const INJECT_OBSERVER_SCRIPT = `
  window.__clsEntries = [];
  window.__lcpEntries = [];

  function getSelector(el) {
    if (!el || el.nodeType !== Node.ELEMENT_NODE) return el ? el.nodeName : 'unknown';
    const parts = [];
    let curr = el;
    while (curr && curr.nodeType === Node.ELEMENT_NODE && parts.length < 5) {
      let part = curr.localName;
      if (curr.id) {
        part += '#' + curr.id;
        parts.unshift(part);
        break;
      }
      if (curr.getAttribute('data-testid')) {
        part += '[data-testid="' + curr.getAttribute('data-testid') + '"]';
        parts.unshift(part);
        break;
      }
      if (curr.className && typeof curr.className === 'string') {
        const cl = curr.className.split(/\\s+/).filter(c => c && !c.includes(':') && !c.includes('[') && !c.includes('/'));
        if (cl.length > 0) part += '.' + cl.slice(0, 2).join('.');
      }
      parts.unshift(part);
      curr = curr.parentElement;
    }
    return parts.join(' > ');
  }

  try {
    new PerformanceObserver((entryList) => {
      for (const entry of entryList.getEntries()) {
        if (!entry.hadRecentInput) {
          const sources = (entry.sources || []).map(src => ({
            node: src.node ? {
              tagName: src.node.tagName,
              id: src.node.id,
              selector: getSelector(src.node),
              snippet: (src.node.textContent || '').slice(0, 100).replace(/\\s+/g, ' ').trim()
            } : null,
            previousRect: src.previousRect ? {
              x: Math.round(src.previousRect.x),
              y: Math.round(src.previousRect.y),
              w: Math.round(src.previousRect.width),
              h: Math.round(src.previousRect.height)
            } : null,
            currentRect: src.currentRect ? {
              x: Math.round(src.currentRect.x),
              y: Math.round(src.currentRect.y),
              w: Math.round(src.currentRect.width),
              h: Math.round(src.currentRect.height)
            } : null
          }));
          window.__clsEntries.push({
            value: entry.value,
            startTime: Math.round(entry.startTime),
            sources
          });
        }
      }
    }).observe({ type: 'layout-shift', buffered: true });
  } catch (e) {
    console.error('CLS observer error:', e);
  }

  try {
    new PerformanceObserver((entryList) => {
      for (const entry of entryList.getEntries()) {
        window.__lcpEntries.push({
          startTime: Math.round(entry.startTime),
          size: entry.size,
          id: entry.id,
          url: entry.url,
          element: entry.element ? {
            tagName: entry.element.tagName,
            selector: getSelector(entry.element),
            snippet: (entry.element.textContent || '').slice(0, 100).replace(/\\s+/g, ' ').trim()
          } : null
        });
      }
    }).observe({ type: 'largest-contentful-paint', buffered: true });
  } catch (e) {
    console.error('LCP observer error:', e);
  }
`;

async function measureRoute(browser, routeDef, vp) {
  const context = await browser.newContext({
    viewport: { width: vp.width, height: vp.height },
    isMobile: vp.isMobile || false,
    hasTouch: vp.isMobile || false,
    userAgent: vp.isMobile
      ? 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1'
      : undefined
  });

  const authPage = await context.newPage();
  await loginUser(authPage);
  await authPage.close();

  const page = await context.newPage();
  const cdp = await context.newCDPSession(page);
  await cdp.send('Performance.enable');

  if (vp.cpuThrottle > 1) {
    await cdp.send('Emulation.setCPUThrottlingRate', { rate: vp.cpuThrottle });
  }

  await page.addInitScript(INJECT_OBSERVER_SCRIPT);

  const url = `${BASE_URL}${routeDef.path}`;
  console.log(`\n--- Measuring ${routeDef.name} (${vp.name}) at ${url} ---`);

  const navStart = performance.now();
  await page.goto(url, { waitUntil: 'load', timeout: 30000 });
  await page.waitForTimeout(3000); // Allow all shifts to settle

  const metrics = await page.evaluate(() => {
    const clsEntries = window.__clsEntries || [];
    const totalCls = clsEntries.reduce((sum, e) => sum + e.value, 0);
    const lcpEntries = window.__lcpEntries || [];
    const lastLcp = lcpEntries.length > 0 ? lcpEntries[lcpEntries.length - 1] : null;
    return {
      totalCls: +(totalCls.toFixed(4)),
      clsEntries,
      lcp: lastLcp ? lastLcp.startTime : 0,
      lcpEntries
    };
  });

  const screenshotName = `cls-${routeDef.name}-${vp.name}.png`;
  await page.screenshot({ path: path.join(OUT_DIR, screenshotName), fullPage: false });

  await page.close();
  await context.close();

  return {
    route: routeDef.path,
    routeName: routeDef.name,
    viewport: vp.name,
    ...metrics,
    screenshot: screenshotName
  };
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  const desktopVp = { name: '1440x900', width: 1440, height: 900, isMobile: false, cpuThrottle: 1 };
  const mobileVp = { name: '390x844', width: 390, height: 844, isMobile: true, cpuThrottle: 4 };

  const desktopRoutes = [
    { name: 'app-dashboard', path: '/app' },
    { name: 'app-courses', path: '/app/courses' },
    { name: 'app-documents', path: '/app/documents' },
    { name: 'app-notes', path: '/app/notes' },
    { name: 'app-quiz', path: '/app/courses/22222222-2222-4222-8222-222222222222/quiz' },
    { name: 'app-settings', path: '/app/settings' },
  ];

  const mobileRoutes = [
    { name: 'learn-mobile-os', path: '/app/courses/22222222-2222-4222-8222-222222222222/learn' },
    { name: 'learn-mobile-ielts', path: '/app/courses/79B30ACD-0BD5-41C8-8744-271AD7B496CE/learn?lesson=0360C033-EBC7-453D-9FAE-C812D3BCB48A' },
  ];

  const results = [];

  for (const r of desktopRoutes) {
    try {
      const res = await measureRoute(browser, r, desktopVp);
      results.push(res);
      console.log(`Result ${r.name}: CLS = ${res.totalCls}, Shifts count = ${res.clsEntries.length}`);
      for (const entry of res.clsEntries) {
        console.log(`  Shift value: ${entry.value.toFixed(4)} at ${entry.startTime}ms`);
        for (const src of entry.sources) {
          console.log(`    Node: ${src.node ? src.node.selector : 'null'} (${src.node ? src.node.snippet.slice(0, 40) : ''})`);
          console.log(`    Prev:`, src.previousRect, `Curr:`, src.currentRect);
        }
      }
    } catch (err) {
      console.error(`Error measuring ${r.name}:`, err.message);
    }
  }

  for (const r of mobileRoutes) {
    try {
      const res = await measureRoute(browser, r, mobileVp);
      results.push(res);
      console.log(`Result ${r.name}: CLS = ${res.totalCls}, LCP = ${res.lcp}ms, Shifts count = ${res.clsEntries.length}`);
      for (const entry of res.clsEntries) {
        console.log(`  Shift value: ${entry.value.toFixed(4)} at ${entry.startTime}ms`);
        for (const src of entry.sources) {
          console.log(`    Node: ${src.node ? src.node.selector : 'null'} (${src.node ? src.node.snippet.slice(0, 40) : ''})`);
          console.log(`    Prev:`, src.previousRect, `Curr:`, src.currentRect);
        }
      }
      if (res.lcpEntries.length > 0) {
        const lastLcp = res.lcpEntries[res.lcpEntries.length - 1];
        console.log(`  LCP Element:`, lastLcp.element?.selector, `Snippet:`, lastLcp.element?.snippet);
      }
    } catch (err) {
      console.error(`Error measuring ${r.name}:`, err.message);
    }
  }

  fs.writeFileSync(path.join(DATA_DIR, 'cls-attribution.json'), JSON.stringify(results, null, 2), 'utf-8');
  console.log(`\nSaved attribution to ${path.join(DATA_DIR, 'cls-attribution.json')}`);

  await browser.close();
})();
