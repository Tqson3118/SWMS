import { chromium } from "@playwright/test";

async function main() {
  const browser = await chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();

  page.on("response", (res) => {
    if (res.status() >= 400) {
      console.log(`HTTP ${res.status()} -> ${res.url()}`);
    }
  });

  page.on("console", (msg) => {
    console.log(`[Browser ${msg.type()}] ${msg.text()}`);
  });

  console.log("Navigating to http://127.0.0.1:3000/workspace ...");
  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForTimeout(2000);

  await browser.close();
}

main().catch(console.error);
