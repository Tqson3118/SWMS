import { expect } from "@playwright/test";
/**
 * Asserts that the document root has zero horizontal overflow:
 * document.documentElement.scrollWidth <= window.innerWidth.
 */
export async function assertZeroHorizontalOverflow(page) {
    const result = await page.evaluate(() => {
        const scrollWidth = document.documentElement.scrollWidth;
        const innerWidth = window.innerWidth;
        return {
            scrollWidth,
            innerWidth,
            hasOverflow: scrollWidth > innerWidth
        };
    });
    expect(result.hasOverflow, `Horizontal overflow detected: scrollWidth (${result.scrollWidth}px) > innerWidth (${result.innerWidth}px)`).toBe(false);
}
/**
 * Asserts that an interactive element's bounding box meets WCAG 2.5.5 / 2.5.8 touch/click target size:
 * width >= minWidth (default 44px) and height >= minHeight (default 44px).
 */
export async function assertTouchTargetSize(locator, minWidth = 44, minHeight = 44) {
    await locator.waitFor({ state: "visible", timeout: 5000 });
    const box = await locator.boundingBox();
    expect(box, "Target element must have a valid rendered bounding box").not.toBeNull();
    if (box) {
        expect(box.width, `Target element width (${box.width}px) must be >= ${minWidth}px for accessible touch ergonomcs`).toBeGreaterThanOrEqual(minWidth);
        expect(box.height, `Target element height (${box.height}px) must be >= ${minHeight}px for accessible touch ergonomcs`).toBeGreaterThanOrEqual(minHeight);
    }
}
/**
 * Computes the WCAG 2.1 relative luminance and contrast ratio between
 * the foreground text color and effective background color of a locator.
 */
export async function computeColorContrast(locator) {
    return await locator.evaluate((element) => {
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
        const fgStyle = window.getComputedStyle(element);
        const fgRgb = parseRgb(fgStyle.color);
        const fgLum = getRelativeLuminance(fgRgb[0], fgRgb[1], fgRgb[2]);
        // Traverse ancestors to find effective background color
        let cur = element;
        let bgRgb = [255, 255, 255, 1]; // default white
        let found = false;
        while (cur && cur !== document.documentElement) {
            const style = window.getComputedStyle(cur);
            const parsed = parseRgb(style.backgroundColor);
            if (parsed[3] > 0.05) {
                bgRgb = parsed;
                found = true;
                break;
            }
            cur = cur.parentElement;
        }
        if (!found) {
            const isDark = document.documentElement.classList.contains("dark");
            bgRgb = isDark ? [2, 6, 23, 1] : [248, 250, 252, 1];
        }
        const bgLum = getRelativeLuminance(bgRgb[0], bgRgb[1], bgRgb[2]);
        const l1 = Math.max(fgLum, bgLum);
        const l2 = Math.min(fgLum, bgLum);
        const ratio = (l1 + 0.05) / (l2 + 0.05);
        return {
            ratio: Math.round(ratio * 100) / 100,
            fgColor: fgStyle.color,
            bgColor: `rgb(${bgRgb[0]}, ${bgRgb[1]}, ${bgRgb[2]})`
        };
    });
}
/**
 * Asserts that the locator meets WCAG AA contrast requirements (default >= 4.5:1).
 */
export async function assertColorContrast(locator, minRatio = 4.5) {
    await locator.waitFor({ state: "visible", timeout: 5000 });
    const result = await computeColorContrast(locator);
    expect(result.ratio, `Color contrast ratio (${result.ratio}:1) between ${result.fgColor} and ${result.bgColor} must be >= ${minRatio}:1 (WCAG AA)`).toBeGreaterThanOrEqual(minRatio);
}
