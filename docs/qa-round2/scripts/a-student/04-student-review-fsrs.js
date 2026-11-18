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

async function runReviewFsrs() {
  console.log('--- STARTING TASK 4: FSRS SPACED REPETITION REVIEW ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };

  if (!fs.existsSync(SESSION_FILE)) {
    throw new Error('Session file not found. Please run 02-student-course-creation.js first.');
  }
  const session = JSON.parse(fs.readFileSync(SESSION_FILE, 'utf-8'));

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'student-review', diagnostics);

  try {
    // 1. Login as the student
    console.log(`[1/5] Logging in as student: ${session.email}...`);
    await loginViaUi(page, session.email, session.password);

    // 2. Open /app/review
    console.log('[2/5] Navigating to /app/review...');
    await page.goto(`${BASE_URL}/app/review`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1500);

    // Check if review cards are loaded
    const cardFront = page.locator('[data-testid="review-card-front"]').first();
    const isCardVisible = await cardFront.isVisible();
    console.log('Is flashcard front visible?', isCardVisible);

    if (isCardVisible) {
      // 3. Inspect Card Front
      const frontText = await cardFront.textContent();
      console.log('Card Front Text preview:', frontText.trim().replace(/\s+/g, ' ').slice(0, 120));
      await safeScreenshot(page, '22-review-card-front.png', 'Flashcard Front Review');

      // 4. Test Space hotkey to flip card
      console.log('[3/5] Pressing Space to flip card...');
      await page.keyboard.press('Space');
      await page.waitForTimeout(600);

      const cardBack = page.locator('[data-testid="review-card-back"]').first();
      const backText = await cardBack.textContent();
      console.log('Card Back Text preview:', backText.trim().replace(/\s+/g, ' ').slice(0, 120));
      await safeScreenshot(page, '23-review-card-back.png', 'Flashcard Back Flipped View');

      // 5. Test Rating hotkey (3 for Good)
      console.log('[4/5] Pressing "3" (Good) to submit rating...');
      await page.keyboard.press('3');
      await page.waitForTimeout(800);
      await safeScreenshot(page, '24-review-rating.png', 'Review Rating Advanced');

      // 6. Test Undo hotkey (Z)
      console.log('Testing Undo hotkey (Z)...');
      await page.keyboard.press('KeyZ');
      await page.waitForTimeout(800);

      const undoBanner = await page.locator('[data-testid="review-undo-banner"], text=Đã hoàn tác').textContent().catch(() => null);
      console.log('Undo banner detected:', undoBanner);

      // Review through remaining cards in session
      console.log('Completing review cards in queue to check session completion...');
      let safetyCount = 0;
      while (safetyCount < 15) {
        safetyCount++;
        const hasCard = await page.locator('[data-testid="review-card-front"]').first().isVisible();
        if (!hasCard) break;

        // Flip with Space
        await page.keyboard.press('Space');
        await page.waitForTimeout(300);

        // Rate with 3 (Good)
        await page.keyboard.press('3');
        await page.waitForTimeout(600);
      }

      console.log(`Finished ${safetyCount} cards in review session.`);
    } else {
      console.log('No cards initially due in review queue.');
    }

    // 7. Check session complete / empty state
    console.log('[5/5] Checking session completion & daily limit controls...');
    await page.waitForTimeout(1000);

    const isSessionComplete = await page.locator('text=Hoàn thành phiên ôn tập!, text=Không có thẻ đến hạn ôn').isVisible();
    console.log('Session complete state visible:', isSessionComplete);

    // Check button "Ôn thêm 20 thẻ"
    const more20Btn = page.locator('[data-testid="review-more-20-btn"], button:has-text("Ôn thêm 20 thẻ")').first();
    const hasMore20 = await more20Btn.isVisible();
    console.log('Is "Ôn thêm 20 thẻ" button available?', hasMore20);

    if (hasMore20) {
      console.log('Clicking "Ôn thêm 20 thẻ"...');
      await more20Btn.click();
      await page.waitForTimeout(1000);
    }

    await safeScreenshot(page, '25-review-completed-or-empty.png', 'Review Completed or Empty State');

    // 8. Check B7 Bug in code analysis:
    // In frontend/src/app/app/review/page.tsx line 327:
    // const errText = await resp.text().catch(() => "");
    // setErrorBanner(`Đồng bộ đánh giá thất bại (${resp.status})${errText ? `: ${errText}` : ""}. Đã giữ trong hàng đợi offline.`);
    console.log('Auditing B7 Flashcard sync error format in page.tsx...');
    diagnostics.findings.push({
      id: 'B7-SYNC-RAW-JSON-AUDIT',
      title: 'Review sync error banner can display raw JSON from backend error response',
      severity: 'Low',
      detail: 'Trong frontend/src/app/app/review/page.tsx line 327, resp.text() được nối trực tiếp vào thông báo lỗi không qua JSON parse để lấy trường message thân thiện.'
    });

  } catch (err) {
    console.error('Error during review task:', err);
    diagnostics.findings.push({
      id: 'REVIEW-ERROR',
      title: 'Exception during Review Task',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 4 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runReviewFsrs().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runReviewFsrs };
