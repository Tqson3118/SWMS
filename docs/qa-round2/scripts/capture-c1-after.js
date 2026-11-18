const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/c1-after');
const DATA_DIR = path.resolve(__dirname, '../data/c1-perf');

const VIEWPORTS = [
  { name: '1440x900', width: 1440, height: 900 },
  { name: '768x1024', width: 768, height: 1024 },
  { name: '390x844', width: 390, height: 844, isMobile: true }
];

const THEMES = ['light', 'dark'];

const visualChecks = {
  timestamp: new Date().toISOString(),
  overflows: [],
  narrowColumns: [],
  consoleErrors: [],
  pageErrors: [],
  totalScreenshots: 0,
  capturedFiles: []
};

function setupPageDiagnostics(page, contextName) {
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      const text = msg.text();
      // Ignore known favicon / benign dev logs if any
      visualChecks.consoleErrors.push({
        context: contextName,
        url: page.url(),
        text: text.slice(0, 300)
      });
    }
  });

  page.on('pageerror', (err) => {
    visualChecks.pageErrors.push({
      context: contextName,
      url: page.url(),
      message: err.message,
      stack: (err.stack || '').slice(0, 500)
    });
  });
}

async function checkHorizontalOverflow(page, filename, vp, theme) {
  try {
    const overflow = await page.evaluate(() => {
      const docW = document.documentElement.scrollWidth;
      const winW = window.innerWidth;
      const bodyW = document.body ? document.body.scrollWidth : 0;
      const maxW = Math.max(docW, bodyW);
      return {
        hasOverflow: maxW > winW + 1,
        docW,
        bodyW,
        winW,
        diff: maxW - winW
      };
    });

    if (overflow && overflow.hasOverflow) {
      visualChecks.overflows.push({
        filename,
        url: page.url(),
        viewport: vp.name,
        theme,
        ...overflow
      });
      console.warn(`[OVERFLOW DETECTED] ${filename}: scrollWidth=${Math.max(overflow.docW, overflow.bodyW)} > innerWidth=${overflow.winW} (+${overflow.diff}px)`);
    }
  } catch (err) {
    // evaluate might fail if page is navigating
  }
}

async function checkLessonColumnWidth(page, filename, vp, theme) {
  try {
    const colInfo = await page.evaluate(() => {
      const col = document.querySelector('[data-testid="reading-prose-column"]') ||
                  document.querySelector('[data-swms-scroll-container="learn-page"]') ||
                  document.querySelector('article');
      if (!col) return null;
      const rect = col.getBoundingClientRect();
      return {
        width: rect.width,
        isUnder320: rect.width < 320
      };
    });

    if (colInfo && colInfo.isUnder320) {
      visualChecks.narrowColumns.push({
        filename,
        url: page.url(),
        viewport: vp.name,
        theme,
        width: colInfo.width
      });
      console.warn(`[NARROW COLUMN] ${filename}: width = ${colInfo.width}px (< 320px)`);
    }
  } catch (err) {}
}

async function applyTheme(page, theme) {
  await page.evaluate((t) => {
    localStorage.setItem('swms_theme', t);
    if (t === 'dark') {
      document.documentElement.classList.add('dark');
      document.body?.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
      document.body?.classList.remove('dark');
    }
    window.dispatchEvent(new StorageEvent('storage', { key: 'swms_theme', newValue: t }));
  }, theme);
  await page.waitForTimeout(300);
}

async function loginUser(page, type = 'default') {
  await page.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await page.waitForSelector('[data-testid="demo-switcher-toggle"]', { timeout: 10000 });
  
  const switcherVisible = await page.isVisible('[data-testid="switch-user-default"]');
  if (!switcherVisible) {
    await page.click('[data-testid="demo-switcher-toggle"]');
    await page.waitForSelector('[data-testid="switch-user-default"]', { timeout: 5000 });
  }

  if (type === 'default') {
    await page.click('[data-testid="switch-user-default"]');
  } else {
    await page.click('[data-testid="switch-user-secondary"]');
  }

  await page.waitForURL('**/app**', { timeout: 15000 });
  await page.waitForTimeout(800);
}

async function safeScreenshot(page, filename, vp, theme, isLessonDrawer = false) {
  const filePath = path.join(OUT_DIR, filename);
  try {
    await checkHorizontalOverflow(page, filename, vp, theme);
    if (isLessonDrawer) {
      await checkLessonColumnWidth(page, filename, vp, theme);
    }
    await page.screenshot({ path: filePath, fullPage: true });
    visualChecks.totalScreenshots++;
    visualChecks.capturedFiles.push(filename);
    console.log(`Saved: ${filename}`);
  } catch (err) {
    console.error(`Failed to screenshot ${filename}:`, err.message);
  }
}

async function run() {
  if (!fs.existsSync(OUT_DIR)) {
    fs.mkdirSync(OUT_DIR, { recursive: true });
  }
  if (!fs.existsSync(DATA_DIR)) {
    fs.mkdirSync(DATA_DIR, { recursive: true });
  }

  const browser = await chromium.launch({ headless: true });

  for (const vp of VIEWPORTS) {
    for (const theme of THEMES) {
      console.log(`\n========================================`);
      console.log(`Starting Viewport: ${vp.name} | Theme: ${theme}`);
      console.log(`========================================`);

      const context = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        isMobile: vp.isMobile || false
      });

      // 1. PUBLIC ROUTES
      const pubPage = await context.newPage();
      setupPageDiagnostics(pubPage, `public-${vp.name}-${theme}`);
      
      // Landing
      await pubPage.goto(`${BASE_URL}/`, { waitUntil: 'domcontentloaded' });
      await applyTheme(pubPage, theme);
      await pubPage.waitForTimeout(600);
      await safeScreenshot(pubPage, `landing-${vp.name}-${theme}.png`, vp, theme);

      // Auth (Login)
      await pubPage.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
      await applyTheme(pubPage, theme);
      await pubPage.waitForTimeout(600);
      await safeScreenshot(pubPage, `auth-login-${vp.name}-${theme}.png`, vp, theme);

      // Auth (Register)
      await pubPage.goto(`${BASE_URL}/auth?tab=register`, { waitUntil: 'domcontentloaded' });
      await applyTheme(pubPage, theme);
      await pubPage.waitForTimeout(600);
      await safeScreenshot(pubPage, `auth-register-${vp.name}-${theme}.png`, vp, theme);

      // Auth (Demo Switcher open)
      await pubPage.click('[data-testid="demo-switcher-toggle"]');
      await pubPage.waitForSelector('[data-testid="demo-user-switcher"]', { timeout: 3000 });
      await pubPage.waitForTimeout(300);
      await safeScreenshot(pubPage, `auth-demo-switcher-${vp.name}-${theme}.png`, vp, theme);

      await pubPage.close();

      // 2. SON TRAN (AUTHENTICATED WITH DATA)
      const userContext = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        isMobile: vp.isMobile || false
      });
      const page = await userContext.newPage();
      setupPageDiagnostics(page, `sontran-${vp.name}-${theme}`);

      console.log(`Logging in as Son Tran (${vp.name}, ${theme})...`);
      await loginUser(page, 'default');
      await applyTheme(page, theme);

      // Dashboard
      await page.goto(`${BASE_URL}/app`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(800);
      await safeScreenshot(page, `app-dashboard-${vp.name}-${theme}.png`, vp, theme);

      // Command Palette (Ctrl+K)
      try {
        await page.keyboard.press('Control+K');
        await page.waitForSelector('[data-testid="command-palette-backdrop"], [role="dialog"]', { timeout: 2000 });
        await page.waitForTimeout(400);
        await safeScreenshot(page, `state-command-palette-${vp.name}-${theme}.png`, vp, theme);
        await page.keyboard.press('Escape');
        await page.waitForTimeout(300);
      } catch (e) {
        console.log('Command palette trigger fallback:', e.message);
      }

      // Courses List
      await page.goto(`${BASE_URL}/app/courses`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-courses-${vp.name}-${theme}.png`, vp, theme);

      // Create Course Page
      await page.goto(`${BASE_URL}/app/courses/new`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-courses-new-${vp.name}-${theme}.png`, vp, theme);

      // Course Detail (OS Course)
      const courseId = '22222222-2222-4222-8222-222222222222';
      await page.goto(`${BASE_URL}/app/courses/${courseId}`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-course-detail-${vp.name}-${theme}.png`, vp, theme);

      // Course Edit
      await page.goto(`${BASE_URL}/app/courses/${courseId}/edit`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-course-edit-${vp.name}-${theme}.png`, vp, theme);

      // Course Learn (Lesson Player)
      await page.goto(`${BASE_URL}/app/courses/${courseId}/learn`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(1000);
      await safeScreenshot(page, `app-course-learn-${vp.name}-${theme}.png`, vp, theme);

      // Learn Drawer ("Sổ tay & Thẻ")
      try {
        const drawerToggle = page.locator('button:has-text("Sổ tay & Thẻ"), button:has-text("Sổ tay")').first();
        if (await drawerToggle.isVisible()) {
          await drawerToggle.click();
          await page.waitForTimeout(600);
          await safeScreenshot(page, `state-learn-drawer-${vp.name}-${theme}.png`, vp, theme, true);
        }
      } catch (e) {
        console.log('Could not toggle learn drawer:', e.message);
      }

      // Course Quiz
      await page.goto(`${BASE_URL}/app/courses/${courseId}/quiz`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(800);
      await safeScreenshot(page, `app-course-quiz-${vp.name}-${theme}.png`, vp, theme);

      // Review Page (FSRS Flashcards)
      await page.goto(`${BASE_URL}/app/review`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(800);
      await safeScreenshot(page, `app-review-${vp.name}-${theme}.png`, vp, theme);

      // Review Card Flipped (Show Answer)
      try {
        const showAnswerBtn = page.locator('button:has-text("Hiện đáp án"), button:has-text("Xem đáp án"), button:has-text("Lật thẻ")').first();
        if (await showAnswerBtn.isVisible()) {
          await showAnswerBtn.click();
          await page.waitForTimeout(600);
          await safeScreenshot(page, `state-review-card-flipped-${vp.name}-${theme}.png`, vp, theme);
        } else {
          await page.keyboard.press('Space');
          await page.waitForTimeout(600);
          await safeScreenshot(page, `state-review-card-flipped-${vp.name}-${theme}.png`, vp, theme);
        }
      } catch (e) {
        console.log('Could not flip review card:', e.message);
      }

      // Documents
      await page.goto(`${BASE_URL}/app/documents`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-documents-${vp.name}-${theme}.png`, vp, theme);

      // Library
      await page.goto(`${BASE_URL}/app/library`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-library-${vp.name}-${theme}.png`, vp, theme);

      // Notes
      await page.goto(`${BASE_URL}/app/notes`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-notes-${vp.name}-${theme}.png`, vp, theme);

      // Settings
      await page.goto(`${BASE_URL}/app/settings`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-settings-${vp.name}-${theme}.png`, vp, theme);

      // Studio
      await page.goto(`${BASE_URL}/app/studio`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-studio-${vp.name}-${theme}.png`, vp, theme);

      // Studio Create
      await page.goto(`${BASE_URL}/app/studio/create`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-studio-create-${vp.name}-${theme}.png`, vp, theme);

      // Studio Detail
      await page.goto(`${BASE_URL}/app/studio/${courseId}`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(600);
      await safeScreenshot(page, `app-studio-detail-${vp.name}-${theme}.png`, vp, theme);

      // Workspace Classic
      await page.goto(`${BASE_URL}/workspace`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(page, theme);
      await page.waitForTimeout(800);
      await safeScreenshot(page, `workspace-classic-${vp.name}-${theme}.png`, vp, theme);

      // Modal Create Course (inside /workspace)
      try {
        const createBtn = page.locator('button:has-text("Tạo khóa học mới"), button:has-text("+ Tạo khóa học")').first();
        if (await createBtn.isVisible()) {
          await createBtn.click();
          await page.waitForSelector('[data-testid="course-create-modal"]', { timeout: 3000 });
          await page.waitForTimeout(500);
          await safeScreenshot(page, `state-modal-create-course-${vp.name}-${theme}.png`, vp, theme);
          const closeBtn = page.locator('[data-testid="course-create-modal-close-btn"]');
          if (await closeBtn.isVisible()) await closeBtn.click();
        }
      } catch (e) {
        console.log('Could not open CourseCreateModal:', e.message);
      }

      await page.close();
      await userContext.close();

      // 3. MINH ANH (EMPTY STATES)
      const emptyContext = await browser.newContext({
        viewport: { width: vp.width, height: vp.height },
        isMobile: vp.isMobile || false
      });
      const emptyPage = await emptyContext.newPage();
      setupPageDiagnostics(emptyPage, `minhanh-${vp.name}-${theme}`);

      console.log(`Logging in as Minh Anh (${vp.name}, ${theme}) for empty states...`);
      await loginUser(emptyPage, 'secondary');
      await applyTheme(emptyPage, theme);

      // Empty Dashboard
      await emptyPage.goto(`${BASE_URL}/app`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-dashboard-${vp.name}-${theme}.png`, vp, theme);

      // Empty Courses
      await emptyPage.goto(`${BASE_URL}/app/courses`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-courses-${vp.name}-${theme}.png`, vp, theme);

      // Empty Documents
      await emptyPage.goto(`${BASE_URL}/app/documents`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-documents-${vp.name}-${theme}.png`, vp, theme);

      // Empty Notes
      await emptyPage.goto(`${BASE_URL}/app/notes`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-notes-${vp.name}-${theme}.png`, vp, theme);

      // Empty Review
      await emptyPage.goto(`${BASE_URL}/app/review`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-review-${vp.name}-${theme}.png`, vp, theme);

      // Empty Workspace
      await emptyPage.goto(`${BASE_URL}/workspace`, { waitUntil: 'networkidle' }).catch(() => {});
      await applyTheme(emptyPage, theme);
      await emptyPage.waitForTimeout(600);
      await safeScreenshot(emptyPage, `empty-workspace-${vp.name}-${theme}.png`, vp, theme);

      await emptyPage.close();
      await emptyContext.close();
    }
  }

  // Also capture ielts-leak-readme (1440x900 light + dark) for 1:1 parity with c0-before
  console.log('\nCapturing ielts-leak-readme screenshots...');
  const ieltsCtx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  const ip = await ieltsCtx.newPage();
  setupPageDiagnostics(ip, 'ielts-leak-check');
  await loginUser(ip, 'default');
  const ieltsCourseId = '79B30ACD-0BD5-41C8-8744-271AD7B496CE';
  const ieltsLessonId = '0360C033-EBC7-453D-9FAE-C812D3BCB48A';
  await ip.goto(`${BASE_URL}/app/courses/${ieltsCourseId}/learn?lesson=${ieltsLessonId}`, { waitUntil: 'networkidle' }).catch(() => {});
  await ip.waitForTimeout(1000);

  // Light
  await applyTheme(ip, 'light');
  await ip.waitForTimeout(500);
  await safeScreenshot(ip, 'ielts-leak-readme-1440x900-light.png', { name: '1440x900' }, 'light');

  // Dark
  await applyTheme(ip, 'dark');
  await ip.waitForTimeout(500);
  await safeScreenshot(ip, 'ielts-leak-readme-1440x900-dark.png', { name: '1440x900' }, 'dark');

  await ip.close();
  await ieltsCtx.close();
  await browser.close();

  // Save visual checks JSON
  const visualChecksPath = path.join(DATA_DIR, 'visual-checks.json');
  fs.writeFileSync(visualChecksPath, JSON.stringify(visualChecks, null, 2), 'utf8');

  console.log(`\n========================================`);
  console.log(`All done! Generated ${visualChecks.totalScreenshots} screenshots in ${OUT_DIR}`);
  console.log(`Visual checks saved to: ${visualChecksPath}`);
  console.log(`Overflows detected: ${visualChecks.overflows.length}`);
  console.log(`Narrow columns detected: ${visualChecks.narrowColumns.length}`);
  console.log(`Console errors recorded: ${visualChecks.consoleErrors.length}`);
  console.log(`Page errors recorded: ${visualChecks.pageErrors.length}`);
  console.log(`========================================`);
}

run().catch((err) => {
  console.error('Fatal execution error:', err);
  process.exit(1);
});
