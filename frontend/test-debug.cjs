const { chromium } = require("@playwright/test");

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
  await page.goto("http://127.0.0.1:3000/workspace");
  const data = await page.evaluate(() => {
    const header = document.querySelector("[data-testid=global-top-header]");
    const brand = document.querySelector("[data-testid=brand-logo-link]");
    const leftDiv = brand?.parentElement;
    const centerDiv = header?.children[1];
    const rightDiv = header?.children[2];

    const brandRect = brand?.getBoundingClientRect();
    const centerPoint = brandRect
      ? { x: brandRect.left + brandRect.width / 2, y: brandRect.top + brandRect.height / 2 }
      : null;
    const elemAtPoint = centerPoint
      ? document.elementFromPoint(centerPoint.x, centerPoint.y)
      : null;

    return {
      headerRect: header?.getBoundingClientRect(),
      brandRect,
      leftDivRect: leftDiv?.getBoundingClientRect(),
      centerDivRect: centerDiv?.getBoundingClientRect(),
      rightDivRect: rightDiv?.getBoundingClientRect(),
      centerPoint,
      elemAtPoint: elemAtPoint
        ? {
            tag: elemAtPoint.tagName,
            className: elemAtPoint.className?.toString(),
            outerHTML: elemAtPoint.outerHTML.slice(0, 200)
          }
        : null
    };
  });
  console.log(JSON.stringify(data, null, 2));
  await browser.close();
})();
