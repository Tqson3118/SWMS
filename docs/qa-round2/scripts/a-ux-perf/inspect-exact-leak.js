const path = require('path');
const { chromium } = require(path.resolve(__dirname, '../../../../frontend/node_modules/@playwright/test'));

const BASE_URL = 'http://127.0.0.1:3000';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const p = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  await p.goto(`${BASE_URL}/auth`, { waitUntil: 'domcontentloaded' });
  await p.click('[data-testid="demo-switcher-toggle"]');
  await p.click('[data-testid="switch-user-default"]');
  await p.waitForURL('**/app**');

  const courseId = '79B30ACD-0BD5-41C8-8744-271AD7B496CE';
  const lessonId = '0360C033-EBC7-453D-9FAE-C812D3BCB48A';
  await p.goto(`${BASE_URL}/app/courses/${courseId}/learn?lesson=${lessonId}`, { waitUntil: 'networkidle' });
  await p.waitForSelector('[data-testid="lesson-markdown-view"]', { timeout: 15000 });

  const leakDetails = await p.evaluate(() => {
    const markdownView = document.querySelector('[data-testid="lesson-markdown-view"]');
    if (!markdownView) return 'No markdown view';

    const walker = document.createTreeWalker(markdownView, NodeFilter.SHOW_TEXT);
    const results = [];
    let node;
    while ((node = walker.nextNode())) {
      if (/CODE_\d+|[\uE000\u001a]/.test(node.textContent)) {
        let el = node.parentElement;
        results.push({
          nodeText: node.textContent,
          parentTag: el ? el.tagName : null,
          parentHtml: el ? el.outerHTML : null,
          grandParentHtml: el && el.parentElement ? el.parentElement.outerHTML : null
        });
      }
    }
    return results;
  });

  console.log('=== LEAK DETAILS ===');
  console.log(JSON.stringify(leakDetails, null, 2));

  await browser.close();
})();
