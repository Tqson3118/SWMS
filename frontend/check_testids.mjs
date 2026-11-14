import { chromium } from "@playwright/test";

async function main() {
  const browser = await chromium.launch({ channel: "chrome" });
  const page = await browser.newPage();
  await page.setViewportSize({ width: 1440, height: 900 });

  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1000);

  const testIds = await page.evaluate(() => {
    return Array.from(document.querySelectorAll("[data-testid]")).map(
      (el) => el.getAttribute("data-testid")
    );
  });
  console.log("All data-testids found:", testIds);

  const themeBtn = page.getByTestId("theme-toggle-btn");
  const count = await themeBtn.count();
  console.log("theme-toggle-btn count:", count);
  if (count > 0) {
    const isVis = await themeBtn.isVisible();
    const box = await themeBtn.boundingBox();
    console.log("theme-toggle-btn visible:", isVis, "box:", box);
  }

  await browser.close();
}

main().catch(console.error);
