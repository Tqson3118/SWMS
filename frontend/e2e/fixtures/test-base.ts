import { test as base, expect, type Page } from "@playwright/test";

export interface TestBaseFixtures {
  capturedErrors: string[];
}

/**
 * Extended Playwright test fixture that enforces the strict zero-error policy:
 * 1. Collects all uncaught exceptions via page.on("pageerror")
 * 2. Collects all console.error messages and React hydration mismatch warnings via page.on("console")
 * 3. Automatically asserts zero captured errors in teardown for every test
 */
export const test = base.extend<TestBaseFixtures>({
  capturedErrors: [
    async ({ page }: { page: Page }, use: (r: string[]) => Promise<void>) => {
      const errors: string[] = [];

      const pageErrorHandler = (err: Error) => {
        errors.push(`[PageError] ${err.message || String(err)}`);
      };

      const consoleHandler = (msg: any) => {
        const text = typeof msg.text === "function" ? msg.text() : String(msg);
        const type = typeof msg.type === "function" ? msg.type() : "unknown";
        if (
          type === "error" ||
          text.includes("Warning: Text content did not match") ||
          text.toLowerCase().includes("hydration mismatch") ||
          text.toLowerCase().includes("hydration failed")
        ) {
          errors.push(`[ConsoleError] ${text}`);
        }
      };

      page.on("pageerror", pageErrorHandler);
      page.on("console", consoleHandler);

      await use(errors);

      expect(errors, "Zero console or runtime errors allowed").toEqual([]);
    },
    { auto: true }
  ]
});

export { expect };
