import assert from "node:assert/strict";
import {
  WRITEIN_GRADING_LABELS,
  detectCodingCategory,
  evaluateBiasSpotterAnswer,
  evaluateBugFixStatic,
  evaluateCbtExercise,
  evaluateFeynmanOffline,
  evaluateFillCodeBlanks,
  evaluateJournalCriteriaOffline,
  evaluateNumericAnswer,
  evaluateParsonsOrder,
  evaluatePracticeWithAiOrOffline,
  evaluatePredictOutput,
  extractCodeBlocksFromMarkdown,
  generateFinanceCalculationProblem,
  isAsciiOrNonCodeBlock,
  normalizeCodingActivities,
  normalizeFinanceActivities,
  normalizePsychologyActivities,
  type BiasSpotterItem,
  type CbtExerciseItem,
  type FeynmanPromptItem,
  type FinanceCalcType,
} from "./writein-practice.ts";

let passed = 0;

async function runTest(name: string, fn: () => void | Promise<void>) {
  try {
    await fn();
    console.log(`PASS: ${name}`);
    passed++;
  } catch (err) {
    console.error(`FAIL: ${name}`);
    throw err;
  }
}

await runTest("Finance generator: all 5 calculation types produce valid formulas, Vietnamese prompts & distinct random numbers across seeds", () => {
  const types: FinanceCalcType[] = ["compound", "dca", "pe", "margin_of_safety", "drawdown"];
  for (const t of types) {
    const p1 = generateFinanceCalculationProblem(t, 1001);
    const p1Again = generateFinanceCalculationProblem(t, 1001);
    assert.deepEqual(p1, p1Again, `Same seed must be deterministic for ${t}`);
    assert.ok(p1.promptVi.length > 20, `Prompt must be descriptive Vietnamese for ${t}`);
    assert.ok(p1.stepsVi.length >= 2, `Steps must explain calculation for ${t}`);
    assert.ok(["triệu đ", "%", "lần"].includes(p1.unit), `Unit must be valid Vietnamese unit for ${t}`);
    assert.ok(Number.isFinite(p1.expected) && p1.expected > 0, `Expected must be positive finite number for ${t}`);

    // Verify different seeds generate varying problems
    const distinctExpected = new Set<number>();
    for (let s = 100; s < 120; s++) {
      distinctExpected.add(generateFinanceCalculationProblem(t, s * 997).expected);
    }
    assert.ok(distinctExpected.size >= 3, `Expected varied randomized problems for ${t}, got ${distinctExpected.size}`);
  }
});

await runTest("Finance generator math formulas are exact for compound, dca, pe, margin_of_safety, drawdown", () => {
  const comp = generateFinanceCalculationProblem("compound", 2026);
  const { principal, rate, years } = comp.variables;
  const expectedComp = Math.round(principal * Math.pow(1 + rate / 100, years) * 100) / 100;
  assert.equal(comp.expected, expectedComp);

  const dca = generateFinanceCalculationProblem("dca", 2026);
  const i = dca.variables.annualRate / 100 / 12;
  const m = dca.variables.months;
  const expectedDca = Math.round(dca.variables.monthly * ((Math.pow(1 + i, m) - 1) / i) * 100) / 100;
  assert.equal(dca.expected, expectedDca);

  const mos = generateFinanceCalculationProblem("margin_of_safety", 2026);
  const expectedMos =
    Math.round(((mos.variables.intrinsic - mos.variables.marketPrice) / mos.variables.intrinsic) * 100 * 100) / 100;
  assert.equal(mos.expected, expectedMos);

  const dd = generateFinanceCalculationProblem("drawdown", 2026);
  const expectedDd = Math.round(((dd.variables.peak - dd.variables.trough) / dd.variables.peak) * 100 * 100) / 100;
  const expectedRec = Math.round(((dd.variables.peak - dd.variables.trough) / dd.variables.trough) * 100 * 100) / 100;
  assert.equal(dd.expected, expectedDd);
  assert.equal(dd.secondaryExpected, expectedRec);
  assert.ok(dd.secondaryExpected! > dd.expected, "Recovery % must strictly exceed drawdown %");
});

await runTest("evaluateNumericAnswer handles Vietnamese comma decimals, units, and tolerance bounds accurately", () => {
  const exact = evaluateNumericAnswer("259,37 triệu đ", 259.37, 1.5, 0.2);
  assert.equal(exact.isCorrect, true);
  assert.equal(exact.score, 100);

  const withinTolerance = evaluateNumericAnswer("262", 259.37, 1.5, 0.2);
  assert.equal(withinTolerance.isCorrect, true);
  assert.equal(withinTolerance.score, 100);

  const smallAbsTolerance = evaluateNumericAnswer("10.18%", 10.0, 1.5, 0.2);
  assert.equal(smallAbsTolerance.isCorrect, true);

  const outsideTolerance = evaluateNumericAnswer("290", 259.37, 1.5, 0.2);
  assert.equal(outsideTolerance.isCorrect, false);
  assert.ok(outsideTolerance.score < 70);

  const invalid = evaluateNumericAnswer("không biết", 100);
  assert.equal(invalid.isValid, false);
  assert.equal(invalid.isCorrect, false);
  assert.equal(invalid.score, 0);
});

await runTest("Coding: normalizeCodingActivities supports single & multi-item ConfigJson and legacy Parsons format", () => {
  const bank = normalizeCodingActivities([
    {
      id: "act-pred",
      type: "predict_output",
      configJson: JSON.stringify({
        items: [
          { id: "p1", title: "Pred 1", code: "console.log(1 + 2);", expectedOutput: "3", explanation: "1+2=3" },
          { id: "p2", title: "Pred 2", code: "console.log('hi');", expectedOutput: "hi", explanation: "string" },
        ],
      }),
    },
    {
      id: "act-parsons",
      type: "parsons",
      title: "Two Sum Parsons",
      configJson: JSON.stringify({
        title: "Two Sum",
        lines: [
          { id: "l2", text: "  return a + b;", correctOrder: 2 },
          { id: "l1", text: "function add(a, b) {", correctOrder: 1 },
          { id: "l3", text: "}", correctOrder: 3 },
        ],
      }),
    },
  ]);

  assert.equal(bank.predictOutput.length, 2);
  assert.equal(bank.parsons.length, 1);
  assert.deepEqual(bank.parsons[0].correctLines, ["function add(a, b) {", "  return a + b;", "}"]);
  assert.equal(bank.parsons[0].initialShuffled.length, 3);
});

await runTest("Coding: evaluatePredictOutput tolerates whitespace/quotes/JSON spacing and flags wrong output", () => {
  const ok1 = evaluatePredictOutput("  60  \n\n  5  ", "60\n5");
  assert.equal(ok1.isCorrect, true);
  assert.equal(ok1.score, 100);

  const okQuotes = evaluatePredictOutput("['a', 'b']\n\"hello\"", '["a","b"]\nhello');
  assert.equal(okQuotes.isCorrect, true);

  const wrong = evaluatePredictOutput("50\n5", "60\n5");
  assert.equal(wrong.isCorrect, false);
  assert.equal(wrong.matchedLines, 1);
  assert.equal(wrong.score, 50);
});

await runTest("Coding: evaluateFillCodeBlanks, evaluateBugFixStatic, evaluateParsonsOrder", () => {
  const fillRes = evaluateFillCodeBlanks(
    [
      { id: "b0", expected: "filter", acceptableAnswers: ["filter"] },
      { id: "b1", expected: "reduce", acceptableAnswers: ["reduce"] },
    ],
    { b0: "  FILTER ", b1: "map" }
  );
  assert.equal(fillRes.correctCount, 1);
  assert.equal(fillRes.score, 50);
  assert.equal(fillRes.isAllCorrect, false);

  const unchanged = evaluateBugFixStatic("const x = a.sort();", {
    buggyCode: "const x = a.sort();",
    cleanFixedCode: "const x = [...a].sort((x, y) => x - y);",
    requiredTokens: ["sort", "x - y"],
  });
  assert.equal(unchanged.isUnchanged, true);
  assert.equal(unchanged.score, 0);

  const fixed = evaluateBugFixStatic("const x = [...a].sort((x, y) => x - y);", {
    buggyCode: "const x = a.sort();",
    cleanFixedCode: "const x = [...a].sort((x, y) => x - y);",
    requiredTokens: ["sort", "x - y"],
  });
  assert.equal(fixed.isPassed, true);
  assert.equal(fixed.score, 100);

  const parsons = evaluateParsonsOrder(["a", "b", "c"], ["a", "c", "b"]);
  assert.equal(parsons.isAllCorrect, false);
  assert.equal(parsons.correctCount, 1);
  assert.equal(parsons.score, 33);
});

await runTest("Psychology: evaluateBiasSpotterAnswer matches accented/unaccented VN & EN names and labels offline fallback", () => {
  const item: BiasSpotterItem = {
    id: "b1",
    title: "Test bias",
    contextTag: "Test",
    scenario: "Chỉ đọc bài ủng hộ quan điểm của mình.",
    canonicalNameVi: "Thiên kiến xác nhận (Confirmation Bias)",
    acceptedNames: ["Thiên kiến xác nhận", "Confirmation Bias"],
    reasonKeywords: ["thông tin", "củng cố", "bỏ qua"],
    minReasonWords: 6,
    sampleExplanation: "Chỉ tìm thông tin củng cố niềm tin và bỏ qua bằng chứng trái chiều.",
    counterMeasureVi: "Tìm lập luận phản biện.",
  };

  const passUnaccented = evaluateBiasSpotterAnswer(
    item,
    "thien kien xac nhan",
    "Người này chỉ tìm thông tin để củng cố niềm tin cũ và bỏ qua dữ liệu ngược lại."
  );
  assert.equal(passUnaccented.nameMatched, true);
  assert.equal(passUnaccented.isPassed, true);
  assert.equal(passUnaccented.usedLiveEndpoint, false);
  assert.equal(passUnaccented.gradingLabelVi, "Chấm theo từ khóa (offline)");

  const passEnglish = evaluateBiasSpotterAnswer(
    item,
    "Confirmation Bias",
    "Tìm thông tin củng cố quan điểm có sẵn và bỏ qua bằng chứng phản bác."
  );
  assert.equal(passEnglish.nameMatched, true);
  assert.equal(passEnglish.isPassed, true);

  const wrongName = evaluateBiasSpotterAnswer(
    item,
    "Hiệu ứng mỏ neo",
    "Tìm thông tin củng cố quan điểm có sẵn và bỏ qua bằng chứng phản bác."
  );
  assert.equal(wrongName.nameMatched, false);
  assert.equal(wrongName.isPassed, false);
  assert.ok(wrongName.score <= 55);
});

await runTest("Psychology: evaluateFeynmanOffline lists matched/missing keywords and attaches offline label", () => {
  const prompt: FeynmanPromptItem = {
    id: "f1",
    conceptTitle: "Hệ thống 1 & 2",
    promptVi: "Giải thích Hệ thống 1 và 2",
    requiredKeywords: ["hệ thống 1|system 1", "hệ thống 2|system 2", "tự động", "logic", "ví dụ"],
    minWords: 12,
    sampleAnswer: "Hệ thống 1 chạy tự động nhanh, còn Hệ thống 2 dùng logic chậm hơn, ví dụ giải toán.",
  };

  const strong = evaluateFeynmanOffline(
    prompt,
    "Hệ thống 1 phản xạ rất nhanh và tự động theo thói quen, còn Hệ thống 2 suy nghĩ có logic và tốn sức hơn, ví dụ khi tính toán ngân sách."
  );
  assert.equal(strong.isPassed, true);
  assert.equal(strong.missingKeywords.length, 0);
  assert.equal(strong.matchedKeywords.length, 5);
  assert.equal(strong.usedLiveEndpoint, false);
  assert.equal(strong.gradingLabelVi, "Chấm theo từ khóa (offline)");

  const weak = evaluateFeynmanOffline(prompt, "Hệ thống 1 nhanh");
  assert.equal(weak.isPassed, false);
  assert.equal(weak.meetsMinLength, false);
  assert.ok(weak.missingKeywords.length >= 3);
  assert.equal(weak.gradingLabelVi, "Chấm theo từ khóa (offline)");
});

await runTest("Psychology: evaluateCbtExercise & evaluateJournalCriteriaOffline", () => {
  const cbtItem: CbtExerciseItem = {
    id: "cbt1",
    title: "CBT Test",
    scenario: "Không ai trả lời tin nhắn nhóm",
    sampleAutomaticThought: "Mọi người ghét mình",
    canonicalDistortionVi: "Đọc suy nghĩ (Mind Reading)",
    acceptedDistortions: ["Đọc suy nghĩ", "Mind Reading"],
    distortionChoicesHint: ["Đọc suy nghĩ", "Trầm trọng hóa"],
    balancedKeywords: ["bằng chứng", "bận", "hỏi lại"],
    minBalancedWords: 8,
    sampleBalancedThought: "Chưa có bằng chứng họ ghét mình, có thể họ đang bận và mình sẽ hỏi lại sau.",
    explanationVi: "Suy diễn ý người khác là đọc suy nghĩ.",
  };

  const passCbt = evaluateCbtExercise(cbtItem, {
    automaticThought: "Mọi người chắc chắn đang coi thường đề xuất của mình",
    distortionName: "doc suy nghi",
    balancedThought: "Chưa có bằng chứng nào cho thấy họ chê mình, có thể mọi người đang bận họp nên chiều mình sẽ hỏi lại.",
  });
  assert.equal(passCbt.distortionMatched, true);
  assert.equal(passCbt.isPassed, true);
  assert.equal(passCbt.gradingLabelVi, "Chấm theo từ khóa (offline)");

  const journalWeak = evaluateJournalCriteriaOffline(
    [
      { label: "Luận điểm", value: "Tốt", minWords: 6 },
      { label: "Rủi ro", value: "", minWords: 6 },
    ],
    ["biên an toàn", "cắt lỗ"]
  );
  assert.ok(journalWeak.score < 50);
  assert.equal(journalWeak.gradingLabelVi, "Chấm theo từ khóa (offline)");
});

await runTest("evaluatePracticeWithAiOrOffline reports honest live vs offline labels under live, offline, and network-error responses", async () => {
  // 1. Live AI response
  const liveFetch = (async () =>
    new Response(
      JSON.stringify({
        score: 92,
        usedLiveEndpoint: true,
        feedbackVi: "Lập luận chặt chẽ.",
        correctedVersion: "Mẫu chuẩn.",
        strengths: ["Rõ ràng"],
        improvements: [],
      }),
      { status: 200, headers: { "Content-Type": "application/json" } }
    )) as unknown as typeof fetch;

  const liveRes = await evaluatePracticeWithAiOrOffline({
    mode: "finance_case",
    prompt: "DCA",
    userResponse: "Giữ kỷ luật DCA dài hạn giúp hạ giá vốn trung bình.",
    expectedKeywords: ["dca", "kỷ luật"],
    fetchImpl: liveFetch,
  });
  assert.equal(liveRes.usedLiveEndpoint, true);
  assert.equal(liveRes.gradingLabelVi, WRITEIN_GRADING_LABELS.ai_live);
  assert.equal(liveRes.score, 92);

  // 2. Backend returns offline fallback (usedLiveEndpoint: false)
  const backendOfflineFetch = (async () =>
    new Response(
      JSON.stringify({
        score: 78,
        usedLiveEndpoint: false,
        feedbackVi: "Chấm theo từ khóa (offline): tốt.",
      }),
      { status: 200, headers: { "Content-Type": "application/json" } }
    )) as unknown as typeof fetch;

  const backendOfflineRes = await evaluatePracticeWithAiOrOffline({
    mode: "feynman",
    prompt: "Stroop",
    userResponse: "Đọc chữ tự động gây nhiễu màu mực khiến phản xạ chậm lại.",
    expectedKeywords: ["tự động", "màu"],
    fetchImpl: backendOfflineFetch,
  });
  assert.equal(backendOfflineRes.usedLiveEndpoint, false);
  assert.equal(backendOfflineRes.gradingLabelVi, "Chấm theo từ khóa (offline)");

  // 3. Network failure / 503 -> honest deterministic client offline fallback
  const failingFetch = (async () => {
    throw new Error("ECONNREFUSED");
  }) as unknown as typeof fetch;

  const fallbackRes = await evaluatePracticeWithAiOrOffline({
    mode: "bias_spotter",
    prompt: "Confirmation bias",
    userResponse: "Tìm thông tin củng cố niềm tin có sẵn và bỏ qua bằng chứng trái chiều.",
    expectedKeywords: ["thông tin", "củng cố", "bỏ qua"],
    minWords: 8,
    fetchImpl: failingFetch,
  });
  assert.equal(fallbackRes.usedLiveEndpoint, false);
  assert.equal(fallbackRes.gradingMode, "keyword_offline");
  assert.equal(fallbackRes.gradingLabelVi, "Chấm theo từ khóa (offline)");
  assert.ok(fallbackRes.score >= 70);
});

await runTest("Normalizers parse DB activities for Finance and Psychology", () => {
  const finBank = normalizeFinanceActivities([
    {
      id: "f-hc",
      type: "hand_calc",
      configJson: JSON.stringify({ problemTypes: ["compound", "drawdown"] }),
    },
    {
      id: "f-case",
      type: "investment_case",
      configJson: JSON.stringify({
        items: [
          {
            id: "c1",
            title: "Case 1",
            scenario: "Giảm 20%",
            options: ["Bán", "DCA"],
            correctIndex: 1,
            expectedKeywords: ["dca"],
          },
        ],
      }),
    },
  ]);
  assert.deepEqual(finBank.recommendedCalcTypes, ["compound", "drawdown"]);
  assert.equal(finBank.cases.length, 1);

  const psyBank = normalizePsychologyActivities([
    {
      id: "p-bias",
      type: "bias_spotter",
      configJson: JSON.stringify({
        items: [
          {
            id: "bs1",
            scenario: "Sunk cost scenario",
            canonicalNameVi: "Ảo tưởng chi phí chìm",
            acceptedNames: ["chi phí chìm"],
          },
        ],
      }),
    },
  ]);
  assert.equal(psyBank.biasSpotters.length, 1);
});

await runTest("Coding: normalizeCodingActivities preserves language on predictOutput (sql, python, javascript)", () => {
  const bank = normalizeCodingActivities([
    {
      id: "act-pred-lang",
      type: "predict_output",
      configJson: JSON.stringify({
        items: [
          {
            id: "p-sql",
            title: "SQL Query",
            language: "SQL",
            code: "SELECT COUNT(*) FROM Users WHERE IsActive = 1;",
            expectedOutput: "42",
            explanation: "Đếm số bản ghi đang hoạt động.",
          },
          {
            id: "p-py",
            title: "Python Async",
            language: "python",
            code: "print(len([x for x in range(10) if x % 2 == 0]))",
            expectedOutput: "5",
            explanation: "Danh sách số chẵn từ 0 đến 9 gồm 5 phần tử.",
          },
          {
            id: "p-js",
            title: "JS Closure",
            language: "JavaScript",
            code: "console.log(typeof null);",
            expectedOutput: "object",
            explanation: "Lỗi lịch sử của JS typeof null trả về object.",
          },
        ],
      }),
    },
  ]);

  assert.equal(bank.predictOutput.length, 3);
  assert.equal(bank.predictOutput[0].language, "sql");
  assert.equal(bank.predictOutput[1].language, "python");
  assert.equal(bank.predictOutput[2].language, "javascript");
});

await runTest("Coding: isAsciiOrNonCodeBlock & extractCodeBlocksFromMarkdown filters ASCII tables and Mermaid diagrams", () => {
  // Test raw lang / tag filtering
  assert.equal(isAsciiOrNonCodeBlock("text", "Some notes"), true);
  assert.equal(isAsciiOrNonCodeBlock("txt", "Raw text"), true);
  assert.equal(isAsciiOrNonCodeBlock("plain", "Just text"), true);
  assert.equal(isAsciiOrNonCodeBlock("plaintext", "Plain"), true);
  assert.equal(isAsciiOrNonCodeBlock("mermaid", "graph TD;\n A-->B;"), true);
  assert.equal(isAsciiOrNonCodeBlock("ascii", "Diagram"), true);
  assert.equal(isAsciiOrNonCodeBlock("diagram", "Architecture"), true);

  // Test ASCII table borders
  const asciiTable1 = `
+---+----------------------+
| 1 | Table column         |
+---+----------------------+
`;
  assert.equal(isAsciiOrNonCodeBlock("csharp", asciiTable1), true);

  const asciiTable2 = `
|---|----------------------|
| A | Row data             |
|---|----------------------|
`;
  assert.equal(isAsciiOrNonCodeBlock("sql", asciiTable2), true);

  // Test Box-drawing characters (┌─┐│└┘├┤┬┴┼)
  const boxDiagram = `
┌──────────────┐
│  Component   │
└──────────────┘
`;
  assert.equal(isAsciiOrNonCodeBlock("react", boxDiagram), true);

  // Test valid code blocks pass
  assert.equal(isAsciiOrNonCodeBlock("javascript", "const x = 10;\nconsole.log(x);"), false);
  assert.equal(isAsciiOrNonCodeBlock("csharp", "public async Task Run() { await Task.Delay(1); }"), false);
  assert.equal(isAsciiOrNonCodeBlock("python", "def process(): return [1, 2, 3]"), false);
  assert.equal(isAsciiOrNonCodeBlock("sql", "SELECT id, name FROM courses;"), false);

  // Test markdown extraction skips ascii/diagram
  const md = `
# Bài học demo

\`\`\`text
+---+----------------+
| 1 | Sơ đồ hệ thống |
+---+----------------+
\`\`\`

\`\`\`mermaid
graph LR
A[Client] --> B[API]
\`\`\`

\`\`\`typescript
export function calculate(a: number, b: number): number {
  return a + b;
}
\`\`\`
`;
  const extracted = extractCodeBlocksFromMarkdown(md);
  assert.equal(extracted.length, 1);
  assert.equal(extracted[0].language, "typescript");
  assert.ok(extracted[0].code.includes("calculate"));
});

await runTest("Coding: detectCodingCategory classifies SQL, Python, JS, HTML without falling into dotnet", () => {
  // SQL
  assert.equal(
    detectCodingCategory({
      courseTitle: "Database Fundamentals",
      lessonTitle: "Bài 6: SQL Phase 6 - Index và Query Optimization",
      lessonMarkdown: "Tìm hiểu cấu trúc B-Tree Index và tối ưu câu lệnh SELECT WHERE JOIN trong cơ sở dữ liệu.",
    }),
    "sql"
  );

  // Python / OS / DevOps
  assert.equal(
    detectCodingCategory({
      courseTitle: "Lập trình Backend hiện đại",
      lessonTitle: "Bài 4: Xây dựng REST API bất đồng bộ với Python FastAPI",
      lessonMarkdown: "Sử dụng async def, Pydantic schema và httpx client để xử lý luồng dữ liệu.",
    }),
    "os"
  );
  assert.equal(
    detectCodingCategory({
      courseTitle: "DevOps & Hệ thống",
      lessonTitle: "Bài 2: Docker Container & Bash Scripting",
      lessonMarkdown: "Quản lý tiến trình trong container Linux và thiết lập script tự động hóa.",
    }),
    "os"
  );

  // React / Web
  assert.equal(
    detectCodingCategory({
      courseTitle: "Frontend Mastery",
      lessonTitle: "Bài 3: Xử lý Form và DOM với JavaScript",
      lessonMarkdown: "Thao tác trên DOM tree, bắt sự kiện click và validate input bằng JS.",
    }),
    "react"
  );
  assert.equal(
    detectCodingCategory({
      courseTitle: "Thiết kế Web Cơ bản",
      lessonTitle: "Bài 1: HTML5 Semantics & CSS Flexbox Grid",
      lessonMarkdown: "Bố cục giao diện hiện đại với Flexbox và Grid container.",
    }),
    "react"
  );

  // C# / .NET
  assert.equal(
    detectCodingCategory({
      courseTitle: "Khóa học .NET 10",
      lessonTitle: "Bài 5: ASP.NET Core Web API & Entity Framework",
      lessonMarkdown: "C# Controllers, Dependency Injection và DbContext với EF Core.",
    }),
    "dotnet"
  );

  // Generic fallback must be "react", NOT "dotnet"
  assert.equal(
    detectCodingCategory({
      courseTitle: "Tổng quan Lập trình",
      lessonTitle: "Bài mở đầu",
      lessonMarkdown: "Giới thiệu chung về lập trình phần mềm.",
    }),
    "react"
  );
});

console.log(`\n✅ All ${passed} writein-practice unit tests passed!`);
