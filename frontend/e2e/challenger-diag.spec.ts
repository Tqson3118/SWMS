import { test, expect } from "@playwright/test";
import path from "node:path";
import fs from "node:fs";

test("Challenger Diagnostic: Canvas rendering details and browser console logs", async ({ page }) => {
  page.on("console", (msg) => console.log(`[BROWSER CONSOLE ${msg.type()}]`, msg.text()));
  page.on("pageerror", (err) => console.error("[BROWSER PAGE ERROR]", err));
  page.on("requestfailed", (req) => console.error("[REQUEST FAILED]", req.url(), req.failure()?.errorText));
  page.on("response", (res) => {
    if (res.status() >= 400) {
      console.error(`[HTTP ${res.status()}]`, res.url());
    }
  });

  await page.setViewportSize({ width: 1920, height: 1080 });
  await page.goto("/workspace");
  await page.waitForLoadState("domcontentloaded");

  const openUploadBtn = page.getByTestId("open-upload-pdf-btn");
  await openUploadBtn.click();

  const userPdfPath = path.resolve(__dirname, "../../test/Giáo trình Pháp luật đại cương_ebook.pdf");
  const fileInput = page.getByTestId("document-file-input");
  await fileInput.setInputFiles(userPdfPath);

  const uploadModal = page.getByTestId("upload-document-modal");
  const submitBtn = uploadModal.locator('button[type="submit"]');
  await submitBtn.click();

  const pageIndicator = page.getByTestId("pdf-page-indicator");
  await expect(pageIndicator).toBeVisible({ timeout: 30000 });
  await expect(pageIndicator).toHaveText(/Trang 1 \/ 298/, { timeout: 10000 });

  const canvas = page.locator("canvas.pdf-canvas").first();
  await expect(canvas).toBeVisible({ timeout: 15000 });

  // Poll for up to 15 seconds to observe canvas dimensions and pixels
  let finalInfo: any = null;
  for (let i = 1; i <= 30; i++) {
    const info = await page.evaluate(() => {
      const c = document.querySelector("canvas.pdf-canvas") as HTMLCanvasElement | null;
      if (!c) return { found: false };
      const ctx = c.getContext("2d");
      if (!ctx) return { found: true, noCtx: true, width: c.width, height: c.height };
      const imgData = ctx.getImageData(0, 0, Math.min(c.width, 200), Math.min(c.height, 200)).data;
      let nonZeroCount = 0;
      let nonWhiteCount = 0;
      for (let j = 0; j < imgData.length; j += 4) {
        const r = imgData[j];
        const g = imgData[j + 1];
        const b = imgData[j + 2];
        const a = imgData[j + 3];
        if (a > 0) nonZeroCount++;
        if (a > 0 && (r < 250 || g < 250 || b < 250)) nonWhiteCount++;
      }
      return {
        found: true,
        width: c.width,
        height: c.height,
        styleWidth: c.style.width,
        styleHeight: c.style.height,
        nonZeroCount,
        nonWhiteCount,
        totalSampled: Math.min(c.width, 200) * Math.min(c.height, 200)
      };
    });
    console.log(`[Diagnostic Poll ${i}]`, JSON.stringify(info));
    finalInfo = info;
    if (info.nonWhiteCount > 0) {
      console.log(">>> MATCH FOUND! Canvas has rendered non-white pixels!");
      break;
    }
    await page.waitForTimeout(500);
  }

  expect(finalInfo.nonWhiteCount).toBeGreaterThan(0);
});
