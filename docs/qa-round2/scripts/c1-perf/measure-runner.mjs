import fs from 'node:fs';
import path from 'node:path';
import pkg from '../../../../frontend/node_modules/playwright/index.js';
const { chromium } = pkg;

const BASE_URL = 'http://127.0.0.1:3000';
const COURSE_ID = '25f79a81-8ac2-46ae-99e1-85975ff42a8b';

const TARGET_ROUTES = [
  { name: 'landing', path: '/', isAuth: false, mainSelector: 'main, [data-testid="hero-heading"], h1' },
  { name: 'auth', path: '/auth', isAuth: false, mainSelector: 'main, form, [data-testid="demo-switcher-toggle"]' },
  { name: 'app-dashboard', path: '/app', isAuth: true, mainSelector: 'main, [data-testid="dashboard-container"], h1' },
  { name: 'app-courses', path: '/app/courses', isAuth: true, mainSelector: 'main, [data-testid="course-card"], h1, .grid' },
  { name: 'course-detail', path: `/app/courses/${COURSE_ID}`, isAuth: true, mainSelector: 'main, h1, [data-testid="course-header"]' },
  { name: 'course-learn', path: `/app/courses/${COURSE_ID}/learn`, isAuth: true, mainSelector: 'main, [data-testid="lesson-content"], .prose, article' },
  { name: 'course-quiz', path: `/app/courses/${COURSE_ID}/quiz`, isAuth: true, mainSelector: 'main, [data-testid="quiz-container"], h1' },
  { name: 'review', path: '/app/review', isAuth: true, mainSelector: 'main, [data-testid="flashcard-deck"], h1, .card' },
  { name: 'documents', path: '/app/documents', isAuth: true, mainSelector: 'main, [data-testid="document-list"], h1, table' },
  { name: 'notes', path: '/app/notes', isAuth: true, mainSelector: 'main, [data-testid="note-editor"], h1, textarea' },
  { name: 'settings', path: '/app/settings', isAuth: true, mainSelector: 'main, form, h1' },
  { name: 'workspace', path: '/workspace', isAuth: true, mainSelector: 'main, [data-testid="workspace-root"], .h-screen' },
];

const VIEWPORTS = [
  {
    mode: 'desktop',
    width: 1440,
    height: 900,
    isMobile: false,
    hasTouch: false,
    cpuThrottle: 1,
    deviceScaleFactor: 1,
  },
  {
    mode: 'mobile',
    width: 390,
    height: 844,
    isMobile: true,
    hasTouch: true,
    cpuThrottle: 4,
    deviceScaleFactor: 3,
    userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
  },
];

const NUM_RUNS = 3;
const SCREENSHOT_DIR = path.resolve('docs/qa-round2/screenshots/c1-perf');
const DATA_DIR = path.resolve('docs/qa-round2/data/c1-perf');

function median(values) {
  if (!values || values.length === 0) return 0;
  const sorted = [...values].sort((a, b) => a - b);
  const mid = Math.floor(sorted.length / 2);
  return sorted.length % 2 !== 0 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
}

async function loginAsSonTran(context) {
  const page = await context.newPage();
  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'load' });
  await page.click('[data-testid="demo-switcher-toggle"]');
  await Promise.all([
    page.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    page.click('[data-testid="switch-user-default"]'),
  ]);
  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForLoadState('networkidle');
  await page.close();
}

async function measureRouteRun(context, routeDef, vp, runIdx) {
  const page = await context.newPage();
  const cdp = await context.newCDPSession(page);

  await cdp.send('Performance.enable');
  await cdp.send('Network.enable');

  if (vp.cpuThrottle > 1) {
    await cdp.send('Emulation.setCPUThrottlingRate', { rate: vp.cpuThrottle });
  }

  // Network tracking
  const networkRequests = [];
  const apiRequests = [];
  const scriptResponses = [];
  const cdpWireBytesMap = new Map();

  cdp.on('Network.loadingFinished', params => {
    cdpWireBytesMap.set(params.requestId, params.encodedDataLength);
  });

  page.on('request', req => {
    req._startTime = performance.now();
  });

  page.on('response', async res => {
    const req = res.request();
    const duration = req._startTime ? performance.now() - req._startTime : 0;
    const url = res.url();
    const status = res.status();
    const resourceType = req.resourceType();

    let bodyBytes = 0;
    try {
      const buf = await res.body();
      bodyBytes = buf.length;
    } catch {}

    networkRequests.push({
      url,
      status,
      resourceType,
      durationMs: duration,
      uncompressedBytes: bodyBytes,
    });

    if (url.includes('/api/v1/')) {
      const parsedUrl = new URL(url);
      apiRequests.push({
        method: req.method(),
        path: parsedUrl.pathname,
        url,
        status,
        durationMs: duration,
        bodyBytes,
      });
    }

    if (resourceType === 'script' || url.endsWith('.js') || url.includes('/_next/static/chunks/')) {
      scriptResponses.push({
        url,
        uncompressedBytes: bodyBytes,
      });
    }
  });

  // Inject Web Vitals observers
  await page.addInitScript(() => {
    window.__perfMetrics = {
      lcp: 0,
      cls: 0,
      tbt: 0,
      totalLongTaskMs: 0,
      longTaskCount: 0,
      inp: 0,
    };

    // LCP
    try {
      new PerformanceObserver(entryList => {
        const entries = entryList.getEntries();
        if (entries.length > 0) {
          window.__perfMetrics.lcp = entries[entries.length - 1].startTime;
        }
      }).observe({ type: 'largest-contentful-paint', buffered: true });
    } catch {}

    // CLS
    try {
      new PerformanceObserver(entryList => {
        for (const entry of entryList.getEntries()) {
          if (!entry.hadRecentInput) {
            window.__perfMetrics.cls += entry.value;
          }
        }
      }).observe({ type: 'layout-shift', buffered: true });
    } catch {}

    // Long Tasks & TBT
    try {
      new PerformanceObserver(entryList => {
        for (const entry of entryList.getEntries()) {
          window.__perfMetrics.totalLongTaskMs += entry.duration;
          window.__perfMetrics.longTaskCount++;
          if (entry.duration > 50) {
            window.__perfMetrics.tbt += (entry.duration - 50);
          }
        }
      }).observe({ type: 'longtask', buffered: true });
    } catch {}

    // Event Timing / INP
    try {
      new PerformanceObserver(entryList => {
        for (const entry of entryList.getEntries()) {
          if (entry.duration > window.__perfMetrics.inp) {
            window.__perfMetrics.inp = entry.duration;
          }
        }
      }).observe({ type: 'first-input', buffered: true });
    } catch {}
  });

  const fullUrl = `${BASE_URL}${routeDef.path}`;
  const navStart = performance.now();

  let navResponse;
  try {
    navResponse = await page.goto(fullUrl, { waitUntil: 'load', timeout: 30000 });
  } catch (err) {
    console.error(`Error loading ${fullUrl}:`, err.message);
  }

  // Time to main content
  let timeToMainContent = 0;
  try {
    const mainEl = await page.waitForSelector(routeDef.mainSelector, { state: 'visible', timeout: 8000 });
    timeToMainContent = performance.now() - navStart;
  } catch {
    timeToMainContent = performance.now() - navStart;
  }

  // Allow layout and long tasks to settle
  await page.waitForTimeout(2000);

  // Extract browser performance timings
  const navTiming = await page.evaluate(() => {
    const nav = performance.getEntriesByType('navigation')[0];
    const paint = performance.getEntriesByType('paint');
    const fcp = paint.find(p => p.name === 'first-contentful-paint')?.startTime || 0;
    const fp = paint.find(p => p.name === 'first-paint')?.startTime || 0;
    return {
      ttfb: nav ? nav.responseStart - nav.requestStart : 0,
      domContentLoaded: nav ? nav.domContentLoadedEventEnd : 0,
      load: nav ? nav.loadEventEnd : 0,
      fcp,
      fp,
      metrics: window.__perfMetrics,
    };
  });

  // Extract CDP metrics
  const cdpMetricsRaw = await cdp.send('Performance.getMetrics');
  const cdpMap = {};
  for (const m of cdpMetricsRaw.metrics) {
    cdpMap[m.name] = m.value;
  }

  // Calculate JS transfer
  const totalJsUncompressedBytes = scriptResponses.reduce((acc, s) => acc + s.uncompressedBytes, 0);
  const totalJsChunks = scriptResponses.length;

  // Screenshot on run 1
  let screenshotFile = null;
  if (runIdx === 1) {
    screenshotFile = path.join(SCREENSHOT_DIR, `${routeDef.name}-${vp.mode}.png`);
    await page.screenshot({ path: screenshotFile, fullPage: false });
  }

  const runResult = {
    run: runIdx,
    route: routeDef.path,
    routeName: routeDef.name,
    viewport: vp.mode,
    status: navResponse ? navResponse.status() : 0,
    ttfb: +(navTiming.ttfb || 0).toFixed(2),
    fcp: +(navTiming.fcp || 0).toFixed(2),
    lcp: +(navTiming.metrics?.lcp || navTiming.fcp || 0).toFixed(2),
    cls: +(navTiming.metrics?.cls || 0).toFixed(4),
    tbt: +(navTiming.metrics?.tbt || 0).toFixed(2),
    totalLongTasksMs: +(navTiming.metrics?.totalLongTaskMs || 0).toFixed(2),
    longTaskCount: navTiming.metrics?.longTaskCount || 0,
    inp: +(navTiming.metrics?.inp || 0).toFixed(2),
    domContentLoaded: +(navTiming.domContentLoaded || 0).toFixed(2),
    timeToMainContent: +timeToMainContent.toFixed(2),
    totalRequests: networkRequests.length,
    totalJsUncompressedKB: +(totalJsUncompressedBytes / 1024).toFixed(2),
    totalJsChunks,
    jsHeapUsedMB: +((cdpMap.JSHeapUsedSize || 0) / (1024 * 1024)).toFixed(2),
    taskDurationMs: +((cdpMap.TaskDuration || 0) * 1000).toFixed(2),
    apiCalls: apiRequests,
    screenshot: screenshotFile,
  };

  await page.close();
  return runResult;
}

async function runBenchmark() {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  fs.mkdirSync(DATA_DIR, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const allRuns = [];
  const routeMedians = [];
  const allApiCalls = [];

  console.log('=== STARTING STUDXY C1-PERF BENCHMARK ===');
  console.log(`Routes: ${TARGET_ROUTES.length} | Viewports: ${VIEWPORTS.length} | Runs per route: ${NUM_RUNS}`);

  for (const vp of VIEWPORTS) {
    console.log(`\n========================================`);
    console.log(`>>> VIEWPORT: ${vp.mode.toUpperCase()} (${vp.width}x${vp.height}, CPU throttle: ${vp.cpuThrottle}x)`);
    console.log(`========================================`);

    // Create fresh context for this viewport mode
    const contextOptions = {
      viewport: { width: vp.width, height: vp.height },
      isMobile: vp.isMobile,
      hasTouch: vp.hasTouch,
      deviceScaleFactor: vp.deviceScaleFactor,
    };
    if (vp.userAgent) contextOptions.userAgent = vp.userAgent;

    const context = await browser.newContext(contextOptions);

    // Login once for the authenticated session
    console.log(`Authenticating demo user "Sơn Trần" for ${vp.mode}...`);
    await loginAsSonTran(context);
    console.log(`Authenticated successfully.`);

    for (const routeDef of TARGET_ROUTES) {
      console.log(`\nMeasuring [${routeDef.name}] (${routeDef.path})...`);
      const runsForRoute = [];

      for (let r = 1; r <= NUM_RUNS; r++) {
        const res = await measureRouteRun(context, routeDef, vp, r);
        runsForRoute.push(res);
        allRuns.push(res);
        allApiCalls.push(...res.apiCalls);
        console.log(
          `  Run ${r}: LCP=${res.lcp}ms | FCP=${res.fcp}ms | TTFB=${res.ttfb}ms | CLS=${res.cls} | TBT=${res.tbt}ms | LongTasks=${res.totalLongTasksMs}ms | JS=${res.totalJsUncompressedKB}KB | Reqs=${res.totalRequests} | MainReady=${res.timeToMainContent}ms`
        );
      }

      // Calculate median for this route & viewport
      const medianObj = {
        routeName: routeDef.name,
        route: routeDef.path,
        viewport: vp.mode,
        isAuth: routeDef.isAuth,
        lcp: +median(runsForRoute.map(x => x.lcp)).toFixed(2),
        cls: +median(runsForRoute.map(x => x.cls)).toFixed(4),
        tbt: +median(runsForRoute.map(x => x.tbt)).toFixed(2),
        totalLongTasksMs: +median(runsForRoute.map(x => x.totalLongTasksMs)).toFixed(2),
        longTaskCount: Math.round(median(runsForRoute.map(x => x.longTaskCount))),
        fcp: +median(runsForRoute.map(x => x.fcp)).toFixed(2),
        ttfb: +median(runsForRoute.map(x => x.ttfb)).toFixed(2),
        timeToMainContent: +median(runsForRoute.map(x => x.timeToMainContent)).toFixed(2),
        totalJsKB: +median(runsForRoute.map(x => x.totalJsUncompressedKB)).toFixed(2),
        totalRequests: Math.round(median(runsForRoute.map(x => x.totalRequests))),
        jsHeapUsedMB: +median(runsForRoute.map(x => x.jsHeapUsedMB)).toFixed(2),
        taskDurationMs: +median(runsForRoute.map(x => x.taskDurationMs)).toFixed(2),
      };

      routeMedians.push(medianObj);
      console.log(
        `  -> MEDIAN [${routeDef.name} ${vp.mode}]: LCP=${medianObj.lcp}ms | FCP=${medianObj.fcp}ms | TTFB=${medianObj.ttfb}ms | CLS=${medianObj.cls} | TBT=${medianObj.tbt}ms | JS=${medianObj.totalJsKB}KB | Main=${medianObj.timeToMainContent}ms`
      );
    }

    await context.close();
  }

  await browser.close();

  // Aggregate API Latency Stats
  const apiMap = new Map();
  for (const call of allApiCalls) {
    const key = `${call.method} ${call.path}`;
    if (!apiMap.has(key)) {
      apiMap.set(key, []);
    }
    apiMap.get(key).push(call.durationMs);
  }

  const apiStats = [];
  for (const [endpoint, durations] of apiMap.entries()) {
    apiStats.push({
      endpoint,
      callCount: durations.length,
      medianDurationMs: +median(durations).toFixed(2),
      minDurationMs: +Math.min(...durations).toFixed(2),
      maxDurationMs: +Math.max(...durations).toFixed(2),
      avgDurationMs: +(durations.reduce((a, b) => a + b, 0) / durations.length).toFixed(2),
    });
  }
  apiStats.sort((a, b) => b.medianDurationMs - a.medianDurationMs);

  // Save Raw Data
  const rawDataPath = path.join(DATA_DIR, 'raw-runs.json');
  fs.writeFileSync(rawDataPath, JSON.stringify(allRuns, null, 2), 'utf8');

  const summaryDataPath = path.join(DATA_DIR, 'summary-medians.json');
  fs.writeFileSync(summaryDataPath, JSON.stringify(routeMedians, null, 2), 'utf8');

  const apiDataPath = path.join(DATA_DIR, 'api-latencies.json');
  fs.writeFileSync(apiDataPath, JSON.stringify(apiStats, null, 2), 'utf8');

  console.log('\n=== BENCHMARK COMPLETE ===');
  console.log(`Saved raw runs to: ${rawDataPath}`);
  console.log(`Saved medians to: ${summaryDataPath}`);
  console.log(`Saved API latencies to: ${apiDataPath}`);

  return { allRuns, routeMedians, apiStats };
}

runBenchmark().catch(err => {
  console.error('Benchmark failed:', err);
  process.exit(1);
});
