import {
  detectCodingCategory,
  isAsciiOrNonCodeBlock,
  type CodingTopicCategory,
} from "./writein-practice.ts";

export { detectCodingCategory, isAsciiOrNonCodeBlock, type CodingTopicCategory };

export type LabMode = "english" | "code" | "finance" | "recall" | "applied";

export type AppliedTabMode = "prompt_critique" | "sql_reasoning" | "threat_spotter";

export const LAB_MODE_LABELS_VI: Record<LabMode, string> = {
  english: "Ngoại ngữ · Nghe & Nói",
  code: "Lập trình · Code & Săn lỗi",
  finance: "Tài chính · Tính toán & Biểu đồ",
  recall: "Kỹ năng & Tình huống",
  applied: "AI, Dữ liệu & An toàn số",
};

export const APPLIED_TAB_LABELS_VI: Record<AppliedTabMode, string> = {
  prompt_critique: "Phản biện & Viết lại Prompt",
  sql_reasoning: "Truy vấn & Chẩn đoán SQL",
  threat_spotter: "Nhận diện Lừa đảo & An toàn số",
};

export type EnglishTabMode =
  | "shadowing"
  | "dictation"
  | "speaking"
  | "word_map"
  | "cloze"
  | "translate"
  | "grammar_fix"
  | "roleplay"
  | "quiz";

export type CodingTabMode =
  | "fill-code"
  | "bug-hunter"
  | "system-visualizer"
  | "lesson-tasks"
  | "js-sandbox"
  | "parsons"
  | "predict-output";

export type FinanceTabMode =
  | "budget"
  | "compound"
  | "debt"
  | "trading"
  | "cases"
  | "portfolio"
  | "behavioral"
  | "hand-calc"
  | "decision-journal";

export type PsychTabMode =
  | "roleplay"
  | "biases"
  | "cbt"
  | "lesson-cases"
  | "cognitive-labs"
  | "bias-spotter"
  | "feynman"
  | "cbt-exercise"
  | "apply-journal";

export type PsychCogSubTab = "stroop" | "memory-span" | "cbt-journal";

export interface ResolvedPracticeTarget {
  labMode: LabMode;
  englishTab?: EnglishTabMode;
  codingTab?: CodingTabMode;
  financeTab?: FinanceTabMode;
  psychTab?: PsychTabMode;
  psychCogSubTab?: PsychCogSubTab;
  recallSubMode?: "psychology" | "feynman";
  appliedTab?: AppliedTabMode;
}

function stripDiacriticsLower(input: string): string {
  return (input || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function isPsychologyCourseOrLesson(courseTitle = "", lessonTitle = "", md = ""): boolean {
  const combined = stripDiacriticsLower(`${courseTitle} ${lessonTitle} ${md.slice(0, 1200)}`);
  return /\b(tam ly|ky nang|giao tiep|thao tung|nhan thuc|cbt|life skills|psychology|cam xuc|stress|tu duy|thuyet trinh|dam phan|attachment|eq|nvc)\b/.test(
    combined
  );
}

/**
 * Resolves a backend activityType + optional activityKey (e.g. from /app/practice query params)
 * into the exact top-level LabMode and sub-tab for LessonPracticeLab and its sub-labs.
 */
export function resolvePracticeSelection(
  activityType?: string | null,
  activityKey?: string | null,
  courseTitle = "",
  lessonTitle = "",
  lessonMarkdown = ""
): ResolvedPracticeTarget | null {
  const rawType = (activityType || "").trim().toLowerCase();
  const rawKey = (activityKey || "").trim().toLowerCase();
  if (!rawType && !rawKey) return null;

  // English / Language modes
  if (rawType === "shadowing") return { labMode: "english", englishTab: "shadowing" };
  if (rawType === "dictation") return { labMode: "english", englishTab: "dictation" };
  if (rawType === "pronunciation" || rawType === "speaking") {
    return { labMode: "english", englishTab: "speaking" };
  }
  if (rawType === "word_map") return { labMode: "english", englishTab: "word_map" };
  if (rawType === "cloze") return { labMode: "english", englishTab: "cloze" };
  if (rawType === "translate") return { labMode: "english", englishTab: "translate" };
  if (rawType === "grammar_fix") return { labMode: "english", englishTab: "grammar_fix" };
  if (rawType === "quiz") return { labMode: "english", englishTab: "quiz" };

  if (rawType === "roleplay") {
    const looksLikeLanguage =
      rawKey.startsWith("roleplay-") ||
      rawKey.startsWith("ja-") ||
      rawKey.startsWith("zh-") ||
      rawKey.startsWith("en-") ||
      /\b(ielts|tieng anh|english|tieng nhat|japanese|jlpt|n5|hiragana|katakana|tieng trung|chinese|hsk|hsk1|pinyin)\b/.test(
        stripDiacriticsLower(`${courseTitle} ${lessonTitle}`)
      );
    if (looksLikeLanguage) {
      return { labMode: "english", englishTab: "roleplay" };
    }
    if (rawKey.startsWith("psych-") || isPsychologyCourseOrLesson(courseTitle, lessonTitle, lessonMarkdown)) {
      return { labMode: "recall", recallSubMode: "psychology", psychTab: "roleplay" };
    }
    return { labMode: "english", englishTab: "roleplay" };
  }

  // Coding modes
  if (rawType === "js_sandbox" || rawType === "js-sandbox" || rawType === "code_write") {
    return { labMode: "code", codingTab: "js-sandbox" };
  }
  if (rawType === "predict_output" || rawType === "predict-output") {
    return { labMode: "code", codingTab: "predict-output" };
  }
  if (rawType === "parsons") return { labMode: "code", codingTab: "parsons" };
  if (
    rawType === "bug_hunt" ||
    rawType === "bug-hunter" ||
    rawType === "bug_fix" ||
    rawType === "bug-fix"
  ) {
    return { labMode: "code", codingTab: "bug-hunter" };
  }
  if (rawType === "fill-code" || rawType === "fill_code" || rawType === "code" || rawType === "coding") {
    return { labMode: "code", codingTab: "fill-code" };
  }
  if (rawType === "system-visualizer" || rawType === "system_visualizer") {
    return { labMode: "code", codingTab: "system-visualizer" };
  }
  if (rawType === "lesson-tasks" || rawType === "lesson_tasks") {
    return { labMode: "code", codingTab: "lesson-tasks" };
  }

  // Finance modes
  if (rawType === "hand_calc" || rawType === "hand-calc" || rawType === "finance_calc") {
    return { labMode: "finance", financeTab: "hand-calc" };
  }
  if (rawType === "decision_journal" || rawType === "decision-journal") {
    return { labMode: "finance", financeTab: "decision-journal" };
  }
  if (rawType === "compound_calculator" || rawType === "compound") {
    return { labMode: "finance", financeTab: "compound" };
  }
  if (rawType === "portfolio_sim" || rawType === "portfolio") {
    return { labMode: "finance", financeTab: "portfolio" };
  }
  if (rawType === "behavioral_quiz" || rawType === "behavioral") {
    return { labMode: "finance", financeTab: "behavioral" };
  }
  if (rawType === "investment_case" || rawType === "finance_case" || rawType === "cases") {
    return { labMode: "finance", financeTab: "cases" };
  }
  if (rawType === "budget") return { labMode: "finance", financeTab: "budget" };
  if (rawType === "debt") return { labMode: "finance", financeTab: "debt" };
  if (rawType === "trading") return { labMode: "finance", financeTab: "trading" };

  // Applied Systems modes (AI Workflows, Data & SQL Reasoning, Cybersecurity Threat Spotter)
  if (rawType === "prompt_critique" || rawType === "prompt-critique") {
    return { labMode: "applied", appliedTab: "prompt_critique" };
  }
  if (rawType === "sql_reasoning" || rawType === "sql-reasoning") {
    return { labMode: "applied", appliedTab: "sql_reasoning" };
  }
  if (rawType === "threat_spotter" || rawType === "threat-spotter") {
    return { labMode: "applied", appliedTab: "threat_spotter" };
  }

  // Psychology / Recall modes
  if (rawType === "bias_spotter" || rawType === "bias-spotter") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "bias-spotter" };
  }
  if (rawType === "cbt_exercise" || rawType === "cbt-exercise") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "cbt-exercise" };
  }
  if (rawType === "apply_journal" || rawType === "apply-journal") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "apply-journal" };
  }
  if (rawType === "stroop_test" || rawType === "stroop") {
    return {
      labMode: "recall",
      recallSubMode: "psychology",
      psychTab: "cognitive-labs",
      psychCogSubTab: "stroop",
    };
  }
  if (rawType === "memory_span" || rawType === "memory-span") {
    return {
      labMode: "recall",
      recallSubMode: "psychology",
      psychTab: "cognitive-labs",
      psychCogSubTab: "memory-span",
    };
  }
  if (rawType === "reflection_journal" || rawType === "cbt-journal") {
    return {
      labMode: "recall",
      recallSubMode: "psychology",
      psychTab: "cognitive-labs",
      psychCogSubTab: "cbt-journal",
    };
  }
  if (rawType === "psych_scenario" || rawType === "lesson-cases") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "lesson-cases" };
  }
  if (rawType === "biases") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "biases" };
  }
  if (rawType === "cbt") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "cbt" };
  }
  if (rawType === "psych_feynman") {
    return { labMode: "recall", recallSubMode: "psychology", psychTab: "feynman" };
  }
  if (rawType === "feynman") {
    if (
      rawKey.startsWith("psych-") ||
      rawKey.startsWith("feynman-") ||
      isPsychologyCourseOrLesson(courseTitle, lessonTitle, lessonMarkdown)
    ) {
      return { labMode: "recall", recallSubMode: "psychology", psychTab: "feynman" };
    }
    return { labMode: "recall", recallSubMode: "feynman" };
  }

  return null;
}

const ACTIVITY_TYPE_LABELS_VI: Record<string, string> = {
  shadowing: "Nhại giọng",
  dictation: "Chép chính tả",
  pronunciation: "Phát âm",
  speaking: "Luyện nói",
  word_map: "Sơ đồ từ vựng",
  cloze: "Điền từ",
  translate: "Dịch câu",
  grammar_fix: "Sửa ngữ pháp",
  roleplay: "Nhập vai hội thoại",
  quiz: "Trắc nghiệm",
  js_sandbox: "Web Worker Sandbox",
  "js-sandbox": "Web Worker Sandbox",
  code_write: "Viết code JS",
  predict_output: "Đoán Output JS",
  "predict-output": "Đoán Output JS",
  parsons: "Ghép dòng code",
  bug_hunt: "Săn lỗi code",
  "bug-hunter": "Săn lỗi code",
  bug_fix: "Sửa Bug Write-in",
  "bug-fix": "Sửa Bug Write-in",
  fill_code: "Điền code",
  "fill-code": "Điền code",
  "system-visualizer": "Mô phỏng hệ thống",
  "lesson-tasks": "Nhiệm vụ code",
  hand_calc: "Tự tính tay tài chính",
  "hand-calc": "Tự tính tay tài chính",
  finance_calc: "Tự tính tay tài chính",
  decision_journal: "Nhật ký quyết định",
  "decision-journal": "Nhật ký quyết định",
  compound_calculator: "Tính lãi kép",
  compound: "Tính lãi kép",
  portfolio_sim: "Mô phỏng danh mục",
  portfolio: "Mô phỏng danh mục",
  behavioral_quiz: "Tâm lý đầu tư",
  behavioral: "Tâm lý đầu tư",
  investment_case: "Tình huống & Giải thích",
  finance_case: "Tình huống & Giải thích",
  budget: "Ngân sách 50/30/20",
  debt: "Lộ trình trả nợ",
  trading: "Biểu đồ nến",
  bias_spotter: "Nhận diện Thiên kiến",
  "bias-spotter": "Nhận diện Thiên kiến",
  feynman: "Feynman Tự định nghĩa",
  psych_feynman: "Feynman Tự định nghĩa",
  cbt_exercise: "Bài tập CBT tình huống",
  "cbt-exercise": "Bài tập CBT tình huống",
  apply_journal: "Nhật ký áp dụng",
  "apply-journal": "Nhật ký áp dụng",
  stroop_test: "Thí nghiệm Stroop",
  memory_span: "Trí nhớ ngắn hạn",
  reflection_journal: "Nhật ký CBT",
  psych_scenario: "Tình huống tâm lý",
  biases: "Bẫy tâm lý",
  cbt: "Gỡ rối CBT",
  prompt_critique: "Phản biện & Viết lại Prompt",
  "prompt-critique": "Phản biện & Viết lại Prompt",
  sql_reasoning: "Truy vấn & Chẩn đoán SQL",
  "sql-reasoning": "Truy vấn & Chẩn đoán SQL",
  threat_spotter: "Nhận diện Lừa đảo & An toàn số",
  "threat-spotter": "Nhận diện Lừa đảo & An toàn số",
  active_recall: "Tự nhắc lại chủ động",
  scenario: "Tình huống thực tế",
};

export function getPracticeActivityTypeLabel(activityType?: string | null): string {
  const key = (activityType || "").trim().toLowerCase();
  return ACTIVITY_TYPE_LABELS_VI[key] ?? (activityType || "Thực hành");
}

export function buildPracticeLessonHref(
  courseId: string,
  lessonId?: string | null,
  activityType?: string | null,
  activityKey?: string | null
): string {
  if (!courseId || !lessonId) {
    return "/app/practice";
  }
  const params = new URLSearchParams();
  if (activityType && activityType.trim()) {
    params.set("type", activityType.trim());
  }
  if (activityKey && activityKey.trim()) {
    params.set("key", activityKey.trim());
  }
  const qs = params.toString();
  return `/app/practice/${encodeURIComponent(courseId)}/${encodeURIComponent(lessonId)}${qs ? `?${qs}` : ""}`;
}

export interface QuickCheckQuestion {
  id: string;
  question: string;
  options: Array<{ key: string; text: string }>;
  correctKey: string;
  explanation: string;
}

function cleanSnippet(text: string): string {
  return text
    .replace(/\[(?:C\d+-\d+|LO-[A-Z0-9-]+)\]/gi, "")
    .replace(/[*_`>#]/g, "")
    .replace(/^(?:[-*+•]|\d+[.)])\s+/, "")
    .replace(/\s+/g, " ")
    .trim();
}

const STRUCTURAL_TITLES_PATTERN =
  /\b(muc tieu bai hoc|learning objectives|doan hoi thoai mau|model dialogue|ngu phap|tu vung|loi phat am|boi canh|tong ket|vi du|bai tap|muc luc|tai lieu tham khao|bai tiep|tong ket nhanh)\b/i;

/**
 * Extracts up to 2 lightweight multiple-choice quick-check questions from lesson Markdown
 * when a lesson does not yet have persisted PracticeQuiz items in the database.
 */
export function extractQuickCheckQuestions(
  lessonMarkdown: string,
  lessonTitle: string
): QuickCheckQuestion[] {
  const md = (lessonMarkdown || "").trim();
  const pairs: Array<{ term: string; desc: string }> = [];
  const seenTerms = new Set<string>();

  // 1. Definition bullets: - **Term**: Description
  const defRegex = /(?:^|\n)(?:[-*]\s*)?\*\*([^*\n:]{3,55})\*\*\s*[:–—\-]\s*([^\n]{12,200})/g;
  let match: RegExpExecArray | null;
  while ((match = defRegex.exec(md)) !== null) {
    const term = cleanSnippet(match[1]);
    const desc = cleanSnippet(match[2]);
    const norm = stripDiacriticsLower(term);
    if (
      term.length >= 3 &&
      desc.length >= 12 &&
      !seenTerms.has(norm) &&
      !norm.startsWith("lo-") &&
      !norm.startsWith("c0") &&
      !STRUCTURAL_TITLES_PATTERN.test(norm)
    ) {
      seenTerms.add(norm);
      pairs.push({ term, desc });
    }
    if (pairs.length >= 6) break;
  }

  // 2. Headings + first descriptive paragraph
  if (pairs.length < 4) {
    const headingRegex = /^#{2,3}\s+([^\n]{4,70})\r?\n+([^#\n`|<][^\n]{18,200})/gm;
    while ((match = headingRegex.exec(md)) !== null) {
      const term = cleanSnippet(match[1]);
      const desc = cleanSnippet(match[2]);
      const norm = stripDiacriticsLower(term);
      if (
        term.length >= 4 &&
        desc.length >= 18 &&
        !seenTerms.has(norm) &&
        !STRUCTURAL_TITLES_PATTERN.test(norm)
      ) {
        seenTerms.add(norm);
        pairs.push({ term, desc });
      }
      if (pairs.length >= 6) break;
    }
  }

  if (pairs.length === 0) {
    return [];
  }

  const fallbackDistractorDescs = [
    "Khái niệm không thuộc trọng tâm của bài giảng này và chỉ dùng để tham khảo phụ.",
    "Quy trình bỏ qua bước kiểm chứng thực tế để rút ngắn thời gian đọc lướt.",
    "Phương pháp ghi nhớ thụ động bằng cách chép lại nguyên văn mà không tự kiểm tra.",
  ];

  const questions: QuickCheckQuestion[] = [];
  const targetCount = Math.min(2, pairs.length);
  const keys = ["A", "B", "C", "D"];

  for (let i = 0; i < targetCount; i++) {
    const target = pairs[i];
    const otherDescs = pairs
      .filter((_, idx) => idx !== i)
      .map((p) => p.desc);
    const distractors = [...otherDescs, ...fallbackDistractorDescs]
      .filter((d) => d !== target.desc)
      .slice(0, 3);

    // Deterministic rotation by index so SSR/CSR match without random jitter
    const correctSlot = (i + target.term.length) % 4;
    const rawTexts: string[] = [];
    let dPtr = 0;
    for (let slot = 0; slot < 4; slot++) {
      if (slot === correctSlot) {
        rawTexts.push(target.desc);
      } else {
        rawTexts.push(distractors[dPtr++] || fallbackDistractorDescs[slot % fallbackDistractorDescs.length]);
      }
    }

    questions.push({
      id: `qc-${i + 1}`,
      question: `Trong bài "${lessonTitle}", ý nào mô tả đúng nhất về "${target.term}"?`,
      options: rawTexts.map((text, idx) => ({
        key: keys[idx],
        text: text.length > 150 ? `${text.slice(0, 147)}...` : text,
      })),
      correctKey: keys[correctSlot],
      explanation: `${target.term}: ${target.desc}`,
    });
  }

  return questions;
}
