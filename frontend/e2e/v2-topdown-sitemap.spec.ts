import fs from "node:fs";
import path from "node:path";
import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures/test-base";

const SAMPLE_ROADMAP_ZIP = path.resolve(__dirname, "../../examples/sample-swm-roadmap.zip");
const SMALL_PDF_PATH = path.resolve(__dirname, "../../test-fixtures/small-3p.pdf");

async function provisionUserAndCourse(page: Page) {
  const email = `v2.sitemap.${Date.now()}.${Math.random().toString(36).slice(2, 7)}@studxy.vn`;
  const regRes = await page.request.post("http://127.0.0.1:5080/api/v1/auth/register", {
    data: {
      email,
      password: "Password!2026",
      fullName: "Học viên Sitemap 4 Tầng",
    },
  });
  expect(regRes.ok()).toBeTruthy();
  const regJson = await regRes.json();
  const userId = regJson.user.id as string;

  await page.addInitScript((uid: string) => {
    window.localStorage.setItem("swms_active_user_id", uid);
  }, userId);

  const zipBuffer = fs.readFileSync(SAMPLE_ROADMAP_ZIP);
  const importRes = await page.request.post("http://127.0.0.1:5080/api/v1/courses/import-zip", {
    headers: { "X-Test-User-Id": userId },
    multipart: {
      file: {
        name: "sample-swm-roadmap.zip",
        mimeType: "application/zip",
        buffer: zipBuffer,
      },
    },
  });
  expect(importRes.ok()).toBeTruthy();
  const importJson = await importRes.json();
  const courseId = String(importJson.courseId || importJson.id);
  expect(courseId).toBeTruthy();

  const pdfBuffer = fs.readFileSync(SMALL_PDF_PATH);
  const uploadRes = await page.request.post("http://127.0.0.1:5080/api/v1/documents/upload", {
    headers: { "X-Test-User-Id": userId },
    multipart: {
      title: "Tài liệu Kiểm thử Sitemap",
      file: {
        name: "small-3p.pdf",
        mimeType: "application/pdf",
        buffer: pdfBuffer,
      },
    },
  });
  expect(uploadRes.ok()).toBeTruthy();
  const uploadJson = await uploadRes.json();
  const documentId = String(uploadJson.documentId || uploadJson.id);
  expect(documentId).toBeTruthy();

  return { userId, courseId, documentId };
}

test.describe("Top-Down 4-Level Sitemap Real E2E (No Mocks)", () => {
  test.setTimeout(90000);

  test("Level 1 & Level 2: /app Today Dashboard, /app/courses filters, /app/courses/new, /app/documents, /app/notes", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    await provisionUserAndCourse(page);

    // Level 1: /app (Today)
    await page.goto("/app");
    await expect(page.getByRole("heading", { name: /hôm nay học gì/i })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByTestId("today-stats-roadmap")).toBeVisible();
    await expect(page.getByText("Tiếp tục lộ trình")).toBeVisible();
    await expect(page.getByText("Ôn tập ghi nhớ")).toBeVisible();
    await expect(page.getByText("Không gian học tập")).toBeVisible();

    // Level 2: /app/courses (Catalog + search + filters)
    await page.getByTestId("sidebar-nav-courses").click();
    await page.waitForURL("**/app/courses", { timeout: 15000 });
    await expect(page.getByRole("heading", { name: "Khóa học", exact: true })).toBeVisible({
      timeout: 15000,
    });

    const courseSearch = page.getByPlaceholder("Tìm khóa học theo tên hoặc mô tả...");
    await courseSearch.fill("Lộ trình Mẫu");
    await expect(page.getByText("Lộ trình Mẫu Chuẩn SWM").first()).toBeVisible();
    await courseSearch.fill("");

    // Level 2: /app/courses/new (4 creation methods + template download)
    await page.goto("/app/courses/new");
    await expect(page.getByRole("heading", { name: "Tạo khóa học mới" })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByText("1. Khởi tạo lộ trình với Trợ lý AI")).toBeVisible();
    await expect(page.getByText("2. Tạo khóa học thủ công")).toBeVisible();
    await expect(page.getByText("3. Tạo từ tài liệu sẵn có")).toBeVisible();
    await expect(page.getByText("4. Nhập gói khóa học (.swm / .zip)")).toBeVisible();
    await expect(page.getByTestId("download-swm-template-btn")).toBeVisible();

    // Level 2: /app/documents (PDF Library)
    await page.getByTestId("sidebar-nav-documents").click();
    await page.waitForURL("**/app/documents", { timeout: 15000 });
    await expect(page.getByRole("heading", { name: "Tài liệu", exact: true })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByRole("heading", { name: "Tài liệu Kiểm thử Sitemap" }).first()).toBeVisible({
      timeout: 15000,
    });

    // Level 2: /app/notes (Notebooks Library)
    await page.getByTestId("sidebar-nav-notes").click();
    await page.waitForURL("**/app/notes", { timeout: 15000 });
    await expect(page.getByRole("heading", { name: "Sổ tay", exact: true })).toBeVisible({
      timeout: 15000,
    });

    // Level 2: /app/practice (Practice Hub)
    await page.getByTestId("sidebar-nav-practice").click();
    await page.waitForURL("**/app/practice", { timeout: 15000 });
    await expect(page.getByRole("heading", { name: "Hub Luyện tập", exact: true })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByTestId("practice-hub-stats")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("practice-hub-review-queue")).toBeVisible();
    await expect(page.getByTestId("practice-hub-courses")).toBeVisible();
  });

  test("Level 3 & Level 4: Course Hub, Course Edit persistence, Quiz, & Learn 4-tab workspace + Study Drawer", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1440, height: 900 });
    const { courseId } = await provisionUserAndCourse(page);

    // Level 3: Course Hub /app/courses/[id]
    await page.goto(`/app/courses/${courseId}`);
    await expect(page.getByRole("heading", { name: /Lộ trình Mẫu Chuẩn SWM/i })).toBeVisible({
      timeout: 15000,
    });
    await expect(page.getByText("Đề cương khóa học")).toBeVisible();

    // Filter syllabus lessons
    const syllabusSearch = page.getByPlaceholder("Tìm bài học trong đề cương...");
    await syllabusSearch.fill("FSRS");
    await expect(page.getByText(/FSRS/i).first()).toBeVisible();
    await syllabusSearch.fill("");

    // Level 3: Course Edit /app/courses/[id]/edit
    await page.getByRole("link", { name: /Biên tập khóa học/i }).click();
    await page.waitForURL(`**/app/courses/${courseId}/edit`, { timeout: 15000 });
    await expect(page.getByText("Cấu trúc khóa học")).toBeVisible({ timeout: 15000 });

    const lessonTitleInput = page
      .locator("div")
      .filter({ has: page.getByText("Tiêu đề bài giảng") })
      .locator("input[type='text']")
      .first();
    await expect(lessonTitleInput).not.toHaveValue("", { timeout: 15000 });
    const originalTitle = await lessonTitleInput.inputValue();

    const saveBtn = page.getByRole("button", { name: /Lưu bài học/i });
    const [saveResp] = await Promise.all([
      page.waitForResponse(
        (resp) => resp.url().includes("/api/v1/lessons/") && resp.request().method() === "PUT"
      ),
      saveBtn.click(),
    ]);
    expect(saveResp.status()).toBe(200);
    await expect(page.getByText("Đã lưu bài học thành công.")).toBeVisible({ timeout: 10000 });

    // Switch to Live Preview tab in editor
    await page.getByRole("button", { name: /Xem trước/i }).click();
    await expect(page.locator("article, div").filter({ hasText: originalTitle }).first()).toBeVisible();

    // Level 4: Focused Learn Workspace /app/courses/[id]/learn
    await page.getByRole("link", { name: /Vào học thử/i }).click();
    await page.waitForURL(`**/app/courses/${courseId}/learn**`, { timeout: 15000 });

    await expect(page.getByTestId("reading-prose-column")).toBeVisible({ timeout: 15000 });
    await expect(page.getByTestId("course-roadmap-sidebar").first()).toBeVisible();

    // Verify Quick Check on /learn and open dedicated fullscreen Practice Lab
    await expect(page.getByTestId("center-tab-practice")).toHaveCount(0);
    await expect(page.getByTestId("lesson-quick-check-section")).toBeVisible({ timeout: 10000 });
    await page.getByTestId("go-to-lesson-practice-btn").click();
    await page.waitForURL(`**/app/practice/${courseId}/**`, { timeout: 15000 });

    await expect(page.getByTestId("lesson-practice-lab")).toBeVisible({ timeout: 15000 });
    await page.getByTestId("practice-mode-finance").click();
    await page.getByTestId("practice-finance-reveal-btn").click();
    await expect(page.getByTestId("practice-finance-result")).toBeVisible();

    // Return back to /app/courses/[id]/learn from fullscreen Practice Lab
    await page.getByTestId("practice-back-to-lesson-btn").click();
    await page.waitForURL(`**/app/courses/${courseId}/learn**`, { timeout: 15000 });
    await expect(page.getByTestId("reading-prose-column")).toBeVisible({ timeout: 15000 });

    // Switch center tab -> Tài liệu PDF & Song song
    await page.getByTestId("center-tab-pdf").click();
    await expect(page.getByTestId("learn-pdf-selector")).toBeVisible({ timeout: 10000 });
    await page.getByTestId("center-tab-split").click();
    await expect(page.getByTestId("reading-prose-column")).toBeVisible();

    // Open Study Drawer (Sổ tay / Đánh dấu / Thẻ) and save note
    await page.getByRole("button", { name: /Sổ tay & Thẻ/i }).click();
    const noteBox = page.getByPlaceholder(/Viết ghi chú bài học tại đây/i);
    await expect(noteBox).toBeVisible({ timeout: 10000 });
    await noteBox.fill("Ghi chú kiểm chứng E2E Sitemap 4 tầng.");

    const [noteSaveResp] = await Promise.all([
      page.waitForResponse(
        (resp) => resp.url().includes("/api/v1/notebooks/") && resp.request().method() === "PUT"
      ),
      page.getByRole("button", { name: "Lưu", exact: true }).click(),
    ]);
    expect(noteSaveResp.status()).toBe(200);

    // Navigate back to Course Hub -> Quiz
    await page.goto(`/app/courses/${courseId}`);
    await page.getByTestId("course-quiz-btn").click();
    await page.waitForURL(`**/app/courses/${courseId}/quiz`, { timeout: 15000 });
    await expect(page.getByTestId("quiz-item-1")).toBeVisible({ timeout: 15000 });
  });
});
