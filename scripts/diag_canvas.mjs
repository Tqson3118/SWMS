import { chromium } from "playwright";
import path from "node:path";
import fs from "node:fs";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();

  page.on("console", (msg) => console.log("[BROWSER CONSOLE]", msg.type(), msg.text()));
  page.on("pageerror", (err) => console.error("[BROWSER ERROR]", err));
  page.on("requestfailed", (req) => console.error("[REQUEST FAILED]", req.url(), req.failure()?.errorText));

  await page.setViewportSize({ width: 1920, height: 1080 });
  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForLoadState("networkidle");

  const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
  await openUploadBtn.click();

  const userPdfPath = path.resolve(__dirname, "../test/Giáo trình Pháp luật đại cương_ebook.pdf");
  console.log("PDF exists:", fs.existsSync(userPdfPath));

  const fileInput = page.getByTestId("document-file-input");
  await fileInput.setInputFiles(userPdfPath);

  const uploadModal = page.getByTestId("upload-document-modal");
  const submitBtn = uploadModal.locator('button[type="submit"]');
  await submitBtn.click();

  const pageIndicator = page.getByTestId("pdf-page-indicator");
  await pageIndicator.waitFor({ state: "visible", timeout: 30000 });
  console.log("Indicator text:", await pageIndicator.innerText());

  const canvas = page.locator("canvas.pdf-canvas").first();
  await canvas.waitFor({ state: "visible", timeout: 15000 });

  for (let attempt = 1; attempt <= 10; attempt++) {
    const info = await page.evaluate(() => {
      const c = document.querySelector("canvas.pdf-canvas");
      if (!c) return { found: false };
      const ctx = c.getContext("2d");
      if (!ctx) return { found: true, noCtx: true, width: c.width, height: c.height };
      const imgData = ctx.getImageData(0, 0, Math.min(c.width, 200), Math.min(c.height, 200)).data;
      let nonZeroCount = 0;
      let nonWhiteCount = 0;
      for (let i = 0; i < imgData.length; i += 4) {
        const r = imgData[i];
        const g = imgData[i + 1];
        const b = imgData[i + 2];
        const a = imgData[i + 3];
        if (a > 0) nonZeroCount++;
        if (a > 0 && (r < 250 || g < 250 || b < 250)) nonWhiteCount++;
      }
      return {
        found: true,
        width: c.width,
        height: c.height,
        nonZeroCount,
        nonWhiteCount,
        totalPixelsSampled: Math.min(c.width, 200) * Math.min(c.height, 200)
      };
    });
    console.log(`[Attempt ${attempt}] Canvas Info:`, JSON.stringify(info));
    if (info.nonWhiteCount > 0) {
      console.log(">>> SUCCESS: Non-white rendered pixels detected!");
      break;
    }
    await page.waitForTimeout(1000);
  }

  await browser.close();
}

main().catch((err) => {
  console.error("DIAGNOSTIC ERROR:", err);
  process.exit(1);
});
