import assert from "node:assert/strict";
import {
  buildPracticeLessonHref,
  extractQuickCheckQuestions,
  getPracticeActivityTypeLabel,
  resolvePracticeSelection,
} from "./practice-helpers.ts";

let passed = 0;

function runTest(name: string, fn: () => void) {
  try {
    fn();
    console.log(`PASS: ${name}`);
    passed++;
  } catch (err) {
    console.error(`FAIL: ${name}`);
    throw err;
  }
}

runTest("resolvePracticeSelection maps English, Coding, Finance, and Psychology activity types", () => {
  assert.deepEqual(resolvePracticeSelection("dictation", "key-1", "Tiếng Anh"), {
    labMode: "english",
    englishTab: "dictation",
  });
  assert.deepEqual(resolvePracticeSelection("pronunciation", "key-2", "IELTS"), {
    labMode: "english",
    englishTab: "speaking",
  });
  assert.deepEqual(resolvePracticeSelection("js_sandbox", "two-sum", "Lập trình"), {
    labMode: "code",
    codingTab: "js-sandbox",
  });
  assert.deepEqual(resolvePracticeSelection("bug_hunt", null, "C# .NET"), {
    labMode: "code",
    codingTab: "bug-hunter",
  });
  assert.deepEqual(resolvePracticeSelection("portfolio_sim", null, "Đầu tư"), {
    labMode: "finance",
    financeTab: "portfolio",
  });
  assert.deepEqual(resolvePracticeSelection("compound_calculator", null, "Tài chính"), {
    labMode: "finance",
    financeTab: "compound",
  });
  assert.deepEqual(resolvePracticeSelection("stroop_test", "psych-stroop-test", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cognitive-labs",
    psychCogSubTab: "stroop",
  });
  assert.deepEqual(resolvePracticeSelection("memory_span", null, "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cognitive-labs",
    psychCogSubTab: "memory-span",
  });
  assert.deepEqual(resolvePracticeSelection("roleplay", "roleplay-1", "Tiếng Anh Giao Tiếp"), {
    labMode: "english",
    englishTab: "roleplay",
  });
  assert.deepEqual(resolvePracticeSelection("roleplay", "psych-roleplay-1", "Tâm lý học & Kỹ năng sống"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "roleplay",
  });
});

runTest("buildPracticeLessonHref encodes query params cleanly and falls back safely", () => {
  assert.equal(
    buildPracticeLessonHref("course-1", "lesson-2", "dictation", "act key"),
    "/app/practice/course-1/lesson-2?type=dictation&key=act+key"
  );
  assert.equal(buildPracticeLessonHref("course-1", null, "dictation", "k"), "/app/practice");
});

runTest("getPracticeActivityTypeLabel returns natural Vietnamese labels", () => {
  assert.equal(getPracticeActivityTypeLabel("js_sandbox"), "Web Worker Sandbox");
  assert.equal(getPracticeActivityTypeLabel("stroop_test"), "Thí nghiệm Stroop");
});

runTest("extractQuickCheckQuestions extracts up to 2 grounded multiple-choice questions", () => {
  const md = `
# Bộ nhớ ảo & TLB
- **TLB (Translation Lookaside Buffer)**: Bộ nhớ đệm phần cứng tốc độ cao lưu bản đồ trang ảo sang khung trang vật lý gần nhất.
- **Page Fault**: Ngoại lệ do MMU kích hoạt khi tiến trình truy cập trang chưa được nạp vào RAM vật lý.
- **Working Set**: Tập hợp các trang bộ nhớ mà tiến trình đang sử dụng tích cực trong một khoảng thời gian.
`;
  const qs = extractQuickCheckQuestions(md, "Bài 3.2: Bộ nhớ ảo");
  assert.equal(qs.length, 2);
  for (const q of qs) {
    assert.equal(q.options.length, 4);
    assert.ok(q.options.some((o) => o.key === q.correctKey));
    assert.ok(q.question.includes("Bài 3.2: Bộ nhớ ảo"));
  }
  assert.deepEqual(extractQuickCheckQuestions("", "Empty"), []);
});

console.log(`\n✅ All ${passed} practice-helpers unit tests passed!`);
