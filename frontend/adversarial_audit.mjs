import { chromium } from "@playwright/test";

function parseRgb(colorStr) {
  const match = colorStr.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([\d.]+))?\)/);
  if (match) {
    return [
      parseInt(match[1], 10),
      parseInt(match[2], 10),
      parseInt(match[3], 10),
      match[4] !== undefined ? parseFloat(match[4]) : 1.0
    ];
  }
  return [0, 0, 0, 1];
}

function getRelativeLuminance(r, g, b) {
  const srgb = [r, g, b].map((val) => {
    const c = val / 255;
    return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
  });
  return 0.2126 * srgb[0] + 0.7152 * srgb[1] + 0.0722 * srgb[2];
}

function computeContrast(rgb1, rgb2) {
  const lum1 = getRelativeLuminance(rgb1[0], rgb1[1], rgb1[2]);
  const lum2 = getRelativeLuminance(rgb2[0], rgb2[1], rgb2[2]);
  const brightest = Math.max(lum1, lum2);
  const darkest = Math.min(lum1, lum2);
  return (brightest + 0.05) / (darkest + 0.05);
}

async function runAudit() {
  const browser = await chromium.launch({ channel: "chrome" });
  const page = await browser.newPage();

  console.log("=== EMPIRICAL AUDIT STARTED ===");

  // -------------------------------------------------------------
  // PART 1: 3-TIER SURFACE & TYPOGRAPHY WCAG AA CONTRAST AUDIT
  // -------------------------------------------------------------
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1000);

  const contrastResults = await page.evaluate(() => {
    function parseHex(hex) {
      hex = hex.replace("#", "");
      if (hex.length === 3) {
        hex = hex.split("").map((c) => c + c).join("");
      }
      const num = parseInt(hex, 16);
      return [(num >> 16) & 255, (num >> 8) & 255, num & 255, 1];
    }

    function getRelativeLuminance(r, g, b) {
      const srgb = [r, g, b].map((val) => {
        const c = val / 255;
        return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
      });
      return 0.2126 * srgb[0] + 0.7152 * srgb[1] + 0.0722 * srgb[2];
    }

    function contrastRatio(hex1, hex2) {
      const rgb1 = parseHex(hex1);
      const rgb2 = parseHex(hex2);
      const lum1 = getRelativeLuminance(rgb1[0], rgb1[1], rgb1[2]);
      const lum2 = getRelativeLuminance(rgb2[0], rgb2[1], rgb2[2]);
      return ((Math.max(lum1, lum2) + 0.05) / (Math.min(lum1, lum2) + 0.05));
    }

    // Light mode tokens
    const lightSurfaces = {
      canvas: "#F8FAFC",
      panel: "#FFFFFF",
      card: "#F1F5F9",
      elevated: "#E2E8F0"
    };
    const lightTexts = {
      primary: "#0F172A",
      secondary: "#334155",
      muted: "#475569",
      subtle: "#64748B"
    };

    // Dark mode tokens
    const darkSurfaces = {
      canvas: "#020617",
      panel: "#0F172A",
      card: "#1E293B",
      elevated: "#334155"
    };
    const darkTexts = {
      primary: "#F8FAFC",
      secondary: "#E2E8F0",
      muted: "#94A3B8",
      subtle: "#64748B"
    };

    const audit = { light: {}, dark: {} };

    for (const [sName, sHex] of Object.entries(lightSurfaces)) {
      audit.light[sName] = {};
      for (const [tName, tHex] of Object.entries(lightTexts)) {
        const ratio = contrastRatio(tHex, sHex);
        audit.light[sName][tName] = {
          surface: sHex,
          text: tHex,
          ratio: parseFloat(ratio.toFixed(2)),
          passBody: ratio >= 4.5,
          passLarge: ratio >= 3.0
        };
      }
    }

    for (const [sName, sHex] of Object.entries(darkSurfaces)) {
      audit.dark[sName] = {};
      for (const [tName, tHex] of Object.entries(darkTexts)) {
        const ratio = contrastRatio(tHex, sHex);
        audit.dark[sName][tName] = {
          surface: sHex,
          text: tHex,
          ratio: parseFloat(ratio.toFixed(2)),
          passBody: ratio >= 4.5,
          passLarge: ratio >= 3.0
        };
      }
    }

    return audit;
  });

  console.log("=== 1. WCAG AA CONTRAST AUDIT (SURFACES vs TEXT) ===");
  console.log(JSON.stringify(contrastResults, null, 2));

  // -------------------------------------------------------------
  // PART 2: RESPONSIVE HEADER REFLOW & TOUCH TARGET AUDIT
  // -------------------------------------------------------------
  const viewports = [
    { name: "Desktop", width: 1440, height: 900 },
    { name: "Tablet-Portrait", width: 768, height: 1024 },
    { name: "Tablet-Landscape", width: 1024, height: 768 }
  ];

  const targetTestIds = [
    "brand-logo-link",
    "mode-tab-dashboard",
    "mode-tab-reading",
    "mode-tab-split",
    "mode-tab-review",
    "cmd-k-trigger",
    "quota-indicator-btn",
    "keyboard-shortcuts-trigger",
    "theme-toggle-btn",
    "user-profile-btn"
  ];

  console.log("\n=== 2. RESPONSIVE REFLOW & TOUCH TARGET SIZES ===");
  for (const vp of viewports) {
    await page.setViewportSize({ width: vp.width, height: vp.height });
    await page.waitForTimeout(300);

    const overflow = await page.evaluate(() => {
      return {
        scrollWidth: document.documentElement.scrollWidth,
        innerWidth: window.innerWidth,
        overflow: document.documentElement.scrollWidth > window.innerWidth
      };
    });

    console.log(`\nViewport ${vp.name} (${vp.width}x${vp.height}):`);
    console.log(`  Horizontal overflow: ${overflow.overflow} (scrollWidth=${overflow.scrollWidth}, innerWidth=${overflow.innerWidth})`);

    for (const id of targetTestIds) {
      const loc = page.getByTestId(id);
      const isVis = await loc.isVisible();
      if (isVis) {
        const box = await loc.boundingBox();
        const touchPass44 = box.width >= 44 && box.height >= 44;
        const touchPass36 = box.width >= 36 && box.height >= 36;
        console.log(`  - [${id}] size: ${Math.round(box.width)}x${Math.round(box.height)}px | >=44px: ${touchPass44} | >=36px: ${touchPass36}`);
      } else {
        console.log(`  - [${id}] NOT VISIBLE`);
      }
    }
  }

  // -------------------------------------------------------------
  // PART 3: ACCESSIBLE FOCUS-VISIBLE RINGS AUDIT
  // -------------------------------------------------------------
  console.log("\n=== 3. ACCESSIBLE FOCUS-VISIBLE RINGS AUDIT ===");
  await page.setViewportSize({ width: 1440, height: 900 });

  const focusTargets = [
    "mode-tab-dashboard",
    "mode-tab-reading",
    "cmd-k-trigger",
    "theme-toggle-btn",
    "quota-indicator-btn"
  ];

  for (const id of focusTargets) {
    const loc = page.getByTestId(id);
    await loc.focus();
    await page.waitForTimeout(100);

    const focusStyles = await loc.evaluate((el) => {
      const style = window.getComputedStyle(el);
      return {
        outline: style.outline,
        outlineWidth: style.outlineWidth,
        outlineStyle: style.outlineStyle,
        outlineColor: style.outlineColor,
        outlineOffset: style.outlineOffset,
        boxShadow: style.boxShadow
      };
    });

    console.log(`  - [${id}] focus styles:`, focusStyles);
  }

  // Test in Dark Mode too
  console.log("\nTesting focus styles in Dark Mode:");
  await page.evaluate(() => document.documentElement.classList.add("dark"));
  await page.waitForTimeout(200);

  for (const id of focusTargets) {
    const loc = page.getByTestId(id);
    await loc.focus();
    await page.waitForTimeout(100);

    const focusStyles = await loc.evaluate((el) => {
      const style = window.getComputedStyle(el);
      return {
        outline: style.outline,
        outlineWidth: style.outlineWidth,
        outlineStyle: style.outlineStyle,
        outlineColor: style.outlineColor,
        outlineOffset: style.outlineOffset,
        boxShadow: style.boxShadow
      };
    });

    console.log(`  - [${id}] dark focus styles:`, focusStyles);
  }

  // -------------------------------------------------------------
  // PART 4: THEME SWITCHING TOGGLE VERIFICATION
  // -------------------------------------------------------------
  console.log("\n=== 4. THEME SWITCHING TOGGLE VERIFICATION ===");
  // Reset
  await page.evaluate(() => {
    localStorage.removeItem("swms_theme");
    document.documentElement.classList.remove("dark");
  });
  await page.goto("http://127.0.0.1:3000/workspace");
  await page.waitForLoadState("domcontentloaded");
  await page.waitForTimeout(1000);

  const state0 = await page.evaluate(() => ({
    htmlDark: document.documentElement.classList.contains("dark"),
    localStorage: localStorage.getItem("swms_theme")
  }));
  console.log("State before click:", state0);

  const themeBtn = page.getByTestId("theme-toggle-btn");
  await themeBtn.click();
  await page.waitForTimeout(400);

  const state1 = await page.evaluate(() => ({
    htmlDark: document.documentElement.classList.contains("dark"),
    localStorage: localStorage.getItem("swms_theme")
  }));
  console.log("State after 1st click:", state1);

  await themeBtn.click();
  await page.waitForTimeout(400);

  const state2 = await page.evaluate(() => ({
    htmlDark: document.documentElement.classList.contains("dark"),
    localStorage: localStorage.getItem("swms_theme")
  }));
  console.log("State after 2nd click:", state2);

  await browser.close();
  console.log("\n=== EMPIRICAL AUDIT FINISHED ===");
}

runAudit().catch(console.error);
