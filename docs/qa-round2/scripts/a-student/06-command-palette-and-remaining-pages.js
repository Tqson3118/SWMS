const path = require('path');
const fs = require('fs');
const {
  BASE_URL,
  DEFAULT_VIEWPORT,
  chromium,
  attachDiagnostics,
  safeScreenshot,
  loginViaUi
} = require('./common');

const SESSION_FILE = path.resolve(__dirname, '../../data/student-session.json');

async function runCmdPaletteAndRemaining() {
  console.log('--- STARTING TASK 6: COMMAND PALETTE & REMAINING PAGES ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };

  if (!fs.existsSync(SESSION_FILE)) {
    throw new Error('Session file not found. Please run 02-student-course-creation.js first.');
  }
  const session = JSON.parse(fs.readFileSync(SESSION_FILE, 'utf-8'));

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'student-cmd-and-pages', diagnostics);

  try {
    // 1. Login as the student
    console.log(`[1/5] Logging in as student: ${session.email}...`);
    await loginViaUi(page, session.email, session.password);

    // 2. Open Command Palette via Ctrl+K or search trigger
    console.log('[2/5] Triggering Command Palette (Ctrl+K)...');
    await page.goto(`${BASE_URL}/app`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(800);

    const searchTrigger = page.locator('[data-testid="cmd-k-trigger"], button:has-text("Tìm kiếm"), button:has-text("Tìm nhanh")').first();
    if (await searchTrigger.isVisible()) {
      await searchTrigger.click();
    } else {
      await page.keyboard.press('Control+KeyK');
    }

    await page.waitForSelector('[data-testid="command-palette-modal"]', { timeout: 8000 });
    await page.waitForTimeout(400);
    console.log('[PASS] Command Palette modal opened successfully.');
    await safeScreenshot(page, '30-cmd-palette-open.png', 'Command Palette Opened');

    // 3. Search in Command Palette
    console.log('[3/5] Searching "Tiếng Anh" in Command Palette...');
    const cmdInput = page.locator('[data-testid="command-palette-input"]').first();
    await cmdInput.fill('Tiếng Anh');
    await page.waitForTimeout(800);

    const resultItems = await page.$$eval('[data-testid^="cmd-result-"]', els => els.map(e => e.textContent.trim().replace(/\s+/g, ' ')));
    console.log(`Found ${resultItems.length} search results:`, resultItems.slice(0, 4));
    await safeScreenshot(page, '31-cmd-palette-search.png', 'Command Palette Search Results');

    // 4. Quick Capture Flashcard
    console.log('[4/5] Testing Quick Capture in Command Palette...');
    const quickInput = page.locator('[data-testid="quick-capture-input"]').first();
    if (await quickInput.isVisible()) {
      await quickInput.fill('Ghi nhớ nhanh: 7 quy tắc phát âm nối âm trong tiếng Anh giao tiếp');
      await page.waitForTimeout(300);

      const quickSaveBtn = page.locator('[data-testid="quick-capture-save-btn"]').first();
      await quickSaveBtn.click();
      await page.waitForTimeout(800);

      const successMsg = await page.textContent('text=Đã tạo thẻ, text=Lưu thẻ').catch(() => null);
      console.log('Quick capture save result:', successMsg);
      await safeScreenshot(page, '32-cmd-palette-quick-action.png', 'Quick Capture Flashcard Saved');
    }

    // Close Command Palette
    await page.keyboard.press('Escape');
    await page.waitForTimeout(400);

    // 5. Test Remaining Pages
    console.log('[5/5] Visiting remaining pages on student account...');

    // /app/notes
    await page.goto(`${BASE_URL}/app/notes`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '33-student-notes.png', 'Student Personal Notes Workspace');

    // /app/documents
    await page.goto(`${BASE_URL}/app/documents`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '34-student-documents.png', 'Student Documents Library');

    // /app/library
    await page.goto(`${BASE_URL}/app/library`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '35-student-library.png', 'Curated Global Library');

    // /app/settings
    await page.goto(`${BASE_URL}/app/settings`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '36-student-settings.png', 'Student Settings & FSRS Configurations');

    // /workspace (Classic)
    await page.goto(`${BASE_URL}/workspace`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1200);
    await safeScreenshot(page, '37-student-workspace.png', 'Workspace Classic View for Student');

  } catch (err) {
    console.error('Error during Command Palette task:', err);
    diagnostics.findings.push({
      id: 'CMD-PALETTE-ERROR',
      title: 'Exception during Command Palette Task',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 6 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runCmdPaletteAndRemaining().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runCmdPaletteAndRemaining };
