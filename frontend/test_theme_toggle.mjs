import { chromium } from "@playwright/test";

async function main() {
  const browser = await chromium.launch({ channel: "chrome" });
  const page = await browser.newPage();

  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1000);

  const initialDark = await page.evaluate(() => ({
    htmlClass: document.documentElement.className,
    isDark: document.documentElement.classList.contains("dark"),
    storedTheme: localStorage.getItem("swms_theme"),
    storeDark: window.__store?.getState?.()?.darkMode
  }));
  console.log("Initial state:", initialDark);

  const btn = page.getByTestId("theme-toggle-btn");
  console.log("Btn visible?", await btn.isVisible());

  await btn.click();
  await page.waitForTimeout(500);

  const afterClick = await page.evaluate(() => ({
    htmlClass: document.documentElement.className,
    isDark: document.documentElement.classList.contains("dark"),
    storedTheme: localStorage.getItem("swms_theme")
  }));
  console.log("After 1st click:", afterClick);

  await btn.click();
  await page.waitForTimeout(500);

  const afterClick2 = await page.evaluate(() => ({
    htmlClass: document.documentElement.className,
    isDark: document.documentElement.classList.contains("dark"),
    storedTheme: localStorage.getItem("swms_theme")
  }));
  console.log("After 2nd click:", afterClick2);

  await browser.close();
}

main().catch(console.error);
