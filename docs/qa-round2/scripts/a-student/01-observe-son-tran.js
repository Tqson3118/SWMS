const path = require('path');
const {
  BASE_URL,
  DEFAULT_VIEWPORT,
  chromium,
  attachDiagnostics,
  safeScreenshot,
  loginDemoSonTran
} = require('./common');

async function runObservation() {
  console.log('--- STARTING TASK 1: OBSERVE DEMO USER SƠN TRẦN ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'observe-son-tran', diagnostics);

  try {
    // 1. Login as Son Tran
    console.log('[1/8] Logging in as Sơn Trần...');
    await loginDemoSonTran(page);
    console.log('Current URL after login:', page.url());

    // 2. Check Dashboard (/app)
    console.log('[2/8] Examining Dashboard (/app)...');
    await page.goto(`${BASE_URL}/app`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    // Check greeting text (B10 check)
    const greetingEl = page.locator('h1').first();
    const greetingText = await greetingEl.textContent().catch(() => '');
    console.log('Greeting header detected:', greetingText.trim());
    if (greetingText.includes('Chào Trần ,') || greetingText.includes('Trần ,')) {
      diagnostics.findings.push({
        id: 'B10-REGRESSION',
        title: 'Greeting still formats as "Chào Trần ,"',
        actual: greetingText.trim()
      });
    }

    await safeScreenshot(page, '01-son-tran-dashboard.png', 'Dashboard overview for Son Tran');

    // 3. Check /app/courses (Catalog, filters, search)
    console.log('[3/8] Examining Courses (/app/courses)...');
    await page.goto(`${BASE_URL}/app/courses`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    // Filter pills check
    const filterButtons = await page.$$eval('button', btns =>
      btns.map(b => b.textContent.trim()).filter(t => t.includes('Tất cả') || t.includes('Công nghệ') || t.includes('Ngoại ngữ') || t.includes('Đang học'))
    );
    console.log('Filter pills found:', filterButtons);

    // Test search filter
    const searchInput = page.locator('input[placeholder*="Tìm" i], input[type="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('Hệ điều hành');
      await page.waitForTimeout(500);
      await safeScreenshot(page, '02-son-tran-courses-search.png', 'Search filter for "Hệ điều hành"');
      await searchInput.fill('');
      await page.waitForTimeout(300);
    }

    await safeScreenshot(page, '02-son-tran-courses.png', 'Courses catalog with filters');

    // 4. Open Technology course (OSTEP / Hệ điều hành) in Learn mode
    console.log('[4/8] Opening Technology Course (Hệ điều hành)...');
    const techCourseId = '22222222-2222-4222-8222-222222222222';
    await page.goto(`${BASE_URL}/app/courses/${techCourseId}/learn`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);

    // Check headings in lesson (B5 check)
    const h1Count = await page.locator('article h1, [data-swms-scroll-container="learn-page"] h1').count();
    const h1Texts = await page.$$eval('article h1, [data-swms-scroll-container="learn-page"] h1', els => els.map(e => e.textContent.trim()));
    console.log(`H1 count in lesson: ${h1Count}`, h1Texts);
    if (h1Count > 1) {
      diagnostics.findings.push({
        id: 'B5-OBSERVED',
        title: 'Multiple H1 headings in lesson player',
        count: h1Count,
        texts: h1Texts
      });
    }

    await safeScreenshot(page, '03-son-tran-learn-reading.png', 'Technology Course Learn Mode Reading');

    // Test Table of Contents toggle
    const tocBtn = page.locator('button:has-text("Mục lục"), button[title*="Mục lục" i], [data-testid="toc-toggle"]').first();
    if (await tocBtn.isVisible()) {
      await tocBtn.click();
      await page.waitForTimeout(500);
      await safeScreenshot(page, '04-son-tran-learn-toc.png', 'Table of Contents opened');
      // close TOC again
      await tocBtn.click();
      await page.waitForTimeout(300);
    }

    // Test Font Size Changer ('T Aa')
    const fontBtn = page.locator('button:has-text("Aa"), button[title*="cỡ chữ" i], [data-testid="font-size-toggle"]').first();
    if (await fontBtn.isVisible()) {
      await fontBtn.click();
      await page.waitForTimeout(400);
      console.log('Clicked Font size toggle');
    }

    // Test Drawer: Sổ tay & Thẻ
    console.log('Testing side drawer "Sổ tay & Thẻ"...');
    const drawerBtn = page.locator('button:has-text("Sổ tay"), button:has-text("Thẻ"), [data-testid="pkm-drawer-toggle"], [data-testid="toggle-notebook-drawer"]').first();
    if (await drawerBtn.isVisible()) {
      await drawerBtn.click();
      await page.waitForTimeout(800);

      // Check lesson column width (B2 check)
      const colWidth = await page.evaluate(() => {
        const col = document.querySelector('[data-testid="reading-prose-column"]') ||
                    document.querySelector('[data-swms-scroll-container="learn-page"]') ||
                    document.querySelector('article');
        return col ? col.getBoundingClientRect().width : null;
      });
      console.log(`Lesson reading column width with drawer open: ${colWidth}px`);
      if (colWidth && colWidth < 320) {
        diagnostics.findings.push({
          id: 'B2-REGRESSION',
          title: `Lesson reading column collapsed to ${colWidth}px (< 320px) when drawer opened`,
          actual: `${colWidth}px`
        });
      }

      await safeScreenshot(page, '05-son-tran-learn-drawer.png', 'Side drawer Notebook & Cards open');
    }

    // 5. Open IELTS / Language course in Learn mode (parallel PDF / dual reading check)
    console.log('[5/8] Opening IELTS Course in Learn mode...');
    const ieltsCourseId = '79b30acd-0bd5-41c8-8744-271ad7b496ce';
    await page.goto(`${BASE_URL}/app/courses/${ieltsCourseId}/learn`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);

    // Check for PDF or split reader
    const pdfEmbed = await page.locator('embed[type*="pdf"], iframe[src*="pdf"], [data-testid="pdf-viewer"]').count();
    console.log(`PDF embeds detected: ${pdfEmbed}`);

    // Check if raw markdown is visible instead of rendered (B9 check)
    const rawMarkdownDetected = await page.evaluate(() => {
      const bodyText = document.body.innerText;
      return bodyText.includes('```markdown') || (bodyText.includes('## ') && bodyText.includes('### ') && !document.querySelector('h2, h3'));
    });
    if (rawMarkdownDetected) {
      diagnostics.findings.push({
        id: 'B9-REGRESSION',
        title: 'Raw markdown symbols displayed unparsed in lesson view'
      });
    }

    await safeScreenshot(page, '06-son-tran-learn-ielts-pdf.png', 'IELTS course reading view');

    // 6. Check /app/notes
    console.log('[6/8] Examining Notes (/app/notes)...');
    await page.goto(`${BASE_URL}/app/notes`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '07-son-tran-notes.png', 'Notes page for Son Tran');

    // 7. Check /app/documents
    console.log('[7/8] Examining Documents (/app/documents)...');
    await page.goto(`${BASE_URL}/app/documents`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '08-son-tran-documents.png', 'Documents library page');

    // 8. Check /app/library, /app/settings, /workspace
    console.log('[8/8] Examining Library, Settings, and Workspace Classic...');
    await page.goto(`${BASE_URL}/app/library`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '09-son-tran-library.png', 'Library catalog page');

    await page.goto(`${BASE_URL}/app/settings`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    // Check review limit setting
    const dailyLimitInput = page.locator('input[type="number"], select, [data-testid="daily-review-limit"]').first();
    const limitValue = await dailyLimitInput.inputValue().catch(() => '');
    console.log('Daily review limit setting value:', limitValue);

    await safeScreenshot(page, '10-son-tran-settings.png', 'Settings page with FSRS and limits');

    await page.goto(`${BASE_URL}/workspace`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1200);
    await safeScreenshot(page, '11-son-tran-workspace-classic.png', 'Classic Workspace comparison view');

  } catch (err) {
    console.error('Error during observation of Son Tran:', err);
    diagnostics.findings.push({
      id: 'OBSERVE-ERROR',
      title: 'Exception during Son Tran observation',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 1 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runObservation().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runObservation };
