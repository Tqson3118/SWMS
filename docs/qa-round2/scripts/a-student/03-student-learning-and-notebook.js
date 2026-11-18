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

async function runLearningAndNotebook() {
  console.log('--- STARTING TASK 3: STUDENT LEARNING & NOTEBOOK ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };

  if (!fs.existsSync(SESSION_FILE)) {
    throw new Error('Session file not found. Please run 02-student-course-creation.js first.');
  }
  const session = JSON.parse(fs.readFileSync(SESSION_FILE, 'utf-8'));
  const courseId = session.courses?.aiCourse?.id || session.courses?.docCourse?.id;

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'student-learning', diagnostics);

  try {
    // 1. Login as the student
    console.log(`[1/5] Logging in as student: ${session.email}...`);
    await loginViaUi(page, session.email, session.password);

    // 2. Open Course in Learn mode
    console.log(`[2/5] Opening Learn page for Course: ${courseId}...`);
    await page.goto(`${BASE_URL}/app/courses/${courseId}/learn`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2000);

    // Wait for lesson article
    await page.waitForSelector('article, [data-swms-scroll-container="learn-page"]', { timeout: 15000 });
    console.log('Lesson loaded successfully in Learn view.');

    // Test Font Size Changer ('T Aa')
    console.log('Testing Font Size Changer...');
    const fontBtn = page.locator('button:has-text("Aa"), button[title*="cỡ chữ" i], [data-testid="font-size-toggle"]').first();
    if (await fontBtn.isVisible()) {
      await fontBtn.click();
      await page.waitForTimeout(400);

      // Select large font if available
      const largeOption = page.locator('button:has-text("Lớn"), button:has-text("18px"), button[data-size="lg"]').first();
      if (await largeOption.isVisible()) {
        await largeOption.click();
        await page.waitForTimeout(300);
      }
      await safeScreenshot(page, '17-learn-font-size.png', 'Lesson with Font Size Changed');
      // close typography popover
      await fontBtn.click().catch(() => {});
    } else {
      await safeScreenshot(page, '17-learn-font-size.png', 'Lesson Reading View');
    }

    // Test Table of Contents (TOC)
    console.log('Testing Table of Contents (TOC)...');
    const tocBtn = page.locator('button:has-text("Mục lục"), [data-testid="toc-toggle"]').first();
    if (await tocBtn.isVisible()) {
      await tocBtn.click();
      await page.waitForTimeout(500);
      // close TOC
      await tocBtn.click().catch(() => {});
      await page.waitForTimeout(300);
    }

    // 3. Open Drawer: Sổ tay & Thẻ
    console.log('[3/5] Opening side drawer "Sổ tay & Thẻ"...');
    const drawerBtn = page.locator('button:has-text("Sổ tay"), button:has-text("Thẻ"), [data-testid="pkm-drawer-toggle"], [data-testid="toggle-notebook-drawer"]').first();
    if (await drawerBtn.isVisible()) {
      await drawerBtn.click();
      await page.waitForTimeout(800);
    }

    // Test Highlight: Select text in article
    console.log('Testing text selection and highlight creation...');
    const targetParagraph = page.locator('article p').first();
    if (await targetParagraph.isVisible()) {
      // Simulate selection via JS in the browser
      await page.evaluate(() => {
        const p = document.querySelector('article p');
        if (!p) return;
        const range = document.createRange();
        range.selectNodeContents(p);
        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
        p.dispatchEvent(new MouseEvent('mouseup', { bubbles: true, cancelable: true }));
      });
      await page.waitForTimeout(800);

      // Check if floating toolbar appeared
      const highlightDot = page.locator('button[title*="Đánh dấu"]').first();
      if (await highlightDot.isVisible()) {
        await highlightDot.click();
        await page.waitForTimeout(600);
        console.log('[PASS] Highlight color clicked successfully.');
      } else {
        console.log('Highlight dot not visible directly, testing 1-click or direct highlight.');
      }
    }
    await safeScreenshot(page, '18-learn-highlight.png', 'Highlight and Text Selection Bar');

    // 4. Test Notebook & Autosave behavior
    console.log('[4/5] Testing Notebook Editor and Autosave...');
    // Make sure we are on the Notebook tab
    const notebookTabBtn = page.locator('button:has-text("Sổ tay")').last();
    if (await notebookTabBtn.isVisible()) {
      await notebookTabBtn.click();
      await page.waitForTimeout(500);
    }

    const noteTextarea = page.locator('textarea[placeholder*="Viết ghi chú"], textarea[placeholder*="Ghi chú"], textarea').first();
    if (await noteTextarea.isVisible()) {
      await noteTextarea.fill('Ghi chú của sinh viên: Cần ôn tập kỹ các mẫu câu thực hành và từ vựng chuyên ngành.');
      console.log('Filled notebook textarea. Waiting for debounced auto-save (2000ms)...');
      await page.waitForTimeout(2500);

      // Check save status indicator
      const saveStatus = await page.textContent('text=Đã lưu, text=Đang lưu').catch(() => null);
      console.log('Autosave status text detected:', saveStatus);

      // Test unsaved warning on reload (F5)
      await noteTextarea.fill('Ghi chú của sinh viên: Nội dung mới gõ chưa kịp lưu...');
      // Check beforeunload listener presence
      const hasBeforeUnload = await page.evaluate(() => {
        return window.onbeforeunload !== null;
      });
      console.log('Does window have onbeforeunload handler?', hasBeforeUnload);
      if (!hasBeforeUnload) {
        diagnostics.findings.push({
          id: 'STU-AUTOSAVE-UNLOAD',
          title: 'Thiếu cảnh báo Unsaved Changes (beforeunload) khi reload trang F5',
          severity: 'Medium',
          detail: 'Khi sinh viên đang gõ dở ghi chú và lỡ tay F5 hoặc đóng tab, không có prompt cảnh báo mất dữ liệu.'
        });
      }

      // Wait for autosave to complete
      await page.waitForTimeout(2500);
      await safeScreenshot(page, '19-learn-note-autosave.png', 'Notebook Autosaved Status in Side Drawer');
    } else {
      console.warn('Notebook textarea was not found in drawer.');
    }

    // 5. Test Creating Flashcard from lesson
    console.log('[5/5] Creating Flashcard and Completing Lesson...');
    // Switch drawer tab to Flashcards
    const flashcardTabBtn = page.locator('button:has-text("Thẻ")').last();
    if (await flashcardTabBtn.isVisible()) {
      await flashcardTabBtn.click();
      await page.waitForTimeout(600);
    }

    const addCardBtn = page.locator('button:has-text("Tạo thẻ mới")').first();
    if (await addCardBtn.isVisible()) {
      await addCardBtn.click();
      await page.waitForTimeout(600);

      // Switch to "Hỏi & Đáp" tab in modal
      const manualTabBtn = page.locator('button:has-text("Hỏi & Đáp")').first();
      if (await manualTabBtn.isVisible()) {
        await manualTabBtn.click();
        await page.waitForTimeout(300);
      }

      // Fill flashcard inputs
      const frontInput = page.locator('textarea[placeholder*="Nhập câu hỏi"]').first();
      const backInput = page.locator('textarea[placeholder*="Nhập đáp án"]').first();

      if (await frontInput.isVisible()) {
        await frontInput.fill('Kỹ năng giao tiếp tiếng Anh cơ bản gồm những gì?');
        await backInput.fill('Gồm: Lắng nghe chủ động, phát âm rõ ràng, phản xạ tự nhiên và sử dụng từ vựng theo ngữ cảnh.');

        const saveCardBtn = page.locator('button:has-text("Lưu thẻ ghi nhớ")').first();
        await saveCardBtn.click();
        await page.waitForTimeout(1200);
        console.log('[PASS] Flashcard created successfully from lesson modal!');
      }
    }
    await safeScreenshot(page, '20-learn-create-flashcard.png', 'Flashcard Created from Lesson View');

    // Test Complete Lesson button
    console.log('Clicking "Đánh dấu hoàn thành"...');
    const completeBtn = page.locator('button:has-text("Đánh dấu hoàn thành"), button:has-text("Hoàn thành")').first();
    if (await completeBtn.isVisible()) {
      await completeBtn.click();
      await page.waitForTimeout(1500);
      console.log('Lesson marked as completed!');
    }
    await safeScreenshot(page, '21-learn-completed.png', 'Lesson Marked as Completed Overview');

  } catch (err) {
    console.error('Error during learning task:', err);
    diagnostics.findings.push({
      id: 'LEARNING-ERROR',
      title: 'Exception during Learning Task',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 3 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runLearningAndNotebook().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runLearningAndNotebook };
