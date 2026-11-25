/**
 * Pure TypeScript Active-Recall Write-in Engine for Coding (JS/C#/SQL),
 * Investing & Personal Finance, and Applied Psychology.
 * Zero DOM dependencies — runnable directly in Node (`node --experimental-strip-types`).
 */

export type WriteInGradingMode = "ai_live" | "keyword_offline";

export const WRITEIN_GRADING_LABELS: Record<WriteInGradingMode, string> = {
  ai_live: "AI chấm trực tiếp",
  keyword_offline: "Chấm theo từ khóa (offline)",
};

export interface RawDbPracticeActivity {
  id?: string;
  type?: string;
  title?: string;
  description?: string;
  configJson?: string;
  passingScore?: number;
  orderIndex?: number;
}

export interface AiPracticeEvaluationResult {
  score: number;
  gradingMode: WriteInGradingMode;
  usedLiveEndpoint: boolean;
  gradingLabelVi: string;
  feedbackVi: string;
  correctedVersion: string;
  strengths: string[];
  improvements: string[];
  matchedKeywords: string[];
  missingKeywords: string[];
}

// ============================================================================
// SHARED TEXT NORMALIZATION & KEYWORD UTILITIES
// ============================================================================

export function normalizeViText(input: string): string {
  if (!input) return "";
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .replace(/[’‘`´]/g, "'")
    .replace(/[“”]/g, '"')
    .replace(/[^a-z0-9%\s]/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

export function countMeaningfulWords(input: string): number {
  const norm = normalizeViText(input);
  if (!norm) return 0;
  return norm.split(/\s+/).filter(Boolean).length;
}

export function matchKeywordsVi(
  userText: string,
  expectedKeywords: string[]
): { matched: string[]; missing: string[] } {
  const normUser = normalizeViText(userText);
  const userTokens = new Set(normUser.split(/\s+/).filter(Boolean));
  const matched: string[] = [];
  const missing: string[] = [];

  for (const rawKw of expectedKeywords) {
    const trimmed = (rawKw || "").trim();
    if (!trimmed) continue;

    // Support pipe-separated aliases inside one keyword slot: "hệ thống 1|system 1"
    const alternatives = trimmed
      .split("|")
      .map((a) => a.trim())
      .filter(Boolean);

    let hit = false;
    for (const alt of alternatives) {
      const normAlt = normalizeViText(alt);
      if (!normAlt) continue;
      if (normUser.includes(normAlt)) {
        hit = true;
        break;
      }
      const altTokens = normAlt.split(/\s+/).filter(Boolean);
      if (altTokens.length > 0 && altTokens.every((t) => userTokens.has(t))) {
        hit = true;
        break;
      }
    }

    const displayLabel = alternatives[0] || trimmed;
    if (hit) matched.push(displayLabel);
    else missing.push(displayLabel);
  }

  return { matched, missing };
}

export function evaluateKeywordsOffline(params: {
  userResponse: string;
  expectedKeywords: string[];
  minWords?: number;
  sampleAnswer?: string;
  contextLabel?: string;
}): AiPracticeEvaluationResult {
  const trimmed = (params.userResponse || "").trim();
  const minWords = Math.max(3, params.minWords ?? 10);
  const wordCount = countMeaningfulWords(trimmed);
  const { matched, missing } = matchKeywordsVi(trimmed, params.expectedKeywords || []);
  const totalKeywords = matched.length + missing.length;

  if (!trimmed || wordCount === 0) {
    return {
      score: 0,
      gradingMode: "keyword_offline",
      usedLiveEndpoint: false,
      gradingLabelVi: WRITEIN_GRADING_LABELS.keyword_offline,
      feedbackVi: "Bạn chưa nhập nội dung câu trả lời. Hãy viết lập luận bằng lời của mình rồi kiểm tra lại.",
      correctedVersion: params.sampleAnswer || "",
      strengths: [],
      improvements: [
        `Viết câu trả lời rõ ràng (tối thiểu khoảng ${minWords} từ).`,
        ...(missing.length > 0 ? [`Đề cập các ý trọng tâm: ${missing.slice(0, 5).join(", ")}.`] : []),
      ],
      matchedKeywords: [],
      missingKeywords: missing,
    };
  }

  const keywordRatio = totalKeywords > 0 ? matched.length / totalKeywords : 0.7;
  const lengthRatio = Math.min(1, wordCount / minWords);

  let sampleOverlap = 0;
  if (params.sampleAnswer && params.sampleAnswer.trim()) {
    const sampleTokens = Array.from(
      new Set(
        normalizeViText(params.sampleAnswer)
          .split(/\s+/)
          .filter((t) => t.length >= 3)
      )
    );
    if (sampleTokens.length > 0) {
      const userSet = new Set(normalizeViText(trimmed).split(/\s+/).filter(Boolean));
      const overlapCount = sampleTokens.filter((t) => userSet.has(t)).length;
      sampleOverlap = overlapCount / sampleTokens.length;
    }
  }

  let score = Math.round(keywordRatio * 68 + lengthRatio * 22 + sampleOverlap * 10);
  if (wordCount < Math.max(3, Math.floor(minWords * 0.4))) {
    score = Math.min(score, 45);
  }
  score = Math.max(0, Math.min(100, score));

  const strengths: string[] = [];
  const improvements: string[] = [];

  if (matched.length > 0) {
    strengths.push(
      `Đã chạm đúng ${matched.length}/${totalKeywords} từ khóa cốt lõi: ${matched.join(", ")}.`
    );
  }
  if (wordCount >= minWords) {
    strengths.push(`Độ dài triển khai tốt (${wordCount} từ, đạt ngưỡng tối thiểu ${minWords} từ).`);
  } else {
    improvements.push(
      `Nên diễn giải đầy đủ hơn (${wordCount}/${minWords} từ) và bổ sung ví dụ hoặc dẫn chứng cụ thể.`
    );
  }
  if (missing.length > 0) {
    improvements.push(`Cần bổ sung các khái niệm còn thiếu: ${missing.slice(0, 5).join(", ")}.`);
  }
  if (strengths.length === 0) {
    strengths.push("Đã chủ động tự diễn đạt bằng lời của mình thay vì chỉ chọn trắc nghiệm.");
  }

  const prefix = `${WRITEIN_GRADING_LABELS.keyword_offline}: `;
  const summary =
    score >= 80
      ? `Đạt ${score}/100. Lập luận bám sát trọng tâm (${matched.length}/${totalKeywords} từ khóa).`
      : score >= 60
      ? `Đạt ${score}/100. Đã nắm được phần ý chính (${matched.length}/${totalKeywords} từ khóa), hãy bổ sung thêm các ý còn thiếu.`
      : `Đạt ${score}/100. Câu trả lời còn thiếu từ khóa quan trọng hoặc quá ngắn — hãy đối chiếu gợi ý bên dưới và luyện lại.`;

  return {
    score,
    gradingMode: "keyword_offline",
    usedLiveEndpoint: false,
    gradingLabelVi: WRITEIN_GRADING_LABELS.keyword_offline,
    feedbackVi: prefix + summary,
    correctedVersion: params.sampleAnswer || "",
    strengths,
    improvements,
    matchedKeywords: matched,
    missingKeywords: missing,
  };
}

export interface EvaluatePracticeAiParams {
  mode: string;
  prompt: string;
  userResponse: string;
  expectedKeywords: string[];
  sampleAnswer?: string;
  contextTitle?: string;
  lang?: string;
  minWords?: number;
  headers?: Record<string, string>;
  endpointUrl?: string;
  fetchImpl?: typeof fetch;
}

/**
 * Calls POST /api/v1/ai/evaluate-practice and guarantees Zero-Facade transparency:
 * - Returns `usedLiveEndpoint: true` + `"AI chấm trực tiếp"` ONLY when live AI responded cleanly.
 * - Falls back to deterministic keyword grading with `usedLiveEndpoint: false` + `"Chấm theo từ khóa (offline)"`
 *   whenever Gemini proxy is offline, returns non-2xx, hits quota, or network fails.
 */
export async function evaluatePracticeWithAiOrOffline(
  params: EvaluatePracticeAiParams
): Promise<AiPracticeEvaluationResult> {
  const trimmed = (params.userResponse || "").trim();
  const offlineFallback = () =>
    evaluateKeywordsOffline({
      userResponse: trimmed,
      expectedKeywords: params.expectedKeywords,
      minWords: params.minWords,
      sampleAnswer: params.sampleAnswer,
      contextLabel: params.contextTitle,
    });

  if (!trimmed) {
    return offlineFallback();
  }

  const doFetch = params.fetchImpl ?? (typeof fetch === "function" ? fetch : null);
  if (!doFetch) {
    return offlineFallback();
  }

  try {
    const url = params.endpointUrl ?? "/api/v1/ai/evaluate-practice";
    const res = await doFetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(params.headers ?? {}),
      },
      body: JSON.stringify({
        mode: params.mode,
        lang: params.lang ?? "vi-VN",
        prompt: params.prompt,
        userResponse: trimmed,
        expectedKeywords: params.expectedKeywords,
        sampleAnswer: params.sampleAnswer ?? "",
        contextTitle: params.contextTitle ?? "Thực hành Write-in",
      }),
    });

    if (!res.ok) {
      return offlineFallback();
    }

    const data: any = await res.json().catch(() => null);
    if (!data || typeof data.score !== "number") {
      return offlineFallback();
    }

    const usedLive = Boolean(data.usedLiveEndpoint);
    const gradingMode: WriteInGradingMode = usedLive ? "ai_live" : "keyword_offline";
    const localMatch = matchKeywordsVi(trimmed, params.expectedKeywords);

    const matchedKeywords =
      Array.isArray(data.matchedKeywords) && data.matchedKeywords.length > 0
        ? data.matchedKeywords.map(String)
        : localMatch.matched;
    const missingKeywords =
      Array.isArray(data.missingKeywords) && data.missingKeywords.length > 0
        ? data.missingKeywords.map(String)
        : localMatch.missing;

    return {
      score: Math.max(0, Math.min(100, Math.round(data.score))),
      gradingMode,
      usedLiveEndpoint: usedLive,
      gradingLabelVi: usedLive
        ? WRITEIN_GRADING_LABELS.ai_live
        : WRITEIN_GRADING_LABELS.keyword_offline,
      feedbackVi:
        typeof data.feedbackVi === "string" && data.feedbackVi.trim()
          ? data.feedbackVi.trim()
          : usedLive
          ? "AI đã hoàn tất đánh giá câu trả lời của bạn."
          : offlineFallback().feedbackVi,
      correctedVersion:
        typeof data.correctedVersion === "string" && data.correctedVersion.trim()
          ? data.correctedVersion.trim()
          : params.sampleAnswer || "",
      strengths: Array.isArray(data.strengths) ? data.strengths.map(String) : [],
      improvements: Array.isArray(data.improvements) ? data.improvements.map(String) : [],
      matchedKeywords,
      missingKeywords,
    };
  } catch {
    return offlineFallback();
  }
}

// ============================================================================
// SAFE JSON PARSER FOR PracticeActivity.ConfigJson (SINGLE OR MULTI-ITEM)
// ============================================================================

export function safeParseConfigJson(raw?: string | null): Record<string, any> | null {
  if (!raw || typeof raw !== "string") return null;
  try {
    const parsed = JSON.parse(raw);
    return parsed && typeof parsed === "object" ? parsed : null;
  } catch {
    return null;
  }
}

function expandConfigItems(cfg: Record<string, any>): Record<string, any>[] {
  if (Array.isArray(cfg.items) && cfg.items.length > 0) {
    return cfg.items.filter((x: unknown) => x && typeof x === "object");
  }
  return [cfg];
}

// ============================================================================
// SECTION 1: CODING (JS / C# / SQL) PARSERS & EVALUATORS
// ============================================================================

export interface JsSandboxTestCase {
  name: string;
  input: any[];
  expected: any;
}

export interface JsSandboxExerciseItem {
  id: string;
  title: string;
  difficulty: "Dễ" | "Trung bình" | "Nâng cao";
  description: string;
  functionName: string;
  starterCode: string;
  solutionCode: string;
  hint: string;
  timeLimitMs: number;
  testCases: JsSandboxTestCase[];
  sourceActivityId?: string;
  passingScore?: number;
}

export interface PredictOutputItem {
  id: string;
  title: string;
  difficulty: "Dễ" | "Trung bình" | "Nâng cao";
  description: string;
  code: string;
  expectedOutput: string;
  acceptedOutputs?: string[];
  hint?: string;
  explanation: string;
  timeLimitMs?: number;
  sourceActivityId?: string;
  passingScore?: number;
  language?: string;
}

export interface BugFixWriteInItem {
  id: string;
  title: string;
  language: string;
  categoryTag: string;
  context: string;
  buggyCode: string;
  bugLineNumber?: number;
  bugBriefDescription?: string;
  rootCauseAnalysis?: string;
  realWorldImpact?: string;
  cleanFixedCode: string;
  acceptedSolutions?: string[];
  requiredTokens?: string[];
  forbiddenTokens?: string[];
  functionName?: string;
  testCases?: JsSandboxTestCase[];
  explanation: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface FillCodeBlankSpec {
  id: string;
  blankIndex: number;
  expected: string;
  acceptableAnswers: string[];
  hint: string;
  chips: string[];
  explanation: string;
}

export interface FillCodeWriteInItem {
  id: string;
  title: string;
  category: "dotnet" | "react" | "sql" | "os";
  language: string;
  description: string;
  practicalScenario: string;
  codeTemplate: string[];
  blanks: FillCodeBlankSpec[];
  lineExplanations: Array<{
    lineNumber: number;
    codeSnippet: string;
    purpose: string;
    riskIfWrong: string;
  }>;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface ParsonsWriteInItem {
  id: string;
  title: string;
  category: string;
  goal: string;
  explanation: string;
  correctLines: string[];
  initialShuffled: string[];
  sourceActivityId?: string;
  passingScore?: number;
}

export interface NormalizedCodingBank {
  jsSandbox: JsSandboxExerciseItem[];
  predictOutput: PredictOutputItem[];
  bugFix: BugFixWriteInItem[];
  fillCode: FillCodeWriteInItem[];
  parsons: ParsonsWriteInItem[];
}

function deterministicShuffleLines(lines: string[]): string[] {
  if (lines.length <= 1) return [...lines];
  const copy = [...lines];
  // Deterministic Fisher-Yates seeded by content length so SSR/CSR & rerenders stay stable
  let seed = copy.reduce((acc, s, idx) => acc + s.length * (idx + 7), 1337);
  for (let i = copy.length - 1; i > 0; i--) {
    seed = (seed * 1664525 + 1013904223) >>> 0;
    const j = seed % (i + 1);
    const tmp = copy[i];
    copy[i] = copy[j];
    copy[j] = tmp;
  }
  // Ensure order actually differs from original when length >= 2
  if (copy.every((line, idx) => line === lines[idx])) {
    const first = copy.shift()!;
    copy.push(first);
  }
  return copy;
}

function normalizeDifficultyVi(raw: unknown): "Dễ" | "Trung bình" | "Nâng cao" {
  const s = normalizeViText(String(raw ?? ""));
  if (s === "de" || s === "easy") return "Dễ";
  if (s === "kho" || s === "hard" || s === "nang cao") return "Nâng cao";
  return "Trung bình";
}

export type CodingTopicCategory = "dotnet" | "react" | "sql" | "os";

export function isAsciiOrNonCodeBlock(rawLang = "", code = ""): boolean {
  const normLang = (rawLang || "").trim().toLowerCase();
  if (
    normLang === "text" ||
    normLang === "txt" ||
    normLang === "plain" ||
    normLang === "plaintext" ||
    normLang === "mermaid" ||
    normLang === "ascii" ||
    normLang === "diagram"
  ) {
    return true;
  }
  if (!code) return false;
  if (code.includes("+---+") || code.includes("|---|")) {
    return true;
  }
  if (/[┌─┐│└┘├┤┬┴┼]/.test(code)) {
    return true;
  }
  return false;
}

export function extractCodeBlocksFromMarkdown(
  markdown: string
): Array<{ language: string; code: string }> {
  if (!markdown) return [];
  const codeBlockRegex = /```([a-zA-Z0-9_-]*)\r?\n([\s\S]*?)```/g;
  const blocks: Array<{ language: string; code: string }> = [];
  let match: RegExpExecArray | null;
  while ((match = codeBlockRegex.exec(markdown)) !== null) {
    const rawLang = (match[1] || "").trim();
    const code = match[2].trim();
    if (!isAsciiOrNonCodeBlock(rawLang, code)) {
      blocks.push({ language: rawLang, code });
    }
  }
  return blocks;
}

export function detectCodingCategory(params: {
  fillCodeCategory?: string;
  language?: string;
  courseTitle?: string;
  lessonTitle?: string;
  lessonMarkdown?: string;
}): CodingTopicCategory {
  // 1. Explicit valid category from DB
  const rawCat = (params.fillCodeCategory || "").trim().toLowerCase();
  if (rawCat === "sql" || rawCat === "os" || rawCat === "react" || rawCat === "dotnet") {
    return rawCat;
  }

  // 2. Explicit language from DB or code block
  const rawLang = (params.language || "").trim().toLowerCase();
  if (rawLang) {
    if (
      rawLang === "sql" ||
      rawLang === "postgres" ||
      rawLang === "postgresql" ||
      rawLang === "mysql" ||
      rawLang === "tsql" ||
      rawLang === "sqlite"
    ) {
      return "sql";
    }
    if (
      rawLang === "python" ||
      rawLang === "py" ||
      rawLang === "bash" ||
      rawLang === "sh" ||
      rawLang === "shell" ||
      rawLang === "dockerfile" ||
      rawLang === "docker" ||
      rawLang === "linux"
    ) {
      return "os";
    }
    if (
      rawLang === "javascript" ||
      rawLang === "js" ||
      rawLang === "typescript" ||
      rawLang === "ts" ||
      rawLang === "jsx" ||
      rawLang === "tsx" ||
      rawLang === "html" ||
      rawLang === "css"
    ) {
      return "react";
    }
    if (
      rawLang === "csharp" ||
      rawLang === "cs" ||
      rawLang === "c#" ||
      rawLang === "dotnet" ||
      rawLang === ".net"
    ) {
      return "dotnet";
    }
  }

  // 3. Text analysis of courseTitle + lessonTitle + lessonMarkdown in strict priority
  const combined = `${params.courseTitle || ""} ${params.lessonTitle || ""} ${params.lessonMarkdown || ""}`.toLowerCase();

  // (a) SQL: sql, database, postgres, t-sql, join, index, transaction, ef core migration/db
  if (
    /\b(sql|database|postgres|postgresql|t-sql|tsql|mysql|sqlite|nosql)\b/.test(combined) ||
    combined.includes("truy vấn") ||
    combined.includes("cơ sở dữ liệu") ||
    combined.includes("sql injection") ||
    combined.includes("inner join") ||
    combined.includes("left join") ||
    combined.includes("migration db") ||
    combined.includes("ef core migration")
  ) {
    return "sql";
  }

  // (b) OS: linux, bash, docker, container, hệ điều hành, bộ nhớ, python, fastapi, rag, llm, aiops, git, setup, cli, terminal, algorithm
  const hasOsContainer =
    /\b(docker|lxc|cgroups|namespaces|containerization|container hóa)\b/.test(combined) ||
    (/\bcontainer\b/.test(combined) && !/\b(flexbox|grid|css|html|layout|ui)\b/.test(combined));

  if (
    /\b(linux|bash|shell|python|fastapi|rag|llm|aiops|git|cli|terminal|algorithm|tlb|deadlock)\b/.test(combined) ||
    hasOsContainer ||
    combined.includes("hệ điều hành") ||
    combined.includes("bộ nhớ ảo") ||
    combined.includes("phân trang") ||
    combined.includes("bộ nhớ") ||
    combined.includes("thuật toán")
  ) {
    return "os";
  }

  // (c) React / Web: html, css, flexbox, grid, javascript, typescript, dom, react, next, hook, useeffect, closure, frontend, trello
  if (
    /\b(html|css|flexbox|grid|javascript|typescript|js|ts|dom|react|next|nextjs|hook|useeffect|usestate|closure|frontend|trello)\b/.test(combined) ||
    combined.includes("lập trình web")
  ) {
    return "react";
  }

  // (d) Dotnet: .net, c#, csharp, asp.net, web api, linq, entity framework, ef core
  if (
    /\b(c#|csharp|dotnet|asp\.net|web api|linq|entity framework|ef core)\b/.test(combined) ||
    combined.includes(".net")
  ) {
    return "dotnet";
  }

  // Mặc định: "react" cho lập trình web phổ thông thay vì gán nhầm sang dotnet
  return "react";
}

export function normalizeCodingActivities(
  activities: RawDbPracticeActivity[]
): NormalizedCodingBank {
  const bank: NormalizedCodingBank = {
    jsSandbox: [],
    predictOutput: [],
    bugFix: [],
    fillCode: [],
    parsons: [],
  };

  for (const act of activities || []) {
    const type = (act.type || "").trim().toLowerCase();
    const cfg = safeParseConfigJson(act.configJson);
    if (!cfg) continue;

    const items = expandConfigItems(cfg);

    if (type === "js_sandbox" || type === "code_write" || type === "js-sandbox") {
      items.forEach((raw, idx) => {
        const starterCode = String(raw.starterCode || "").trim();
        const rawCases = Array.isArray(raw.testCases) ? raw.testCases : [];
        if (!starterCode || rawCases.length === 0) return;

        const testCases: JsSandboxTestCase[] = rawCases.map((tc: any, tcIdx: number) => ({
          name: String(tc.name || tc.description || `Test case #${tcIdx + 1}`),
          input: Array.isArray(tc.input) ? tc.input : [tc.input],
          expected: tc.expected,
        }));

        const inferredFn =
          String(raw.functionName || "").trim() ||
          starterCode.match(/function\s+([A-Za-z_$][A-Za-z0-9_$]*)/)?.[1] ||
          "solution";

        bank.jsSandbox.push({
          id: String(raw.id || `${act.id || "db-js"}-${idx}`),
          title: String(raw.title || act.title || `Bài tập JS #${idx + 1}`),
          difficulty: normalizeDifficultyVi(raw.difficulty),
          description: String(raw.description || act.description || "Viết hàm JavaScript vượt qua toàn bộ test case."),
          functionName: inferredFn,
          starterCode,
          solutionCode: String(raw.solutionCode || starterCode),
          hint: String(raw.hint || "Kiểm tra kỹ kiểu dữ liệu đầu vào và trường hợp biên (mảng rỗng, số âm)."),
          timeLimitMs: Number(raw.timeLimitMs) || 2000,
          testCases,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "predict_output" || type === "predict-output") {
      items.forEach((raw, idx) => {
        const code = String(raw.code || "").trim();
        if (!code) return;
        bank.predictOutput.push({
          id: String(raw.id || `${act.id || "db-pred"}-${idx}`),
          title: String(raw.title || act.title || `Đoán Output #${idx + 1}`),
          difficulty: normalizeDifficultyVi(raw.difficulty),
          description: String(raw.description || act.description || "Đọc đoạn mã JS và tự dự đoán chính xác những gì console.log sẽ in ra."),
          code,
          expectedOutput: String(raw.expectedOutput ?? ""),
          acceptedOutputs: Array.isArray(raw.acceptedOutputs)
            ? raw.acceptedOutputs.map(String)
            : undefined,
          hint: raw.hint ? String(raw.hint) : undefined,
          explanation: String(raw.explanation || "Theo dõi từng dòng thực thi và trạng thái biến trong bộ nhớ."),
          timeLimitMs: Number(raw.timeLimitMs) || 2000,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
          language: raw.language ? String(raw.language).trim().toLowerCase() : undefined,
        });
      });
    } else if (type === "bug_fix" || type === "bug-fix" || type === "bug_hunt" || type === "bug-hunter") {
      items.forEach((raw, idx) => {
        const buggyCode = String(raw.buggyCode || raw.codeSnippet || "").trim();
        if (!buggyCode) return;
        const rawCases = Array.isArray(raw.testCases) ? raw.testCases : [];
        const testCases: JsSandboxTestCase[] | undefined =
          rawCases.length > 0
            ? rawCases.map((tc: any, tcIdx: number) => ({
                name: String(tc.name || tc.description || `Test case #${tcIdx + 1}`),
                input: Array.isArray(tc.input) ? tc.input : [tc.input],
                expected: tc.expected,
              }))
            : undefined;

        const inferredFn =
          String(raw.functionName || "").trim() ||
          buggyCode.match(/function\s+([A-Za-z_$][A-Za-z0-9_$]*)/)?.[1];

        bank.bugFix.push({
          id: String(raw.id || raw.bugKey || `${act.id || "db-bug"}-${idx}`),
          title: String(raw.title || act.title || `Sửa Bug #${idx + 1}`),
          language: String(raw.language || "javascript").toLowerCase(),
          categoryTag: String(raw.categoryTag || "JavaScript Debugging"),
          context: String(raw.context || act.description || "Tìm nguyên nhân lỗi và sửa lại đoạn mã bên dưới."),
          buggyCode,
          bugLineNumber: typeof raw.bugLineNumber === "number"
            ? raw.bugLineNumber
            : typeof raw.bugLine === "number"
            ? raw.bugLine
            : undefined,
          bugBriefDescription: raw.bugBriefDescription ? String(raw.bugBriefDescription) : undefined,
          rootCauseAnalysis: raw.rootCauseAnalysis ? String(raw.rootCauseAnalysis) : String(raw.explanation || ""),
          realWorldImpact: raw.realWorldImpact ? String(raw.realWorldImpact) : undefined,
          cleanFixedCode: String(raw.cleanFixedCode || ""),
          acceptedSolutions: Array.isArray(raw.acceptedSolutions)
            ? raw.acceptedSolutions.map(String)
            : undefined,
          requiredTokens: Array.isArray(raw.requiredTokens)
            ? raw.requiredTokens.map(String)
            : undefined,
          forbiddenTokens: Array.isArray(raw.forbiddenTokens)
            ? raw.forbiddenTokens.map(String)
            : undefined,
          functionName: inferredFn,
          testCases,
          explanation: String(raw.explanation || raw.rootCauseAnalysis || ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "fill_code" || type === "fill-code") {
      items.forEach((raw, idx) => {
        const codeTemplate = Array.isArray(raw.codeTemplate) ? raw.codeTemplate.map(String) : [];
        const blanksRaw = Array.isArray(raw.blanks) ? raw.blanks : [];
        if (codeTemplate.length === 0 || blanksRaw.length === 0) return;

        const blanks: FillCodeBlankSpec[] = blanksRaw.map((b: any, bIdx: number) => ({
          id: String(b.id || `b${bIdx}`),
          blankIndex: typeof b.blankIndex === "number" ? b.blankIndex : bIdx,
          expected: String(b.expected ?? ""),
          acceptableAnswers: Array.isArray(b.acceptableAnswers)
            ? b.acceptableAnswers.map(String)
            : [String(b.expected ?? "")],
          hint: String(b.hint || `Điền phần mã còn thiếu #${bIdx + 1}`),
          chips: Array.isArray(b.chips) ? b.chips.map(String) : [String(b.expected ?? "")],
          explanation: String(b.explanation || ""),
        }));

        const rawCat = String(raw.category || "").toLowerCase();
        const rawLang = String(raw.language || "").toLowerCase();
        let category: "dotnet" | "react" | "sql" | "os";
        if (rawCat === "dotnet" || rawCat === "sql" || rawCat === "os" || rawCat === "react") {
          category = rawCat;
        } else {
          category = detectCodingCategory({ fillCodeCategory: rawCat, language: rawLang });
        }

        bank.fillCode.push({
          id: String(raw.id || `${act.id || "db-fill"}-${idx}`),
          title: String(raw.title || act.title || `Điền Code #${idx + 1}`),
          category,
          language: String(raw.language || "javascript"),
          description: String(raw.description || act.description || ""),
          practicalScenario: String(raw.practicalScenario || act.description || "Điền phần code còn thiếu để hoàn chỉnh đoạn xử lý."),
          codeTemplate,
          blanks,
          lineExplanations: Array.isArray(raw.lineExplanations) ? raw.lineExplanations : [],
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "parsons") {
      items.forEach((raw, idx) => {
        let correctLines: string[] = [];
        let initialShuffled: string[] = [];

        if (Array.isArray(raw.correctLines) && raw.correctLines.length > 0) {
          correctLines = raw.correctLines.map(String);
          initialShuffled =
            Array.isArray(raw.initialShuffled) && raw.initialShuffled.length === correctLines.length
              ? raw.initialShuffled.map(String)
              : deterministicShuffleLines(correctLines);
        } else if (Array.isArray(raw.lines) && raw.lines.length > 0) {
          const sorted = [...raw.lines].sort(
            (a: any, b: any) => (Number(a.correctOrder) || 0) - (Number(b.correctOrder) || 0)
          );
          correctLines = sorted.map((l: any) => String(l.text ?? ""));
          initialShuffled = deterministicShuffleLines(correctLines);
        }

        if (correctLines.length === 0) return;

        bank.parsons.push({
          id: String(raw.id || `${act.id || "db-parsons"}-${idx}`),
          title: String(raw.title || act.title || `Parsons Problem #${idx + 1}`),
          category: String(raw.category || "Thuật toán & JavaScript"),
          goal: String(raw.goal || act.description || "Sắp xếp các dòng code theo đúng trình tự logic."),
          explanation: String(
            raw.explanation || "Kiểm tra thứ tự khai báo biến, điều kiện vòng lặp và giá trị trả về."
          ),
          correctLines,
          initialShuffled,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    }
  }

  return bank;
}

export function normalizeCodeWhitespace(code: string): string {
  return (code || "")
    .replace(/\r\n/g, "\n")
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
    .join("\n")
    .replace(/[ \t]+/g, " ")
    .replace(/\s*([{}();,:])\s*/g, "$1")
    .trim();
}

export function normalizeOutputLine(line: string): string {
  let s = (line ?? "").trim();
  // Unify smart quotes and single quotes around string tokens to double quotes for fair comparison
  s = s.replace(/[’‘]/g, "'").replace(/[“”]/g, '"');
  // Normalize JSON-like spacing: [1, 2, 3] -> [1,2,3], { a: 1 } -> {a:1}
  if (/^[\[{]/.test(s) && /[\]}]$/.test(s)) {
    s = s.replace(/'/g, '"').replace(/\s*,\s*/g, ",").replace(/\s*:\s*/g, ":").replace(/\[\s+/g, "[").replace(/\s+\]/g, "]").replace(/\{\s+/g, "{").replace(/\s+\}/g, "}");
  } else {
    // Strip wrapping single/double quotes if user wrote "hello" for console.log("hello") -> hello
    if ((s.startsWith('"') && s.endsWith('"')) || (s.startsWith("'") && s.endsWith("'"))) {
      s = s.slice(1, -1).trim();
    }
    s = s.replace(/'/g, '"').replace(/\s+/g, " ");
  }
  return s;
}

export function normalizeOutputBlock(output: string): string[] {
  return (output || "")
    .replace(/\r\n/g, "\n")
    .split("\n")
    .map((l) => l.trim())
    .filter((l) => l.length > 0)
    .map(normalizeOutputLine);
}

export interface PredictOutputEvaluationResult {
  isCorrect: boolean;
  score: number;
  matchedLines: number;
  totalExpectedLines: number;
  normalizedUserLines: string[];
  normalizedExpectedLines: string[];
  feedbackVi: string;
}

export function evaluatePredictOutput(
  userPrediction: string,
  actualOutput: string,
  acceptedOutputs?: string[]
): PredictOutputEvaluationResult {
  const userLines = normalizeOutputBlock(userPrediction);
  const candidates = [actualOutput, ...(acceptedOutputs || [])].filter(
    (c) => typeof c === "string"
  );

  let bestMatched = 0;
  let bestExpected = normalizeOutputBlock(actualOutput);
  let exactMatch = false;

  for (const cand of candidates) {
    const expLines = normalizeOutputBlock(cand);
    if (expLines.length === 0 && userLines.length === 0) {
      exactMatch = true;
      bestExpected = expLines;
      break;
    }
    if (
      expLines.length === userLines.length &&
      expLines.every((line, i) => line === userLines[i])
    ) {
      exactMatch = true;
      bestMatched = expLines.length;
      bestExpected = expLines;
      break;
    }
    // Also check flat single-line match when user wrote outputs separated by spaces/commas
    const flatUser = userLines.join(" ");
    const flatExp = expLines.join(" ");
    if (flatUser && flatUser === flatExp) {
      exactMatch = true;
      bestMatched = expLines.length;
      bestExpected = expLines;
      break;
    }

    let matched = 0;
    const minLen = Math.min(expLines.length, userLines.length);
    for (let i = 0; i < minLen; i++) {
      if (expLines[i] === userLines[i]) matched++;
    }
    if (matched >= bestMatched) {
      bestMatched = matched;
      bestExpected = expLines;
    }
  }

  const totalExpected = Math.max(1, bestExpected.length);
  const score = exactMatch
    ? 100
    : userLines.length === 0
    ? 0
    : Math.max(
        0,
        Math.min(
          95,
          Math.round((bestMatched / Math.max(totalExpected, userLines.length)) * 100)
        )
      );

  const feedbackVi = exactMatch
    ? "Chính xác tuyệt đối! Dự đoán của bạn khớp hoàn toàn với kết quả thực thi thực tế trong Web Worker."
    : userLines.length === 0
    ? "Bạn chưa nhập dự đoán output trước khi chạy thử."
    : `Khớp ${bestMatched}/${bestExpected.length} dòng output. Hãy đối chiếu từng dòng kết quả thực tế từ Web Worker và xem giải thích bên dưới.`;

  return {
    isCorrect: exactMatch,
    score,
    matchedLines: bestMatched,
    totalExpectedLines: bestExpected.length,
    normalizedUserLines: userLines,
    normalizedExpectedLines: bestExpected,
    feedbackVi,
  };
}

export function normalizeCodeBlankValue(val: string): string {
  return (val || "")
    .trim()
    .replace(/[’‘]/g, "'")
    .replace(/[“”]/g, '"')
    .replace(/\s+/g, " ")
    .toLowerCase();
}

export interface FillCodeEvaluationResult {
  score: number;
  correctCount: number;
  totalBlanks: number;
  isAllCorrect: boolean;
  results: Record<string, boolean>;
  missedBlanks: Array<{ id: string; expected: string; explanation: string }>;
}

export function evaluateFillCodeBlanks(
  blanks: Array<{
    id: string;
    expected: string;
    acceptableAnswers?: string[];
    explanation?: string;
  }>,
  userInputs: Record<string, string>
): FillCodeEvaluationResult {
  const results: Record<string, boolean> = {};
  const missedBlanks: Array<{ id: string; expected: string; explanation: string }> = [];
  let correctCount = 0;

  for (const blank of blanks) {
    const rawUser = userInputs[blank.id] ?? "";
    const normUser = normalizeCodeBlankValue(rawUser);
    const normUserCompact = normUser.replace(/\s+/g, "");

    const accepted = [blank.expected, ...(blank.acceptableAnswers || [])];
    const isOk = accepted.some((ans) => {
      const normAns = normalizeCodeBlankValue(ans);
      if (normUser === normAns) return true;
      // Handle empty-array / blank-whitespace equivalence cleanly (e.g. "[]" vs "" when expected is "")
      if (normAns === "" && (normUser === "" || normUser === "[]")) return true;
      if (normUserCompact === normAns.replace(/\s+/g, "")) return true;
      return false;
    });

    results[blank.id] = isOk;
    if (isOk) {
      correctCount++;
    } else {
      missedBlanks.push({
        id: blank.id,
        expected: blank.expected,
        explanation: blank.explanation || "",
      });
    }
  }

  const totalBlanks = blanks.length;
  const score = totalBlanks > 0 ? Math.round((correctCount / totalBlanks) * 100) : 0;

  return {
    score,
    correctCount,
    totalBlanks,
    isAllCorrect: totalBlanks > 0 && correctCount === totalBlanks,
    results,
    missedBlanks,
  };
}

export interface BugFixStaticEvaluationResult {
  score: number;
  isPassed: boolean;
  isUnchanged: boolean;
  matchedRequired: string[];
  missingRequired: string[];
  remainingForbidden: string[];
  feedbackVi: string;
}

export function evaluateBugFixStatic(
  userCode: string,
  item: Pick<
    BugFixWriteInItem,
    "buggyCode" | "cleanFixedCode" | "acceptedSolutions" | "requiredTokens" | "forbiddenTokens"
  >
): BugFixStaticEvaluationResult {
  const normUser = normalizeCodeWhitespace(userCode);
  const normBuggy = normalizeCodeWhitespace(item.buggyCode);

  if (!normUser) {
    return {
      score: 0,
      isPassed: false,
      isUnchanged: false,
      matchedRequired: [],
      missingRequired: item.requiredTokens || [],
      remainingForbidden: [],
      feedbackVi: "Bạn chưa nhập mã nguồn đã sửa.",
    };
  }

  if (normUser === normBuggy) {
    return {
      score: 0,
      isPassed: false,
      isUnchanged: true,
      matchedRequired: [],
      missingRequired: item.requiredTokens || [],
      remainingForbidden: item.forbiddenTokens || [],
      feedbackVi: "Mã nguồn vẫn giống hệt bản chứa lỗi ban đầu. Hãy chỉnh sửa trực tiếp dòng gây bug rồi kiểm tra lại.",
    };
  }

  const candidateSolutions = [item.cleanFixedCode, ...(item.acceptedSolutions || [])]
    .filter((s) => typeof s === "string" && s.trim().length > 0)
    .map(normalizeCodeWhitespace);

  if (candidateSolutions.some((sol) => sol === normUser)) {
    return {
      score: 100,
      isPassed: true,
      isUnchanged: false,
      matchedRequired: item.requiredTokens || [],
      missingRequired: [],
      remainingForbidden: [],
      feedbackVi: "Chính xác 100%! Bản sửa của bạn khớp hoàn toàn với lời giải chuẩn Production.",
    };
  }

  const lowerUser = userCode.toLowerCase();
  const compactUser = normUser.toLowerCase();
  const reqs = item.requiredTokens || [];
  const forbids = item.forbiddenTokens || [];

  const matchedRequired: string[] = [];
  const missingRequired: string[] = [];
  for (const tok of reqs) {
    const cleanTok = tok.trim();
    if (!cleanTok) continue;
    const alts = cleanTok.split("|").map((x) => x.trim().toLowerCase()).filter(Boolean);
    const hit = alts.some(
      (a) => lowerUser.includes(a) || compactUser.includes(a.replace(/\s+/g, ""))
    );
    if (hit) matchedRequired.push(alts[0]);
    else missingRequired.push(alts[0]);
  }

  const remainingForbidden: string[] = [];
  for (const fTok of forbids) {
    const cleanF = fTok.trim().toLowerCase();
    if (!cleanF) continue;
    if (lowerUser.includes(cleanF) || compactUser.includes(cleanF.replace(/\s+/g, ""))) {
      remainingForbidden.push(fTok.trim());
    }
  }

  if (reqs.length > 0 || forbids.length > 0) {
    const reqRatio = reqs.length > 0 ? matchedRequired.length / reqs.length : 1;
    const forbidRatio =
      forbids.length > 0 ? (forbids.length - remainingForbidden.length) / forbids.length : 1;
    const score = Math.round(reqRatio * 70 + forbidRatio * 30);
    const isPassed = score >= 70 && remainingForbidden.length === 0;
    const feedbackVi = isPassed
      ? `Tốt lắm! Đạt ${score}/100 — bạn đã loại bỏ đoạn gây bug và bổ sung đúng cơ chế xử lý an toàn.`
      : remainingForbidden.length > 0
      ? `Chưa đạt (${score}/100): Mã nguồn vẫn còn mẫu gây lỗi (${remainingForbidden.join(", ")}).`
      : `Chưa đạt (${score}/100): Còn thiếu thành phần sửa lỗi quan trọng (${missingRequired.join(", ")}).`;

    return {
      score,
      isPassed,
      isUnchanged: false,
      matchedRequired,
      missingRequired,
      remainingForbidden,
      feedbackVi,
    };
  }

  // Fallback token overlap against cleanFixedCode
  const refTokens = Array.from(
    new Set(
      item.cleanFixedCode
        .toLowerCase()
        .split(/[^a-z0-9_]+/)
        .filter((t) => t.length >= 3)
    )
  );
  const userTokenSet = new Set(
    lowerUser.split(/[^a-z0-9_]+/).filter((t) => t.length >= 3)
  );
  const overlap = refTokens.length > 0
    ? refTokens.filter((t) => userTokenSet.has(t)).length / refTokens.length
    : 0;
  const score = Math.max(0, Math.min(95, Math.round(overlap * 100)));
  const isPassed = score >= 75;

  return {
    score,
    isPassed,
    isUnchanged: false,
    matchedRequired,
    missingRequired,
    remainingForbidden,
    feedbackVi: isPassed
      ? `Đạt ${score}/100! Bản sửa đã bao phủ các thành phần trọng yếu của lời giải chuẩn.`
      : `Đạt ${score}/100. Bản sửa chưa đủ sát với cách khắc phục chuẩn — hãy xem phân tích Root Cause để đối chiếu.`,
  };
}

export interface ParsonsEvaluationResult {
  isAllCorrect: boolean;
  score: number;
  correctCount: number;
  totalLines: number;
  checkedLines: Array<{ text: string; isCorrectPosition: boolean }>;
}

export function evaluateParsonsOrder(
  correctLines: string[],
  submittedLines: string[]
): ParsonsEvaluationResult {
  let correctCount = 0;
  const checkedLines = submittedLines.map((text, idx) => {
    const isCorrect =
      idx < correctLines.length && text.trimEnd() === correctLines[idx].trimEnd();
    if (isCorrect) correctCount++;
    return { text, isCorrectPosition: isCorrect };
  });

  const totalLines = correctLines.length;
  const isAllCorrect =
    totalLines > 0 && submittedLines.length === totalLines && correctCount === totalLines;
  const score = totalLines > 0 ? Math.round((correctCount / totalLines) * 100) : 0;

  return {
    isAllCorrect,
    score,
    correctCount,
    totalLines,
    checkedLines,
  };
}

// ============================================================================
// SECTION 2: FINANCE RANDOMIZED HAND-CALCULATION GENERATOR & EVALUATOR
// ============================================================================

export type FinanceCalcType =
  | "compound"
  | "dca"
  | "pe"
  | "margin_of_safety"
  | "drawdown";

export interface FinanceCalcProblem {
  id: string;
  type: FinanceCalcType;
  seed: number;
  titleVi: string;
  promptVi: string;
  questionLabelVi: string;
  unit: "triệu đ" | "%" | "lần";
  expected: number;
  tolerancePercent: number;
  toleranceAbs: number;
  formulaVi: string;
  stepsVi: string[];
  variables: Record<string, number>;
  /** Drawdown problems ask both max drawdown % and recovery % needed to break even */
  secondaryQuestionLabelVi?: string;
  secondaryUnit?: "triệu đ" | "%" | "lần";
  secondaryExpected?: number;
}

export interface NumericEvaluationResult {
  isValid: boolean;
  isCorrect: boolean;
  parsedValue: number | null;
  expected: number;
  absError: number;
  relErrorPercent: number;
  score: number;
  feedbackVi: string;
}

function createPrng(seed: number): () => number {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

function pickFrom<T>(rng: () => number, items: readonly T[]): T {
  const idx = Math.floor(rng() * items.length) % items.length;
  return items[idx];
}

function round2(n: number): number {
  return Math.round(n * 100) / 100;
}

export function generateFinanceCalculationProblem(
  type: FinanceCalcType,
  seed?: number
): FinanceCalcProblem {
  const effectiveSeed =
    typeof seed === "number" && Number.isFinite(seed)
      ? Math.floor(Math.abs(seed))
      : Math.floor(Date.now() % 1_000_000_000) ^ Math.floor(Math.random() * 1_000_000_000);
  const rng = createPrng(effectiveSeed || 42);

  if (type === "compound") {
    const principal = pickFrom(rng, [50, 80, 100, 120, 150, 200, 250, 300]); // triệu đ
    const rate = pickFrom(rng, [7, 8, 9, 10, 11, 12, 14, 15]); // %/năm
    const years = pickFrom(rng, [3, 4, 5, 6, 8, 10, 12]);
    const factor = Math.pow(1 + rate / 100, years);
    const fv = round2(principal * factor);

    return {
      id: `calc-compound-${effectiveSeed}`,
      type,
      seed: effectiveSeed,
      titleVi: "Tự tính Lãi kép (Compound Interest)",
      promptVi: `Bạn đầu tư một lần số vốn ban đầu ${principal} triệu đ vào quỹ chỉ số với tỷ suất sinh lời kép bình quân ${rate}%/năm (ghép lãi hàng năm) và giữ nguyên trong ${years} năm.`,
      questionLabelVi: `Tổng giá trị tài sản sau ${years} năm là bao nhiêu?`,
      unit: "triệu đ",
      expected: fv,
      tolerancePercent: 1.5,
      toleranceAbs: 0.25,
      formulaVi: "FV = P × (1 + r)^n",
      stepsVi: [
        `Bước 1: Xác định tham số: Vốn gốc P = ${principal} triệu đ, lãi suất r = ${rate}% = ${(rate / 100).toFixed(2)}, số kỳ n = ${years} năm.`,
        `Bước 2: Tính hệ số tăng trưởng kép: (1 + ${(rate / 100).toFixed(2)})^${years} ≈ ${factor.toFixed(4)}.`,
        `Bước 3: Nhân với vốn gốc: FV = ${principal} × ${factor.toFixed(4)} ≈ ${fv} triệu đ (tiền lãi sinh ra ≈ ${round2(fv - principal)} triệu đ).`,
      ],
      variables: { principal, rate, years },
    };
  }

  if (type === "dca") {
    const monthly = pickFrom(rng, [2, 3, 4, 5, 6, 8, 10, 12]); // triệu đ/tháng
    const annualRate = pickFrom(rng, [8, 9, 10, 12, 14]); // %/năm
    const years = pickFrom(rng, [3, 5, 7, 10, 12]);
    const months = years * 12;
    const monthlyRate = annualRate / 100 / 12;
    const growthFactor = (Math.pow(1 + monthlyRate, months) - 1) / monthlyRate;
    const fv = round2(monthly * growthFactor);
    const totalPrincipal = monthly * months;

    return {
      id: `calc-dca-${effectiveSeed}`,
      type,
      seed: effectiveSeed,
      titleVi: "Tự tính Tích lũy đều hằng tháng (DCA)",
      promptVi: `Mỗi cuối tháng bạn trích đều đặn ${monthly} triệu đ để đầu tư DCA liên tục trong ${years} năm (${months} tháng) với lợi suất kỳ vọng ${annualRate}%/năm (ghép lãi theo tháng).`,
      questionLabelVi: `Tổng giá trị tài sản tích lũy sau ${years} năm là bao nhiêu?`,
      unit: "triệu đ",
      expected: fv,
      tolerancePercent: 1.5,
      toleranceAbs: 0.5,
      formulaVi: "FV = PMT × [((1 + i)^m - 1) / i]  với i = r / 12, m = n × 12",
      stepsVi: [
        `Bước 1: Đổi sang kỳ tháng: PMT = ${monthly} triệu đ/tháng, lãi suất tháng i = ${annualRate}% / 12 ≈ ${(monthlyRate * 100).toFixed(4)}% (${monthlyRate.toFixed(6)}), số tháng m = ${months}.`,
        `Bước 2: Tính hệ số dòng tiền đều: [((1 + ${monthlyRate.toFixed(6)})^${months} - 1) / ${monthlyRate.toFixed(6)}] ≈ ${growthFactor.toFixed(3)}.`,
        `Bước 3: FV = ${monthly} × ${growthFactor.toFixed(3)} ≈ ${fv} triệu đ (so với tổng vốn góp ${totalPrincipal} triệu đ → lãi kép +${round2(fv - totalPrincipal)} triệu đ).`,
      ],
      variables: { monthly, annualRate, years, months, totalPrincipal },
    };
  }

  if (type === "pe") {
    const modeVariant = rng() < 0.5 ? "ratio" : "target_price";
    if (modeVariant === "ratio") {
      const eps = pickFrom(rng, [2.5, 3.0, 3.5, 4.0, 4.5, 5.0, 6.0, 8.0]); // nghìn đ
      const multiplier = pickFrom(rng, [9, 10.5, 12, 13.5, 15, 16.5, 18, 20]);
      const price = round2(eps * multiplier); // nghìn đ
      const pe = round2(price / eps);

      return {
        id: `calc-pe-${effectiveSeed}`,
        type,
        seed: effectiveSeed,
        titleVi: "Tự tính Chỉ số Định giá P/E",
        promptVi: `Cổ phiếu công ty đang giao dịch ở mức giá ${price} nghìn đ/cổ phiếu. Thu nhập ròng trên mỗi cổ phiếu (EPS 4 quý gần nhất) đạt ${eps} nghìn đ.`,
        questionLabelVi: "Hệ số P/E hiện tại của cổ phiếu là bao nhiêu?",
        unit: "lần",
        expected: pe,
        tolerancePercent: 1.5,
        toleranceAbs: 0.2,
        formulaVi: "P/E = Giá thị trường mỗi cổ phiếu (P) / Thu nhập mỗi cổ phiếu (EPS)",
        stepsVi: [
          `Bước 1: Lấy Giá thị trường P = ${price} nghìn đ và EPS = ${eps} nghìn đ.`,
          `Bước 2: Tính P/E = ${price} / ${eps} = ${pe} lần.`,
          `Ý nghĩa: Nhà đầu tư đang chấp nhận trả ${pe} đồng cho mỗi 1 đồng lợi nhuận hàng năm của doanh nghiệp.`,
        ],
        variables: { price, eps },
      };
    }

    const netProfit = pickFrom(rng, [120, 150, 200, 250, 320, 400, 500]); // tỷ đ -> ask P/E or target valuation
    const peTarget = pickFrom(rng, [10, 12, 14, 15, 16, 18]);
    const epsThousands = pickFrom(rng, [3.2, 4.0, 4.5, 5.0, 6.0, 7.5]);
    const totalSharesMillions = Math.round((netProfit * 1000) / epsThousands) / 1000;
    const targetPriceK = round2(epsThousands * peTarget);
    // Express in triệu đ for a 1,000-share lot so unit stays within triệu đ / % / lần
    const lotValueMillions = round2((targetPriceK * 1000) / 1000);

    return {
      id: `calc-pe-${effectiveSeed}`,
      type,
      seed: effectiveSeed,
      titleVi: "Định giá Mục tiêu theo P/E hợp lý",
      promptVi: `Một doanh nghiệp đạt EPS dự phóng ${epsThousands} nghìn đ/cổ phiếu (quy mô khoảng ${totalSharesMillions.toFixed(1)} triệu cổ phiếu). Mức P/E hợp lý của ngành là ${peTarget} lần. Bạn dự định định giá một lô 1.000 cổ phiếu.`,
      questionLabelVi: "Giá trị hợp lý của lô 1.000 cổ phiếu theo P/E mục tiêu là bao nhiêu?",
      unit: "triệu đ",
      expected: lotValueMillions,
      tolerancePercent: 1.5,
      toleranceAbs: 0.2,
      formulaVi: "Giá mục tiêu 1 CP = EPS × P/E mục tiêu; Giá trị lô 1.000 CP (triệu đ) = (Giá 1 CP nghìn đ × 1.000) / 1.000",
      stepsVi: [
        `Bước 1: Tính giá hợp lý cho 1 cổ phiếu: ${epsThousands} nghìn đ × ${peTarget} = ${targetPriceK} nghìn đ/cổ phiếu.`,
        `Bước 2: Nhân với 1.000 cổ phiếu: ${targetPriceK} nghìn đ × 1.000 = ${lotValueMillions} triệu đ.`,
      ],
      variables: { epsThousands, peTarget, targetPriceK },
    };
  }

  if (type === "margin_of_safety") {
    const intrinsic = pickFrom(rng, [60, 75, 80, 100, 120, 140, 150, 160, 200]); // nghìn đ
    const discountPct = pickFrom(rng, [18, 22, 25, 28, 30, 35, 40, 45]);
    const marketPrice = round2(intrinsic * (1 - discountPct / 100));
    const mos = round2(((intrinsic - marketPrice) / intrinsic) * 100);

    return {
      id: `calc-mos-${effectiveSeed}`,
      type,
      seed: effectiveSeed,
      titleVi: "Tự tính Biên an toàn (Margin of Safety — Graham)",
      promptVi: `Sau khi phân tích dòng tiền tự do, bạn định giá giá trị nội tại (Intrinsic Value) của cổ phiếu là ${intrinsic} nghìn đ/CP. Hiện thị trường hoảng loạn khiến giá cổ phiếu giảm về ${marketPrice} nghìn đ/CP.`,
      questionLabelVi: "Biên an toàn (Margin of Safety) hiện tại là bao nhiêu %?",
      unit: "%",
      expected: mos,
      tolerancePercent: 1.5,
      toleranceAbs: 0.2,
      formulaVi: "Biên an toàn (%) = [(Giá trị nội tại - Giá thị trường) / Giá trị nội tại] × 100%",
      stepsVi: [
        `Bước 1: Tính phần chiết khấu tuyệt đối: ${intrinsic} - ${marketPrice} = ${round2(intrinsic - marketPrice)} nghìn đ.`,
        `Bước 2: Chia cho giá trị nội tại: ${round2(intrinsic - marketPrice)} / ${intrinsic} = ${((intrinsic - marketPrice) / intrinsic).toFixed(4)}.`,
        `Bước 3: Quy ra phần trăm: × 100% = ${mos}%. (Benjamin Graham thường yêu cầu tối thiểu 25%–30% biên an toàn).`,
      ],
      variables: { intrinsic, marketPrice },
    };
  }

  // drawdown: BOTH max drawdown % AND recovery % needed to break even
  const peak = pickFrom(rng, [100, 150, 200, 250, 300, 400, 500]); // triệu đ
  const dropPct = pickFrom(rng, [20, 25, 30, 35, 40, 45, 50, 60]);
  const trough = round2(peak * (1 - dropPct / 100));
  const drawdownPct = round2(((peak - trough) / peak) * 100);
  const recoveryPct = round2(((peak - trough) / trough) * 100);

  return {
    id: `calc-drawdown-${effectiveSeed}`,
    type: "drawdown",
    seed: effectiveSeed,
    titleVi: "Tự tính Max Drawdown & % Tăng Lại Để Hòa Vốn",
    promptVi: `Danh mục đầu tư của bạn từng đạt đỉnh ${peak} triệu đ nhưng trong đợt suy thoái thị trường đã giảm xuống đáy còn ${trough} triệu đ.`,
    questionLabelVi: "1. Mức sụt giảm tối đa (Max Drawdown) là bao nhiêu %?",
    unit: "%",
    expected: drawdownPct,
    tolerancePercent: 1.5,
    toleranceAbs: 0.2,
    secondaryQuestionLabelVi: "2. Từ đáy này, danh mục cần tăng bao nhiêu % để quay lại hòa vốn đỉnh cũ?",
    secondaryUnit: "%",
    secondaryExpected: recoveryPct,
    formulaVi: "Max Drawdown (%) = (Đỉnh - Đáy) / Đỉnh × 100%;  % Hòa vốn = (Đỉnh - Đáy) / Đáy × 100%",
    stepsVi: [
      `Bước 1 (Max Drawdown): (${peak} - ${trough}) / ${peak} × 100% = ${drawdownPct}%.`,
      `Bước 2 (% Cần tăng lại để hòa vốn): (${peak} - ${trough}) / ${trough} × 100% = ${recoveryPct}%.`,
      `Bài học rủi ro bất đối xứng: Giảm ${drawdownPct}% đòi hỏi mức tăng tới ${recoveryPct}% chỉ để quay lại vạch xuất phát!`,
    ],
    variables: { peak, trough, drawdownPct, recoveryPct },
  };
}

export function parseVietnameseNumericInput(raw: string | number): number | null {
  if (typeof raw === "number") {
    return Number.isFinite(raw) ? raw : null;
  }
  const s = String(raw || "").trim();
  if (!s) return null;

  // Remove unit suffixes & whitespace
  let cleaned = s
    .replace(/triệu\s*đ|trieu\s*d|triệu|trieu|nghìn\s*đ|nghin\s*d|vnđ|vnd|đ|lần|lan|%/gi, "")
    .replace(/\s+/g, "")
    .trim();

  // Handle Vietnamese decimal comma vs thousands separator:
  // If both '.' and ',' exist: whichever comes last is decimal separator
  if (cleaned.includes(",") && cleaned.includes(".")) {
    if (cleaned.lastIndexOf(",") > cleaned.lastIndexOf(".")) {
      cleaned = cleaned.replace(/\./g, "").replace(",", ".");
    } else {
      cleaned = cleaned.replace(/,/g, "");
    }
  } else if (cleaned.includes(",")) {
    // Single comma -> treat as decimal point unless clearly 3 digits grouped multiple times (e.g. 1,000,000)
    const parts = cleaned.split(",");
    if (parts.length > 2) {
      cleaned = cleaned.replace(/,/g, "");
    } else {
      cleaned = cleaned.replace(",", ".");
    }
  }

  const parsed = Number(cleaned);
  return Number.isFinite(parsed) ? parsed : null;
}

export function evaluateNumericAnswer(
  userInput: string | number,
  expected: number,
  tolerancePercent = 1.5,
  toleranceAbs = 0.2
): NumericEvaluationResult {
  const parsed = parseVietnameseNumericInput(userInput);
  if (parsed === null) {
    return {
      isValid: false,
      isCorrect: false,
      parsedValue: null,
      expected,
      absError: Math.abs(expected),
      relErrorPercent: 100,
      score: 0,
      feedbackVi: "Giá trị nhập vào chưa phải là số hợp lệ. Hãy nhập con số (ví dụ: 125.5 hoặc 125,5).",
    };
  }

  const absError = Math.abs(parsed - expected);
  const relErrorPercent =
    Math.abs(expected) > 1e-9 ? (absError / Math.abs(expected)) * 100 : absError * 100;
  const maxAllowedDiff = Math.max(
    Math.abs(expected) * (tolerancePercent / 100),
    toleranceAbs
  );
  const isCorrect = absError <= maxAllowedDiff + 1e-9;

  let score = 0;
  if (isCorrect) {
    score = 100;
  } else if (relErrorPercent <= tolerancePercent * 3 || absError <= toleranceAbs * 3) {
    score = 60;
  } else if (relErrorPercent <= 15) {
    score = 35;
  }

  const feedbackVi = isCorrect
    ? `Chính xác! Kết quả của bạn (${round2(parsed)}) nằm trong dung sai cho phép so với đáp án chuẩn ${round2(expected)} (sai số ${round2(relErrorPercent)}%).`
    : `Chưa chính xác: Bạn tính ra ${round2(parsed)}, đáp án chuẩn là ${round2(expected)} (sai số ${round2(relErrorPercent)}%, vượt dung sai ±${tolerancePercent}%). Hãy xem từng bước giải bên dưới.`;

  return {
    isValid: true,
    isCorrect,
    parsedValue: round2(parsed),
    expected: round2(expected),
    absError: round2(absError),
    relErrorPercent: round2(relErrorPercent),
    score,
    feedbackVi,
  };
}

// ============================================================================
// FINANCE DB ACTIVITY NORMALIZER
// ============================================================================

export interface FinanceCaseWriteInItem {
  id: string;
  title: string;
  scenario: string;
  options: string[];
  correctIndex: number;
  explanation: string;
  reasonPromptVi: string;
  expectedKeywords: string[];
  sampleReasoningVi: string;
  minWords: number;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface DecisionJournalPromptItem {
  id: string;
  title: string;
  scenario: string;
  expectedKeywords: string[];
  sampleThesisVi: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface NormalizedFinanceBank {
  cases: FinanceCaseWriteInItem[];
  decisionJournals: DecisionJournalPromptItem[];
  recommendedCalcTypes: FinanceCalcType[];
}

export function normalizeFinanceActivities(
  activities: RawDbPracticeActivity[]
): NormalizedFinanceBank {
  const bank: NormalizedFinanceBank = {
    cases: [],
    decisionJournals: [],
    recommendedCalcTypes: [],
  };

  for (const act of activities || []) {
    const type = (act.type || "").trim().toLowerCase();
    const cfg = safeParseConfigJson(act.configJson);
    if (!cfg) continue;

    const items = expandConfigItems(cfg);

    if (type === "investment_case" || type === "finance_case") {
      items.forEach((raw, idx) => {
        const scenario = String(raw.scenario || "").trim();
        const options = Array.isArray(raw.options) ? raw.options.map(String) : [];
        if (!scenario || options.length < 2) return;
        bank.cases.push({
          id: String(raw.id || `${act.id || "db-fcase"}-${idx}`),
          title: String(raw.title || act.title || `Tình huống đầu tư #${idx + 1}`),
          scenario,
          options,
          correctIndex: typeof raw.correctIndex === "number" ? raw.correctIndex : 0,
          explanation: String(raw.explanation || ""),
          reasonPromptVi: String(
            raw.reasonPromptVi ||
              "Tự viết lập luận của bạn: Vì sao bạn chọn phương án đó? Dựa trên nguyên lý tài chính nào?"
          ),
          expectedKeywords: Array.isArray(raw.expectedKeywords)
            ? raw.expectedKeywords.map(String)
            : ["rủi ro", "kỷ luật", "dòng tiền", "dài hạn"],
          sampleReasoningVi: String(raw.sampleExplanation || raw.explanation || ""),
          minWords: Number(raw.minWords) || 12,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "decision_journal" || type === "decision-journal") {
      items.forEach((raw, idx) => {
        bank.decisionJournals.push({
          id: String(raw.id || `${act.id || "db-dj"}-${idx}`),
          title: String(raw.title || act.title || "Nhật ký Quyết định Đầu tư 4 Trụ cột"),
          scenario: String(
            raw.scenario ||
              act.description ||
              "Ghi chép đầy đủ luận điểm đầu tư, định giá biên an toàn, kịch bản sai và kỷ luật cảm xúc trước khi giải ngân."
          ),
          expectedKeywords: Array.isArray(raw.expectedKeywords)
            ? raw.expectedKeywords.map(String)
            : ["biên an toàn", "rủi ro", "cắt lỗ", "dòng tiền", "kỷ luật"],
          sampleThesisVi: String(raw.sampleAnswer || raw.sampleThesisVi || ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "hand_calc" || type === "hand-calc") {
      const typesRaw = Array.isArray(cfg.problemTypes) ? cfg.problemTypes : [];
      for (const t of typesRaw) {
        const s = String(t).trim().toLowerCase() as FinanceCalcType;
        if (
          (s === "compound" ||
            s === "dca" ||
            s === "pe" ||
            s === "margin_of_safety" ||
            s === "drawdown") &&
          !bank.recommendedCalcTypes.includes(s)
        ) {
          bank.recommendedCalcTypes.push(s);
        }
      }
    }
  }

  return bank;
}

// ============================================================================
// SECTION 3: PSYCHOLOGY WRITE-IN PARSERS & EVALUATORS
// ============================================================================

export interface BiasSpotterItem {
  id: string;
  title: string;
  contextTag: string;
  scenario: string;
  canonicalNameVi: string;
  acceptedNames: string[];
  reasonKeywords: string[];
  minReasonWords?: number;
  sampleExplanation: string;
  counterMeasureVi: string;
  flashcardFront?: string;
  flashcardBack?: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface BiasSpotterEvaluationResult {
  score: number;
  isPassed: boolean;
  nameMatched: boolean;
  matchedAlias?: string;
  reasonEval: AiPracticeEvaluationResult;
  usedLiveEndpoint: boolean;
  gradingMode: WriteInGradingMode;
  gradingLabelVi: string;
  feedbackVi: string;
}

function stripBiasPrefix(norm: string): string {
  return norm
    .replace(/^(thien kien|hieu ung|ao tuong|bay tam ly|bay nhan thuc|hoi chung|loi tu duy|loi nhan thuc)\s+/i, "")
    .replace(/\s+(bias|effect|fallacy)$/i, "")
    .trim();
}

export function matchAcceptedConceptName(
  userInput: string,
  acceptedNames: string[]
): { matched: boolean; matchedAlias?: string } {
  const normUser = normalizeViText(userInput);
  if (!normUser || normUser.length < 2) return { matched: false };
  const strippedUser = stripBiasPrefix(normUser);

  for (const rawAlias of acceptedNames || []) {
    const normAlias = normalizeViText(rawAlias);
    if (!normAlias) continue;
    const strippedAlias = stripBiasPrefix(normAlias);

    if (normUser === normAlias || (strippedUser && strippedUser === strippedAlias)) {
      return { matched: true, matchedAlias: rawAlias };
    }
    // Accept when user includes the full canonical alias inside a slightly longer phrase
    if (normAlias.length >= 4 && normUser.includes(normAlias)) {
      return { matched: true, matchedAlias: rawAlias };
    }
    if (strippedAlias.length >= 4 && strippedUser.includes(strippedAlias)) {
      return { matched: true, matchedAlias: rawAlias };
    }
  }

  return { matched: false };
}

export function evaluateBiasSpotterAnswer(
  item: BiasSpotterItem,
  userBiasName: string,
  userReason: string,
  aiReasonEval?: AiPracticeEvaluationResult | null
): BiasSpotterEvaluationResult {
  const nameCheck = matchAcceptedConceptName(userBiasName, [
    item.canonicalNameVi,
    ...item.acceptedNames,
  ]);

  const reasonEval: AiPracticeEvaluationResult =
    aiReasonEval ??
    evaluateKeywordsOffline({
      userResponse: userReason,
      expectedKeywords: item.reasonKeywords,
      minWords: item.minReasonWords ?? 8,
      sampleAnswer: item.sampleExplanation,
      contextLabel: item.title,
    });

  const namePoints = nameCheck.matched ? 50 : 0;
  const reasonPoints = Math.round(reasonEval.score * 0.5);
  let totalScore = Math.max(0, Math.min(100, namePoints + reasonPoints));
  // If bias name is completely wrong, cap below passing threshold so learner must identify the concept accurately
  if (!nameCheck.matched) {
    totalScore = Math.min(totalScore, 55);
  }
  const passing = item.passingScore ?? 70;
  const isPassed = nameCheck.matched && totalScore >= passing;

  const feedbackVi = nameCheck.matched
    ? `Bạn đã gọi tên chính xác "${item.canonicalNameVi}" (+50 điểm) và phần lý giải đạt ${reasonEval.score}/100 (${reasonEval.gradingLabelVi}).`
    : `Tên thiên kiến/khái niệm chưa đúng (Đáp án chuẩn: ${item.canonicalNameVi}). Phần giải thích đạt ${reasonEval.score}/100 (${reasonEval.gradingLabelVi}).`;

  return {
    score: totalScore,
    isPassed,
    nameMatched: nameCheck.matched,
    matchedAlias: nameCheck.matchedAlias,
    reasonEval,
    usedLiveEndpoint: reasonEval.usedLiveEndpoint,
    gradingMode: reasonEval.gradingMode,
    gradingLabelVi: reasonEval.gradingLabelVi,
    feedbackVi,
  };
}

export interface FeynmanPromptItem {
  id: string;
  conceptTitle: string;
  promptVi: string;
  requiredKeywords: string[];
  minWords: number;
  sampleAnswer: string;
  everydayExampleHintVi?: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface FeynmanEvaluationResult extends AiPracticeEvaluationResult {
  wordCount: number;
  minWords: number;
  meetsMinLength: boolean;
  isPassed: boolean;
}

export function evaluateFeynmanOffline(
  item: FeynmanPromptItem,
  userExplanation: string
): FeynmanEvaluationResult {
  const minWords = Math.max(6, item.minWords || 15);
  const wordCount = countMeaningfulWords(userExplanation);
  const meetsMinLength = wordCount >= minWords;

  const base = evaluateKeywordsOffline({
    userResponse: userExplanation,
    expectedKeywords: item.requiredKeywords,
    minWords,
    sampleAnswer: item.sampleAnswer,
    contextLabel: item.conceptTitle,
  });

  const passing = item.passingScore ?? 70;
  const isPassed = base.score >= passing;

  return {
    ...base,
    wordCount,
    minWords,
    meetsMinLength,
    isPassed,
  };
}

export interface CbtExerciseItem {
  id: string;
  title: string;
  scenario: string;
  sampleAutomaticThought: string;
  canonicalDistortionVi: string;
  acceptedDistortions: string[];
  distortionChoicesHint: string[];
  balancedKeywords: string[];
  minBalancedWords?: number;
  sampleBalancedThought: string;
  explanationVi: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface CbtExerciseEvaluationResult {
  score: number;
  isPassed: boolean;
  hasAutomaticThought: boolean;
  distortionMatched: boolean;
  matchedDistortionAlias?: string;
  balancedEval: AiPracticeEvaluationResult;
  usedLiveEndpoint: boolean;
  gradingMode: WriteInGradingMode;
  gradingLabelVi: string;
  feedbackVi: string;
}

export function evaluateCbtExercise(
  item: CbtExerciseItem,
  input: {
    automaticThought: string;
    distortionName: string;
    balancedThought: string;
  },
  aiBalancedEval?: AiPracticeEvaluationResult | null
): CbtExerciseEvaluationResult {
  const autoWords = countMeaningfulWords(input.automaticThought);
  const hasAutomaticThought = autoWords >= 4;

  const distortionCheck = matchAcceptedConceptName(input.distortionName, [
    item.canonicalDistortionVi,
    ...item.acceptedDistortions,
  ]);

  const balancedEval: AiPracticeEvaluationResult =
    aiBalancedEval ??
    evaluateKeywordsOffline({
      userResponse: input.balancedThought,
      expectedKeywords: item.balancedKeywords,
      minWords: item.minBalancedWords ?? 10,
      sampleAnswer: item.sampleBalancedThought,
      contextLabel: item.title,
    });

  const autoPoints = hasAutomaticThought ? 15 : autoWords > 0 ? 5 : 0;
  const distortionPoints = distortionCheck.matched ? 35 : 0;
  const balancedPoints = Math.round(balancedEval.score * 0.5);
  let totalScore = Math.max(0, Math.min(100, autoPoints + distortionPoints + balancedPoints));
  if (!distortionCheck.matched) {
    totalScore = Math.min(totalScore, 65);
  }

  const passing = item.passingScore ?? 70;
  const isPassed = distortionCheck.matched && hasAutomaticThought && totalScore >= passing;

  const feedbackVi = isPassed
    ? `Xuất sắc (${totalScore}/100)! Bạn đã gọi đúng lỗi nhận thức "${item.canonicalDistortionVi}" và tái cấu trúc được suy nghĩ cân bằng (${balancedEval.gradingLabelVi}).`
    : !distortionCheck.matched
    ? `Đạt ${totalScore}/100. Lỗi nhận thức chưa chuẩn (Đáp án chuẩn: ${item.canonicalDistortionVi}). Phần tái cấu trúc đạt ${balancedEval.score}/100 (${balancedEval.gradingLabelVi}).`
    : `Đạt ${totalScore}/100. Bạn đã nhận diện đúng lỗi nhận thức, hãy viết rõ hơn suy nghĩ tự động và lập luận cân bằng theo bằng chứng thực tế (${balancedEval.gradingLabelVi}).`;

  return {
    score: totalScore,
    isPassed,
    hasAutomaticThought,
    distortionMatched: distortionCheck.matched,
    matchedDistortionAlias: distortionCheck.matchedAlias,
    balancedEval,
    usedLiveEndpoint: balancedEval.usedLiveEndpoint,
    gradingMode: balancedEval.gradingMode,
    gradingLabelVi: balancedEval.gradingLabelVi,
    feedbackVi,
  };
}

export interface JournalFieldInput {
  label: string;
  value: string;
  minWords?: number;
  keywords?: string[];
}

export function evaluateJournalCriteriaOffline(
  fields: JournalFieldInput[],
  overallKeywords: string[] = [],
  sampleAnswer = ""
): AiPracticeEvaluationResult {
  const combined = fields
    .map((f) => `${f.label}: ${(f.value || "").trim()}`)
    .join("\n");

  const completedFields = fields.filter(
    (f) => countMeaningfulWords(f.value) >= (f.minWords ?? 6)
  );
  const completionRatio = fields.length > 0 ? completedFields.length / fields.length : 0;

  const allKeywords = Array.from(
    new Set([
      ...overallKeywords,
      ...fields.flatMap((f) => f.keywords || []),
    ])
  );
  const { matched, missing } = matchKeywordsVi(combined, allKeywords);
  const kwRatio = allKeywords.length > 0 ? matched.length / allKeywords.length : completionRatio;

  const score = Math.max(
    0,
    Math.min(100, Math.round(completionRatio * 60 + kwRatio * 40))
  );

  const strengths: string[] = [];
  const improvements: string[] = [];

  if (completedFields.length > 0) {
    strengths.push(
      `Đã hoàn thành rõ ràng ${completedFields.length}/${fields.length} mục cốt lõi.`
    );
  }
  const incompleteFields = fields.filter(
    (f) => countMeaningfulWords(f.value) < (f.minWords ?? 6)
  );
  if (incompleteFields.length > 0) {
    improvements.push(
      `Cần viết cụ thể hơn ở các mục: ${incompleteFields.map((f) => f.label).join(", ")}.`
    );
  }
  if (matched.length > 0) {
    strengths.push(`Đã lồng ghép từ khóa nguyên lý: ${matched.slice(0, 5).join(", ")}.`);
  }
  if (missing.length > 0) {
    improvements.push(`Gợi ý bổ sung góc nhìn: ${missing.slice(0, 4).join(", ")}.`);
  }

  return {
    score,
    gradingMode: "keyword_offline",
    usedLiveEndpoint: false,
    gradingLabelVi: WRITEIN_GRADING_LABELS.keyword_offline,
    feedbackVi: `${WRITEIN_GRADING_LABELS.keyword_offline}: Đạt ${score}/100 (hoàn thiện ${completedFields.length}/${fields.length} mục, khớp ${matched.length}/${allKeywords.length} từ khóa tiêu chí).`,
    correctedVersion: sampleAnswer,
    strengths,
    improvements,
    matchedKeywords: matched,
    missingKeywords: missing,
  };
}

export interface ApplyJournalPromptItem {
  id: string;
  title: string;
  promptVi: string;
  expectedKeywords: string[];
  samplePlanVi: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface NormalizedPsychologyBank {
  biasSpotters: BiasSpotterItem[];
  feynmanPrompts: FeynmanPromptItem[];
  cbtExercises: CbtExerciseItem[];
  applyJournals: ApplyJournalPromptItem[];
}

export function normalizePsychologyActivities(
  activities: RawDbPracticeActivity[]
): NormalizedPsychologyBank {
  const bank: NormalizedPsychologyBank = {
    biasSpotters: [],
    feynmanPrompts: [],
    cbtExercises: [],
    applyJournals: [],
  };

  for (const act of activities || []) {
    const type = (act.type || "").trim().toLowerCase();
    const cfg = safeParseConfigJson(act.configJson);
    if (!cfg) continue;

    const items = expandConfigItems(cfg);

    if (type === "bias_spotter" || type === "bias-spotter") {
      items.forEach((raw, idx) => {
        const scenario = String(raw.scenario || "").trim();
        const canonicalNameVi = String(raw.canonicalNameVi || raw.biasName || "").trim();
        if (!scenario || !canonicalNameVi) return;
        bank.biasSpotters.push({
          id: String(raw.id || `${act.id || "db-bias"}-${idx}`),
          title: String(raw.title || act.title || `Nhận diện Thiên kiến #${idx + 1}`),
          contextTag: String(raw.contextTag || "Tình huống thực tế"),
          scenario,
          canonicalNameVi,
          acceptedNames: Array.isArray(raw.acceptedNames)
            ? raw.acceptedNames.map(String)
            : [canonicalNameVi],
          reasonKeywords: Array.isArray(raw.reasonKeywords)
            ? raw.reasonKeywords.map(String)
            : [],
          minReasonWords: Number(raw.minReasonWords) || 8,
          sampleExplanation: String(raw.sampleExplanation || raw.explanation || ""),
          counterMeasureVi: String(raw.counterMeasureVi || ""),
          flashcardFront: raw.flashcardFront ? String(raw.flashcardFront) : undefined,
          flashcardBack: raw.flashcardBack ? String(raw.flashcardBack) : undefined,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "feynman" || type === "psych_feynman") {
      items.forEach((raw, idx) => {
        const conceptTitle = String(raw.conceptTitle || raw.title || act.title || "").trim();
        if (!conceptTitle) return;
        bank.feynmanPrompts.push({
          id: String(raw.id || `${act.id || "db-feyn"}-${idx}`),
          conceptTitle,
          promptVi: String(
            raw.promptVi ||
              act.description ||
              `Hãy tự giải thích khái niệm "${conceptTitle}" bằng ngôn từ đời thường của riêng bạn kèm một ví dụ thực tế.`
          ),
          requiredKeywords: Array.isArray(raw.requiredKeywords)
            ? raw.requiredKeywords.map(String)
            : [],
          minWords: Number(raw.minWords) || 15,
          sampleAnswer: String(raw.sampleAnswer || ""),
          everydayExampleHintVi: raw.everydayExampleHintVi
            ? String(raw.everydayExampleHintVi)
            : undefined,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "cbt_exercise" || type === "cbt-exercise") {
      items.forEach((raw, idx) => {
        const scenario = String(raw.scenario || "").trim();
        const canonicalDistortionVi = String(raw.canonicalDistortionVi || "").trim();
        if (!scenario || !canonicalDistortionVi) return;
        bank.cbtExercises.push({
          id: String(raw.id || `${act.id || "db-cbt"}-${idx}`),
          title: String(raw.title || act.title || `Bài tập CBT #${idx + 1}`),
          scenario,
          sampleAutomaticThought: String(raw.sampleAutomaticThought || ""),
          canonicalDistortionVi,
          acceptedDistortions: Array.isArray(raw.acceptedDistortions)
            ? raw.acceptedDistortions.map(String)
            : [canonicalDistortionVi],
          distortionChoicesHint: Array.isArray(raw.distortionChoicesHint)
            ? raw.distortionChoicesHint.map(String)
            : [],
          balancedKeywords: Array.isArray(raw.balancedKeywords)
            ? raw.balancedKeywords.map(String)
            : [],
          minBalancedWords: Number(raw.minBalancedWords) || 10,
          sampleBalancedThought: String(raw.sampleBalancedThought || ""),
          explanationVi: String(raw.explanationVi || ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "apply_journal" || type === "apply-journal") {
      items.forEach((raw, idx) => {
        bank.applyJournals.push({
          id: String(raw.id || `${act.id || "db-apply"}-${idx}`),
          title: String(raw.title || act.title || "Nhật ký Áp dụng & Giao tiếp NVC"),
          promptVi: String(
            raw.promptVi ||
              act.description ||
              "Chọn một tình huống sắp tới của bản thân và viết kế hoạch áp dụng bài học / lời thoại NVC 4 bước."
          ),
          expectedKeywords: Array.isArray(raw.expectedKeywords)
            ? raw.expectedKeywords.map(String)
            : ["quan sát", "cảm xúc", "nhu cầu", "đề nghị"],
          samplePlanVi: String(raw.samplePlanVi || raw.sampleAnswer || ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    }
  }

  return bank;
}
