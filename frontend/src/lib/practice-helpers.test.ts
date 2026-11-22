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
  assert.equal(getPracticeActivityTypeLabel("predict_output"), "Đoán Output JS");
  assert.equal(getPracticeActivityTypeLabel("bug_fix"), "Sửa Bug Write-in");
  assert.equal(getPracticeActivityTypeLabel("hand_calc"), "Tự tính tay tài chính");
  assert.equal(getPracticeActivityTypeLabel("decision_journal"), "Nhật ký quyết định");
  assert.equal(getPracticeActivityTypeLabel("bias_spotter"), "Nhận diện Thiên kiến");
  assert.equal(getPracticeActivityTypeLabel("feynman"), "Feynman Tự định nghĩa");
  assert.equal(getPracticeActivityTypeLabel("cbt_exercise"), "Bài tập CBT tình huống");
  assert.equal(getPracticeActivityTypeLabel("apply_journal"), "Nhật ký áp dụng");
});

runTest("resolvePracticeSelection maps all new s3a Write-in activity types accurately", () => {
  assert.deepEqual(resolvePracticeSelection("predict_output", "pred-1", "Lập trình JS"), {
    labMode: "code",
    codingTab: "predict-output",
  });
  assert.deepEqual(resolvePracticeSelection("bug_fix", "bug-1", "Lập trình JS"), {
    labMode: "code",
    codingTab: "bug-hunter",
  });
  assert.deepEqual(resolvePracticeSelection("hand_calc", "calc-1", "Đầu tư"), {
    labMode: "finance",
    financeTab: "hand-calc",
  });
  assert.deepEqual(resolvePracticeSelection("decision_journal", "dj-1", "Đầu tư"), {
    labMode: "finance",
    financeTab: "decision-journal",
  });
  assert.deepEqual(resolvePracticeSelection("bias_spotter", "bs-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "bias-spotter",
  });
  assert.deepEqual(resolvePracticeSelection("feynman", "feyn-1", "Tâm lý học Ứng dụng"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "feynman",
  });
  assert.deepEqual(resolvePracticeSelection("cbt_exercise", "cbt-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "cbt-exercise",
  });
  assert.deepEqual(resolvePracticeSelection("apply_journal", "app-1", "Tâm lý học"), {
    labMode: "recall",
    recallSubMode: "psychology",
    psychTab: "apply-journal",
  });
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
