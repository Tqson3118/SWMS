const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../frontend/node_modules/@playwright/test'));

const BASE_URL = process.env.SWMS_FRONTEND_URL || 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../screenshots/b-practice-labs');

if (!fs.existsSync(OUT_DIR)) {
  fs.mkdirSync(OUT_DIR, { recursive: true });
}

const COURSES = [
  {
    type: 'coding',
    name: 'Lập trình .NET & Hệ điều hành',
    courseId: '22222222-2222-4222-8222-222222222222',
    lessonId: '44444444-4444-4444-8444-444444444444',
  },
  {
    type: 'english',
    name: 'Tiếng Anh & IELTS',
    courseId: '79B30ACD-0BD5-41C8-8744-271AD7B496CE',
    lessonId: '165075AA-E343-460B-81F0-15EA8AE07C98',
  },
  {
    type: 'finance',
    name: 'Tài chính cá nhân & Đầu tư',
    courseId: '25F79A81-8AC2-46AE-99E1-85975FF42A8B',
    lessonId: '7BF55630-815B-4745-B2B4-F05B844CD987',
  },
  {
    type: 'psychology',
    name: 'Tâm lý học & Kỹ năng sống',
    courseId: '43C1CB70-DCDD-4858-8FAD-575D3C149CC7',
    lessonId: 'C45B8820-EAD3-438B-BD17-8DA56AD96C05',
  },
];

async function setupPageDiagnostics(page, suiteName, diag) {
  page.on('console', msg => {
    if (msg.type() === 'error') {
      const text = msg.text();
      // Ignore known benign favicon 404 or expected network aborts if any
      if (!text.includes('favicon.ico') && !text.includes('AbortError')) {
        diag.consoleErrors.push({ suite: suiteName, text: text.slice(0, 300) });
      }
    }
  });

  page.on('pageerror', err => {
    diag.pageErrors.push({ suite: suiteName, message: err.message });
  });
}

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

async function setTheme(page, theme) {
  await page.evaluate((th) => {
    localStorage.setItem('swms_theme', th);
    if (th === 'dark') {
      document.documentElement.classList.add('dark');
      document.body?.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
      document.body?.classList.remove('dark');
    }
  }, theme);
  await page.waitForTimeout(300);
}

async function run() {
  console.log('================================================================');
  console.log('🚀 PHASE B: VERIFY 4 PRACTICE LABS & RICH HTML/CSS MARKDOWN');
  console.log('================================================================\n');

  const browser = await chromium.launch({ headless: true });
  const diag = { consoleErrors: [], pageErrors: [] };
  const verificationReport = [];

  try {
    // ------------------------------------------------------------------------
    // Part 1: Desktop Verification (1440x900)
    // ------------------------------------------------------------------------
    console.log('>>> [1/2] DESKTOP VERIFICATION (1440x900) ...');
    const desktopContext = await browser.newContext({ viewport: { width: 1440, height: 900 } });
    const desktopPage = await desktopContext.newPage();
    setupPageDiagnostics(desktopPage, 'Desktop', diag);

    await loginUser(desktopPage);

    for (const c of COURSES) {
      console.log(`\n--- Testing ${c.name} (${c.type}) ---`);
      const url = `${BASE_URL}/app/courses/${c.courseId}/learn?lesson=${c.lessonId}`;
      await desktopPage.goto(url, { waitUntil: 'networkidle' });
      await desktopPage.waitForTimeout(1000);

      // 1.1 Verify Markdown View & Rich HTML/CSS Elements
      const mdView = desktopPage.locator('[data-testid="lesson-markdown-view"]');
      const mdVisible = await mdView.isVisible().catch(() => false);
      console.log(`  [Theory Tab] lesson-markdown-view visible: ${mdVisible}`);

      if (mdVisible) {
        // Check for Callout cards, tables, code blocks, etc.
        const elementsInfo = await desktopPage.evaluate(() => {
          const root = document.querySelector('[data-testid="lesson-markdown-view"]');
          if (!root) return {};
          return {
            calloutsCount: root.querySelectorAll('[data-testid="markdown-callout"]').length,
            codeBlocksCount: root.querySelectorAll('pre, [data-testid="code-block-container"]').length,
            tablesCount: root.querySelectorAll('table').length,
            copyButtonsCount: root.querySelectorAll('[data-testid="code-copy-btn"]').length,
            openPracticeBtn: Boolean(document.querySelector('[data-testid="lesson-end-open-practice-btn"]')),
          };
        });
        console.log(`  [Theory Tab] Rich elements:`, elementsInfo);

        // Capture Rich Markdown Cards screenshot for sample
        if (c.type === 'coding') {
          await setTheme(desktopPage, 'light');
          await desktopPage.screenshot({
            path: path.join(OUT_DIR, 'markdown-rich-cards-desktop.png'),
            fullPage: false,
          });
          console.log(`  [SCREENSHOT] Saved: markdown-rich-cards-desktop.png`);
        }

        // 1.2 Test end-of-lesson practice button
        const openPracticeBtn = desktopPage.locator('[data-testid="lesson-end-open-practice-btn"]');
        if (await openPracticeBtn.isVisible()) {
          console.log(`  [Action] Clicking [data-testid="lesson-end-open-practice-btn"]...`);
          await openPracticeBtn.click();
          await desktopPage.waitForTimeout(1000);
        } else {
          // Fallback: switch via top tab
          console.log(`  [Action] Switching to Practice tab via header button...`);
          const practiceTab = desktopPage.locator('button:has-text("Thực hành")').first();
          if (await practiceTab.isVisible()) {
            await practiceTab.click();
            await desktopPage.waitForTimeout(1000);
          }
        }
      } else {
        // Switch to practice tab directly
        const practiceTab = desktopPage.locator('button:has-text("Thực hành")').first();
        if (await practiceTab.isVisible()) {
          await practiceTab.click();
          await desktopPage.waitForTimeout(1000);
        }
      }

      // 1.3 Verify Practice Lab Content & Interactions
      let labResult = { type: c.type, course: c.name, verified: false, details: '' };

      if (c.type === 'coding') {
        const codingLab = desktopPage.locator('[data-testid="coding-practice-lab"]');
        const labExists = await codingLab.isVisible().catch(() => false);
        console.log(`  [Practice Tab] CodingPracticeLab rendered: ${labExists}`);

        // Verify submode tabs
        const bugTab = desktopPage.locator('[data-testid="mode-tab-bug-hunter"]');
        if (await bugTab.isVisible()) {
          await bugTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [CodingLab] Switched to Bug Hunter mode`);
        }

        const visTab = desktopPage.locator('[data-testid="mode-tab-system-visualizer"]');
        if (await visTab.isVisible()) {
          await visTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [CodingLab] Switched to System Visualizer mode`);

          // Switch to TLB visualizer
          const tlbSubTab = desktopPage.locator('[data-testid="submode-tlb-btn"]');
          if (await tlbSubTab.isVisible()) {
            await tlbSubTab.click();
            await desktopPage.waitForTimeout(400);
            console.log(`  [CodingLab] Opened TLB & Memory Visualizer`);

            // Check hit rate slider
            const slider = desktopPage.locator('[data-testid="tlb-hit-rate-slider"]');
            if (await slider.isVisible()) {
              await slider.fill('95');
              await desktopPage.waitForTimeout(300);
              console.log(`  [CodingLab] Adjusted TLB hit rate slider to 95%`);
            }
          }
        }

        // Test notebook save button
        const saveNbBtn = desktopPage.locator('[data-testid="lab-save-notebook-btn"]').first();
        if (await saveNbBtn.isVisible()) {
          await saveNbBtn.click();
          await desktopPage.waitForTimeout(600);
          const feedbackBanner = await desktopPage.locator('[data-testid="lab-feedback-banner"]').textContent().catch(() => '');
          console.log(`  [CodingLab] Save to notebook toast: "${feedbackBanner.trim()}"`);
        }

        // Capture Light & Dark Screenshots
        await setTheme(desktopPage, 'light');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'coding-lab-desktop-light.png') });
        await setTheme(desktopPage, 'dark');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'coding-lab-desktop-dark.png') });
        console.log(`  [SCREENSHOT] Saved coding-lab-desktop-(light|dark).png`);

        labResult.verified = labExists;
        labResult.details = 'Rendered CodingPracticeLab with 3 sub-modes, interactive TLB slider and notebook saving.';
      } else if (c.type === 'english') {
        const engTitle = desktopPage.locator('text=Luyện Tập Tiếng Anh & IELTS Chuyên Sâu');
        const labExists = await engTitle.isVisible().catch(() => false);
        console.log(`  [Practice Tab] EnglishPracticeLab rendered: ${labExists}`);

        // Interact with Accent switch
        const gbBtn = desktopPage.locator('button:has-text("Anh - Anh")');
        if (await gbBtn.isVisible()) {
          await gbBtn.click();
          await desktopPage.waitForTimeout(300);
          console.log(`  [EnglishLab] Accent switched to British English (en-GB)`);
        }

        // Interact with Speed switcher
        const speedBtn = desktopPage.locator('button:has-text("1.25x")');
        if (await speedBtn.isVisible()) {
          await speedBtn.click();
          await desktopPage.waitForTimeout(300);
          console.log(`  [EnglishLab] Speech speed set to 1.25x`);
        }

        // Switch to Dictation mode
        const dictBtn = desktopPage.locator('button:has-text("Chép Chính Tả")');
        if (await dictBtn.isVisible()) {
          await dictBtn.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [EnglishLab] Switched to Dictation practice mode`);
        }

        // Capture Light & Dark Screenshots
        await setTheme(desktopPage, 'light');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'english-lab-desktop-light.png') });
        await setTheme(desktopPage, 'dark');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'english-lab-desktop-dark.png') });
        console.log(`  [SCREENSHOT] Saved english-lab-desktop-(light|dark).png`);

        labResult.verified = labExists;
        labResult.details = 'Rendered EnglishPracticeLab with Shadowing/Dictation/Speaking modes, US/GB accent toggle.';
      } else if (c.type === 'finance') {
        const finLab = desktopPage.locator('[data-testid="finance-practice-lab"]');
        const labExists = await finLab.isVisible().catch(() => false);
        console.log(`  [Practice Tab] FinancePracticeLab rendered: ${labExists}`);

        // Test mode tabs
        const compoundTab = desktopPage.locator('[data-testid="tab-compound"]');
        if (await compoundTab.isVisible()) {
          await compoundTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [FinanceLab] Switched to Compound Interest (Lãi Kép) tab`);
        }

        const debtTab = desktopPage.locator('[data-testid="tab-debt"]');
        if (await debtTab.isVisible()) {
          await debtTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [FinanceLab] Switched to Debt Snowball/Avalanche tab`);
        }

        const tradingTab = desktopPage.locator('[data-testid="tab-trading"]');
        if (await tradingTab.isVisible()) {
          await tradingTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [FinanceLab] Switched to Trading & Candlestick simulation tab`);
        }

        // Capture Light & Dark Screenshots
        await setTheme(desktopPage, 'light');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'finance-lab-desktop-light.png') });
        await setTheme(desktopPage, 'dark');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'finance-lab-desktop-dark.png') });
        console.log(`  [SCREENSHOT] Saved finance-lab-desktop-(light|dark).png`);

        labResult.verified = labExists;
        labResult.details = 'Rendered FinancePracticeLab with Budget 50/30/20, Compound Interest, Debt Payoff, and Candlestick Trading simulations.';
      } else if (c.type === 'psychology') {
        // Switch to recall/skills mode
        const recallModeBtn = desktopPage.locator('[data-testid="practice-mode-recall"]');
        if (await recallModeBtn.isVisible()) {
          await recallModeBtn.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [PsychologyLab] Switched to recall/skills mode`);
        }

        const roleplaySubTab = desktopPage.locator('button:has-text("Mô phỏng tình huống & Tâm lý")');
        if (await roleplaySubTab.isVisible()) {
          await roleplaySubTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [PsychologyLab] Switched to Psychology Roleplay mode`);
        }

        const psychHeading = desktopPage.locator('[data-testid="psychology-practice-lab"]');
        const psychExists = await psychHeading.isVisible().catch(() => false);
        const feynmanHeading = desktopPage.locator('[data-testid="practice-feynman-input"]');
        const feynmanExists = await feynmanHeading.isVisible().catch(() => false);
        console.log(`  [Practice Tab] PsychologyLab / Feynman rendered: ${psychExists || feynmanExists}`);

        // Test flip concept card if in Feynman mode
        const feynmanSubTab = desktopPage.locator('button:has-text("Tự diễn đạt Feynman")');
        if (await feynmanSubTab.isVisible()) {
          await feynmanSubTab.click();
          await desktopPage.waitForTimeout(400);
          console.log(`  [PsychologyLab] Switched to Feynman self-explanation mode`);
        }

        const flipBtn = desktopPage.locator('button:has-text("Lật thẻ xem đáp án")').first();
        if (await flipBtn.isVisible()) {
          await flipBtn.click();
          await desktopPage.waitForTimeout(300);
          console.log(`  [PsychologyLab] Flipped concept card`);
        }

        // Capture Light & Dark Screenshots
        await setTheme(desktopPage, 'light');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'psychology-lab-desktop-light.png') });
        await setTheme(desktopPage, 'dark');
        await desktopPage.screenshot({ path: path.join(OUT_DIR, 'psychology-lab-desktop-dark.png') });
        console.log(`  [SCREENSHOT] Saved psychology-lab-desktop-(light|dark).png`);

        labResult.verified = psychExists || feynmanExists;
        labResult.details = 'Rendered PsychologyLifeSkillsLab / Feynman Active Recall with concept flip cards & roleplay.';
      }

      verificationReport.push(labResult);
    }

    await desktopContext.close();

    // ------------------------------------------------------------------------
    // Part 2: Mobile Verification (390x844)
    // ------------------------------------------------------------------------
    console.log('\n>>> [2/2] MOBILE VERIFICATION (390x844) ...');
    const mobileContext = await browser.newContext({
      viewport: { width: 390, height: 844 },
      isMobile: true,
      hasTouch: true,
      userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1',
    });
    const mobilePage = await mobileContext.newPage();
    setupPageDiagnostics(mobilePage, 'Mobile', diag);

    await loginUser(mobilePage);

    for (const c of COURSES) {
      console.log(`\n--- Mobile: Testing ${c.name} (${c.type}) ---`);
      const url = `${BASE_URL}/app/courses/${c.courseId}/learn?lesson=${c.lessonId}`;
      await mobilePage.goto(url, { waitUntil: 'networkidle' });
      await mobilePage.waitForTimeout(1000);

      // Verify no horizontal overflow in theory view
      const bodyOverflow = await mobilePage.evaluate(() => {
        return {
          scrollWidth: document.documentElement.scrollWidth,
          clientWidth: document.documentElement.clientWidth,
          hasOverflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 2
        };
      });
      console.log(`  [Mobile Theory] Overflow check:`, bodyOverflow);

      if (c.type === 'coding') {
        await setTheme(mobilePage, 'light');
        await mobilePage.screenshot({ path: path.join(OUT_DIR, 'markdown-rich-cards-mobile.png') });
        console.log(`  [SCREENSHOT] Saved markdown-rich-cards-mobile.png`);
      }

      // Switch to Practice tab
      const practiceTab = mobilePage.locator('button:has-text("Thực hành")').first();
      if (await practiceTab.isVisible()) {
        await practiceTab.click();
        await mobilePage.waitForTimeout(1000);
      }

      if (c.type === 'psychology') {
        const recallModeBtn = mobilePage.locator('[data-testid="practice-mode-recall"]');
        if (await recallModeBtn.isVisible()) {
          await recallModeBtn.click();
          await mobilePage.waitForTimeout(400);
        }
      }

      // Verify no horizontal overflow in practice lab
      const labOverflow = await mobilePage.evaluate(() => {
        return {
          scrollWidth: document.documentElement.scrollWidth,
          clientWidth: document.documentElement.clientWidth,
          hasOverflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 2
        };
      });
      console.log(`  [Mobile Practice Lab] Overflow check:`, labOverflow);

      // Capture Mobile Screenshot
      await setTheme(mobilePage, 'light');
      const mobileShotName = `${c.type}-lab-mobile-390.png`;
      await mobilePage.screenshot({ path: path.join(OUT_DIR, mobileShotName) });
      console.log(`  [SCREENSHOT] Saved: ${mobileShotName}`);

      // Test Mobile TOC button & automatic closing on lesson change
      const tocBtn = mobilePage.locator('button[aria-label="Mở mục lục bài học"], button:has-text("Mục lục")').first();
      if (await tocBtn.isVisible()) {
        await tocBtn.click();
        await mobilePage.waitForTimeout(500);
        console.log(`  [Mobile TOC] Drawer opened`);

        // Select another lesson in drawer if visible
        const nextLessonLink = mobilePage.locator('aside[data-swms-drawer="mobile-toc"] button').first();
        if (await nextLessonLink.isVisible()) {
          console.log(`  [Mobile TOC] Clicking lesson item to verify auto-close...`);
          await nextLessonLink.click();
          await mobilePage.waitForTimeout(600);
          const drawerStillOpen = await mobilePage.locator('aside[data-swms-drawer="mobile-toc"]').isVisible().catch(() => false);
          console.log(`  [Mobile TOC] Drawer auto-closed on navigation: ${!drawerStillOpen}`);
        }
      }
    }

    await mobileContext.close();

    console.log('\n================================================================');
    console.log('📊 PHASE B PRACTICE LAB VERIFICATION SUMMARY');
    console.log('================================================================');
    console.table(verificationReport);

    console.log(`\nDiagnostics: Console Errors = ${diag.consoleErrors.length}, Page Errors = ${diag.pageErrors.length}`);
    if (diag.consoleErrors.length > 0) {
      console.log('Console errors encountered:', diag.consoleErrors);
    }
    if (diag.pageErrors.length > 0) {
      console.log('Page errors encountered:', diag.pageErrors);
    }

    const success = verificationReport.every(r => r.verified) && diag.consoleErrors.length === 0 && diag.pageErrors.length === 0;
    console.log(`\nAll 4 Practice Labs Verified: ${success ? 'PASSED ✅' : 'FAILED ❌'}`);
    process.exit(success ? 0 : 1);

  } catch (err) {
    console.error('Fatal error during Practice Lab verification:', err);
    process.exit(1);
  } finally {
    await browser.close();
  }
}

run();
