const path = require('path');
const fs = require('fs');
const {
  BASE_URL,
  DEFAULT_VIEWPORT,
  chromium,
  attachDiagnostics,
  safeScreenshot,
  registerViaUi
} = require('./common');

const SESSION_FILE = path.resolve(__dirname, '../../data/student-session.json');

async function runCourseCreation() {
  console.log('--- STARTING TASK 2: STUDENT COURSE CREATION (3 FLOWS) ---');
  const diagnostics = { consoleErrors: [], pageErrors: [], findings: [] };

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: DEFAULT_VIEWPORT });
  const page = await context.newPage();
  attachDiagnostics(page, 'student-creation', diagnostics);

  try {
    // 1. Register fresh student user directly via UI
    const timestamp = Date.now();
    console.log(`[1/5] Registering new student user via UI (timestamp: ${timestamp})...`);
    const studentUser = await registerViaUi(page, timestamp);
    console.log(`Successfully registered and logged in as: ${studentUser.email} (ID: ${studentUser.userId})`);

    // Persist session for subsequent tests
    const sessionData = {
      email: studentUser.email,
      password: studentUser.password,
      userId: studentUser.userId,
      timestamp,
      courses: {}
    };
    fs.writeFileSync(SESSION_FILE, JSON.stringify(sessionData, null, 2), 'utf-8');

    // ==========================================
    // FLOW 1: AI Wizard Course Creation
    // ==========================================
    console.log('[2/5] Flow 1: AI Wizard Course Creation...');
    await page.goto(`${BASE_URL}/app/courses/new`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(1000);

    // Wait for AI Topic input
    const topicInput = page.locator('input[placeholder*="Thiết kế hệ thống"], input[placeholder*="Backend"]').first();
    await topicInput.waitFor({ state: 'visible', timeout: 10000 });
    await topicInput.fill('Tiếng Anh Giao Tiếp Hằng Ngày Cho Sinh Viên');

    // Click Preview
    const previewBtn = page.locator('button:has-text("Xem trước")').first();
    await previewBtn.click();
    console.log('Clicked "Xem trước", waiting for outline preview...');

    // Wait for preview container
    await page.waitForSelector('text=Bản xem trước đề cương', { timeout: 15000 });
    await page.waitForTimeout(600);

    // Extract preview chapter texts to check B6 bug
    const previewChapterTexts = await page.$$eval('.rounded-\\[8px\\], .bg-\\[var\\(--surface-card\\)\\]', (els) => {
      return els.map(e => e.querySelector('p')?.textContent?.trim() || e.textContent.trim()).filter(t => t.includes('Chương'));
    });
    console.log('AI Wizard Preview Chapters:', previewChapterTexts);

    const hasB6PreviewBug = previewChapterTexts.some(t => /Chương\s+\d+\s*:\s*Chương\s+\d+/i.test(t));
    if (hasB6PreviewBug) {
      console.warn('[BUG B6 DETECTED] Duplicate "Chương 1: Chương 1:" in preview!');
      diagnostics.findings.push({
        id: 'B6-AI-PREFIX-PREVIEW',
        title: 'Lặp prefix chương trong bản xem trước AI Wizard',
        severity: 'Medium',
        sample: previewChapterTexts.find(t => /Chương\s+\d+\s*:\s*Chương\s+\d+/i.test(t))
      });
    } else {
      console.log('[PASS] AI Wizard Preview has clean chapter titles.');
    }

    await safeScreenshot(page, '12-ai-wizard-preview.png', 'AI Wizard Syllabus Preview');

    // Click Commit / Create Course
    const createBtn = page.locator('button:has-text("Lưu & Mở khóa học"), button:has-text("Tạo khóa học")').first();
    await createBtn.click();
    console.log('Clicked "Lưu & Mở khóa học", waiting for navigation...');

    await page.waitForURL('**/app/courses/**', { timeout: 25000 });
    await page.waitForTimeout(1500);

    const currentUrl = page.url();
    const aiCourseId = currentUrl.split('/app/courses/')[1].split('/')[0].split('?')[0];
    console.log(`AI Course Created successfully! ID: ${aiCourseId} (URL: ${currentUrl})`);
    sessionData.courses.aiCourse = {
      id: aiCourseId,
      title: 'Tiếng Anh Giao Tiếp Hằng Ngày Cho Sinh Viên',
      url: currentUrl
    };

    // Check chapters on the course page
    const renderedChapters = await page.$$eval('h2, h3, .font-semibold', els => els.map(e => e.textContent.trim()).filter(t => t.includes('Chương')));
    console.log('Rendered chapters on Course page:', renderedChapters);
    const hasB6CourseBug = renderedChapters.some(t => /Chương\s+\d+\s*:\s*Chương\s+\d+/i.test(t));
    if (hasB6CourseBug) {
      console.warn('[BUG B6 DETECTED] Duplicate "Chương 1: Chương 1:" on Course Overview page!');
      diagnostics.findings.push({
        id: 'B6-AI-PREFIX-COURSE',
        title: 'Lặp prefix chương trên trang tổng quan khóa học',
        severity: 'Medium',
        sample: renderedChapters.find(t => /Chương\s+\d+\s*:\s*Chương\s+\d+/i.test(t))
      });
    }

    await safeScreenshot(page, '13-ai-wizard-course-created.png', 'AI Wizard Course Created Overview');

    // ==========================================
    // FLOW 2: Upload PDF / Markdown Document
    // ==========================================
    console.log('[3/5] Flow 2: Upload Document Course Creation...');
    await page.goto(`${BASE_URL}/app/courses/new`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(800);

    // Check file input accept attribute (B13 check)
    const fileInput = page.locator('input[type="file"]').first();
    const acceptAttr = await fileInput.getAttribute('accept');
    console.log('Upload input accept attribute:', acceptAttr);
    if (!acceptAttr || !acceptAttr.includes('.pdf')) {
      diagnostics.findings.push({
        id: 'B13-UPLOAD-ACCEPT',
        title: 'File upload input missing .pdf in accept attribute',
        severity: 'Low',
        actual: acceptAttr
      });
    } else {
      console.log('[PASS] File upload input properly specifies accept=".pdf"');
    }

    const testMarkdownPath = path.resolve(__dirname, '../../data/student-test-course.md');
    console.log(`Uploading test file: ${testMarkdownPath}`);

    const uploadStart = Date.now();
    await fileInput.setInputFiles(testMarkdownPath);

    // Wait for processing and navigation
    console.log('Waiting for document processing and redirect to created course...');
    await page.waitForURL('**/app/courses/**', { timeout: 30000 });
    const uploadDuration = Date.now() - uploadStart;
    await page.waitForTimeout(1500);

    const docCourseUrl = page.url();
    const docCourseId = docCourseUrl.split('/app/courses/')[1].split('/')[0].split('?')[0];
    console.log(`Document Course Created in ${uploadDuration}ms! ID: ${docCourseId}`);
    sessionData.courses.docCourse = {
      id: docCourseId,
      title: 'Nhập Môn Cấu Trúc Dữ Liệu & Giải Thuật',
      url: docCourseUrl,
      durationMs: uploadDuration
    };

    await safeScreenshot(page, '14-upload-doc-course-created.png', 'Upload Document Course Created Overview');

    // ==========================================
    // FLOW 3: Course Studio Manual Creation / Editing
    // ==========================================
    console.log('[4/5] Flow 3: Course Studio (/app/studio)...');
    await page.goto(`${BASE_URL}/app/studio`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(1000);
    await safeScreenshot(page, '15-studio-list.png', 'Studio Courses List for Student');

    // Open the document course in studio
    console.log(`Opening Studio Editor for Course: ${docCourseId}...`);
    await page.goto(`${BASE_URL}/app/studio/${docCourseId}`, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(1500);

    // Add a new chapter manually
    console.log('Adding new chapter manually via Studio...');
    const addChapterBtn = page.locator('[data-testid="studio-add-chapter-btn"]').first();
    if (await addChapterBtn.isVisible()) {
      await addChapterBtn.click();
      await page.waitForTimeout(300);

      const chapterInput = page.locator('[data-testid="studio-new-chapter-input"]').first();
      await chapterInput.fill('Chương 3: Cây Nhị Phân & Đồ Thị');

      const confirmBtn = page.locator('[data-testid="studio-create-chapter-confirm"]').first();
      await confirmBtn.click();
      await page.waitForTimeout(1000);
      console.log('New chapter created in Studio!');
    }

    // Toggle publish
    const publishBtn = page.locator('[data-testid="studio-publish-toggle-btn"]').first();
    if (await publishBtn.isVisible()) {
      await publishBtn.click();
      await page.waitForTimeout(800);
      console.log('Toggled course publish state.');
    }

    await safeScreenshot(page, '16-studio-course-outline.png', 'Studio Course Outline with Added Chapter');

    // Update session data file
    fs.writeFileSync(SESSION_FILE, JSON.stringify(sessionData, null, 2), 'utf-8');
    console.log('[5/5] Session data saved successfully to:', SESSION_FILE);

  } catch (err) {
    console.error('Error during course creation task:', err);
    diagnostics.findings.push({
      id: 'CREATION-ERROR',
      title: 'Exception during Course Creation Task',
      error: err.message
    });
  } finally {
    await browser.close();
  }

  console.log('--- TASK 2 COMPLETE ---');
  console.log('Console Errors:', diagnostics.consoleErrors.length);
  console.log('Page Errors:', diagnostics.pageErrors.length);
  console.log('Findings:', JSON.stringify(diagnostics.findings, null, 2));
  return diagnostics;
}

if (require.main === module) {
  runCourseCreation().then(() => process.exit(0)).catch(err => {
    console.error(err);
    process.exit(1);
  });
}

module.exports = { runCourseCreation };
