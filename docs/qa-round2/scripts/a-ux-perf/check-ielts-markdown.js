const path = require('path');
const fs = require('fs');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';
const OUT_DIR = path.resolve(__dirname, '../../screenshots/a-ux-perf');
const DATA_DIR = path.resolve(__dirname, '../../data/a-ux-perf');

if (!fs.existsSync(OUT_DIR)) fs.mkdirSync(OUT_DIR, { recursive: true });
if (!fs.existsSync(DATA_DIR)) fs.mkdirSync(DATA_DIR, { recursive: true });

(async () => {
  const browser = await chromium.launch({ headless: true });
  const p = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.waitForSelector('[data-testid="demo-switcher-toggle"]', { timeout: 10000 });
  const switcherVisible = await p.isVisible('[data-testid="switch-user-default"]');
  if (!switcherVisible) {
    await p.click('[data-testid="demo-switcher-toggle"]');
    await p.waitForSelector('[data-testid="switch-user-default"]', { timeout: 5000 });
  }
  await Promise.all([
    p.waitForResponse(res => res.url().includes('/api/v1/auth/login') && res.status() === 200),
    p.click('[data-testid="switch-user-default"]')
  ]);
  await p.waitForURL('**/app**', { timeout: 15000 });
  await p.waitForTimeout(600);

  const courseId = '79B30ACD-0BD5-41C8-8744-271AD7B496CE';
  const lessonId = '0360C033-EBC7-453D-9FAE-C812D3BCB48A';
  await p.goto(`${BASE_URL}/app/courses/${courseId}/learn?lesson=${lessonId}`, { waitUntil: 'networkidle' });
  await p.waitForSelector('[data-testid="lesson-markdown-view"]', { timeout: 15000 });

  // Capture light mode
  await p.evaluate(() => {
    localStorage.setItem('swms_theme', 'light');
    document.documentElement.classList.remove('dark');
    document.body?.classList.remove('dark');
  });
  await p.waitForTimeout(500);
  await p.screenshot({ path: path.join(OUT_DIR, 'ielts-markdown-1440x900-light.png'), fullPage: true });

  // Capture dark mode
  await p.evaluate(() => {
    localStorage.setItem('swms_theme', 'dark');
    document.documentElement.classList.add('dark');
    document.body?.classList.add('dark');
  });
  await p.waitForTimeout(500);
  await p.screenshot({ path: path.join(OUT_DIR, 'ielts-markdown-1440x900-dark.png'), fullPage: true });

  // Mobile light mode
  await p.setViewportSize({ width: 390, height: 844 });
  await p.evaluate(() => {
    localStorage.setItem('swms_theme', 'light');
    document.documentElement.classList.remove('dark');
    document.body?.classList.remove('dark');
  });
  await p.waitForTimeout(500);
  await p.screenshot({ path: path.join(OUT_DIR, 'ielts-markdown-390x844-light.png'), fullPage: true });

  // Check DOM text for leaks and tokens
  const domInfo = await p.evaluate(() => {
    const markdownView = document.querySelector('[data-testid="lesson-markdown-view"]');
    if (!markdownView) return { error: 'No lesson-markdown-view found' };

    const fullText = markdownView.textContent || '';
    const fullHtml = markdownView.innerHTML || '';

    // Check for CODE_ placeholders or unicode leaks
    const codePlaceholderMatches = fullText.match(/[\uE000\u001a]?CODE_\d+[\uE000\u001a]?/g) || [];
    const linkPlaceholderMatches = fullText.match(/[\uE000\u001a]?LINK_\d+[\uE000\u001a]?/g) || [];
    const rawPrivateUseChars = fullText.match(/[\uE000-\uF8FF]/g) || [];
    const rawSubChars = fullText.match(/\u001a/g) || [];

    // Find all list items, code elements, tables, blockquotes
    const codeElements = Array.from(markdownView.querySelectorAll('code, pre')).map(el => el.textContent?.trim());
    const listItems = Array.from(markdownView.querySelectorAll('li')).map(el => el.textContent?.trim());
    const tableHeaders = Array.from(markdownView.querySelectorAll('th')).map(el => el.textContent?.trim());
    const tableCells = Array.from(markdownView.querySelectorAll('td')).map(el => el.textContent?.trim());
    const links = Array.from(markdownView.querySelectorAll('a')).map(el => ({ text: el.textContent?.trim(), href: el.getAttribute('href') }));

    // Find all text nodes that contain CODE_0
    const leakyElements = [];
    const walker = document.createTreeWalker(markdownView, NodeFilter.SHOW_TEXT);
    let node;
    while ((node = walker.nextNode())) {
      if (/CODE_\d+|LINK_\d+|[\uE000\u001a]/.test(node.textContent)) {
        leakyElements.push({
          parentTag: node.parentElement ? node.parentElement.tagName : 'unknown',
          parentClass: node.parentElement ? node.parentElement.className : '',
          text: node.textContent
        });
      }
    }

    return {
      hasLeak: codePlaceholderMatches.length > 0 || leakyElements.length > 0,
      codePlaceholderMatches,
      linkPlaceholderMatches,
      rawPrivateUseCharsCount: rawPrivateUseChars.length,
      rawSubCharsCount: rawSubChars.length,
      leakyElements,
      sampleCodeElements: codeElements.slice(0, 5),
      sampleListItems: listItems.slice(0, 10),
      tableHeaders,
      sampleTableCells: tableCells.slice(0, 6),
      sampleLinks: links.slice(0, 10)
    };
  });

  console.log('=== IELTS DOM ANALYSIS ===');
  console.log(JSON.stringify(domInfo, null, 2));

  fs.writeFileSync(
    path.join(DATA_DIR, 'ielts-markdown-analysis.json'),
    JSON.stringify(domInfo, null, 2),
    'utf-8'
  );

  await browser.close();
})();
