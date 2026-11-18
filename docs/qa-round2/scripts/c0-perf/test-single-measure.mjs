import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

async function testMobile() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 390, height: 844 },
    userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
    isMobile: true,
    hasTouch: true,
  });

  const page = await context.newPage();
  const cdp = await context.newCDPSession(page);
  await cdp.send('Emulation.setCPUThrottlingRate', { rate: 4 });

  await page.addInitScript(() => {
    window.__perfMetrics = {
      lcp: 0,
      cls: 0,
      tbt: 0,
      totalLongTaskMs: 0,
      longTaskCount: 0,
    };
    new PerformanceObserver((entryList) => {
      const entries = entryList.getEntries();
      if (entries.length > 0) {
        window.__perfMetrics.lcp = entries[entries.length - 1].startTime;
      }
    }).observe({ type: 'largest-contentful-paint', buffered: true });

    new PerformanceObserver((entryList) => {
      for (const entry of entryList.getEntries()) {
        if (!entry.hadRecentInput) window.__perfMetrics.cls += entry.value;
      }
    }).observe({ type: 'layout-shift', buffered: true });

    new PerformanceObserver((entryList) => {
      for (const entry of entryList.getEntries()) {
        window.__perfMetrics.totalLongTaskMs += entry.duration;
        window.__perfMetrics.longTaskCount++;
        if (entry.duration > 50) window.__perfMetrics.tbt += (entry.duration - 50);
      }
    }).observe({ type: 'longtask', buffered: true });
  });

  console.log('Navigating to http://127.0.0.1:3000/ with 4x CPU throttle...');
  await page.goto('http://127.0.0.1:3000/', { waitUntil: 'load' });
  await page.waitForTimeout(2000);

  const navTiming = await page.evaluate(() => {
    const nav = performance.getEntriesByType('navigation')[0];
    const paint = performance.getEntriesByType('paint');
    const fcp = paint.find(p => p.name === 'first-contentful-paint')?.startTime || 0;
    return {
      ttfb: nav ? nav.responseStart - nav.requestStart : 0,
      fcp,
      perfMetrics: window.__perfMetrics,
    };
  });

  console.log('Mobile 4x Results:');
  console.log('TTFB:', navTiming.ttfb.toFixed(1), 'ms');
  console.log('FCP:', navTiming.fcp.toFixed(1), 'ms');
  console.log('LCP:', navTiming.perfMetrics.lcp.toFixed(1), 'ms');
  console.log('CLS:', navTiming.perfMetrics.cls.toFixed(4));
  console.log('TBT:', navTiming.perfMetrics.tbt.toFixed(1), 'ms');
  console.log('Total Long Tasks:', navTiming.perfMetrics.totalLongTaskMs.toFixed(1), 'ms (', navTiming.perfMetrics.longTaskCount, 'tasks)');

  await browser.close();
}

testMobile().catch(console.error);
