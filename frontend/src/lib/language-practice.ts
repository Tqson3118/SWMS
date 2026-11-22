/**
 * Reusable Multi-Language Practice Schema & Pure Evaluation Engine
 * Supports en-US, en-GB, and prepares CJK support (ja-JP, zh-CN) for Step 3.
 * 100% backward-compatible with legacy PracticeActivity.ConfigJson payloads.
 */

export type LanguageCode = "en-US" | "en-GB" | "ja-JP" | "zh-CN";
export type DifficultyLevel = "easy" | "medium" | "hard";
export type DifficultyFilter = "all" | DifficultyLevel;

export const DIFFICULTY_LABELS: Record<DifficultyLevel, string> = {
  easy: "Dễ",
  medium: "Vừa",
  hard: "Khó",
};

export const DIFFICULTY_FILTER_OPTIONS: Array<{ id: DifficultyFilter; label: string }> = [
  { id: "all", label: "Tất cả" },
  { id: "easy", label: "Dễ" },
  { id: "medium", label: "Vừa" },
  { id: "hard", label: "Khó" },
];

export type LanguagePracticeActivityType =
  | "shadowing"
  | "dictation"
  | "pronunciation"
  | "speaking"
  | "word_map"
  | "cloze"
  | "translate"
  | "grammar_fix"
  | "roleplay"
  | "quiz";

export interface RawPracticeActivity {
  id?: string;
  type: string;
  title?: string;
  description?: string;
  configJson?: string;
  passingScore?: number;
  orderIndex?: number;
}

export interface SentenceDrillItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  topic: string;
  sentence: string;
  ipa: string;
  reading?: string;
  meaningVi: string;
  audioUrl: string | null;
  speakingPrompt: string;
  speakingOutline: string[];
  sampleAnswer: string;
  keyPhrases: string[];
  targetWords?: string[];
  wordChips?: string[];
  distractors?: string[];
  sourceActivityId?: string;
  passingScore?: number;
}

export interface WordMapNodeItem {
  text: string;
  ipa?: string;
  reading?: string;
  meaningVi: string;
  example?: string;
}

export interface WordMapPracticeItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  title: string;
  centralWord: string;
  ipa?: string;
  reading?: string;
  meaningVi: string;
  collocations: WordMapNodeItem[];
  phrasalVerbs: WordMapNodeItem[];
  synonyms: WordMapNodeItem[];
  contextExamples: Array<{ sentence: string; meaningVi: string }>;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface ClozeBlankSpec {
  id?: string;
  acceptedAnswers: string[];
  hintVi?: string;
  explanationVi?: string;
}

export interface ClozePracticeItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  title: string;
  promptVi: string;
  /** Supports {{0}}, {{1}} placeholders or sequential ___ blanks */
  template: string;
  blanks: ClozeBlankSpec[];
  fullSentence?: string;
  meaningVi: string;
  explanationVi?: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface TranslatePracticeItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  title: string;
  sourceVi: string;
  hintKeywords: string[];
  acceptedAnswers: string[];
  sampleAnswer: string;
  grammarNoteVi?: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface GrammarFixPracticeItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  title: string;
  incorrectSentence: string;
  errorTypeHintVi: string;
  correctedSentence: string;
  acceptedAnswers: string[];
  meaningVi?: string;
  explanationVi: string;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface RoleplayPracticeItem {
  id: string;
  lang: LanguageCode;
  difficulty: DifficultyLevel;
  title: string;
  scenarioVi: string;
  aiRole: string;
  aiPrompt: string;
  aiPromptVi: string;
  userGoalVi: string;
  expectedKeywords: string[];
  sampleResponses: string[];
  minWords: number;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface NormalizedLanguagePracticeBank {
  shadowing: SentenceDrillItem[];
  dictation: SentenceDrillItem[];
  pronunciation: SentenceDrillItem[];
  wordMaps: WordMapPracticeItem[];
  cloze: ClozePracticeItem[];
  translate: TranslatePracticeItem[];
  grammarFix: GrammarFixPracticeItem[];
  roleplay: RoleplayPracticeItem[];
}

// ============================================================================
// PRONUNCIATION & EVALUATION RESULT TYPES
// ============================================================================

export type WordPronunciationStatus = "correct" | "near" | "wrong";

export interface PronunciationWordResult {
  word: string;
  cleanTarget: string;
  matchedSpoken?: string;
  status: WordPronunciationStatus;
  distance: number;
}

export type VietnamesePronunciationTipCategory =
  | "th_sound"
  | "ending_sounds"
  | "s_es_ed_endings"
  | "confusable_pairs"
  | "cjk_tone_pitch";

export interface VietnamesePronunciationTip {
  id: string;
  category: VietnamesePronunciationTipCategory;
  title: string;
  adviceVi: string;
  exampleWords: string[];
}

export interface PronunciationEvaluationResult {
  score: number;
  words: PronunciationWordResult[];
  correctCount: number;
  nearCount: number;
  wrongCount: number;
  missedWords: string[];
  tips: VietnamesePronunciationTip[];
}

export interface ClozeTemplateSegment {
  type: "text" | "blank";
  text?: string;
  blankIndex?: number;
}

export interface ClozeBlankResult {
  index: number;
  userAnswer: string;
  acceptedAnswers: string[];
  isCorrect: boolean;
  hintVi?: string;
  explanationVi?: string;
}

export interface ClozeEvaluationResult {
  score: number;
  correctCount: number;
  totalBlanks: number;
  isAllCorrect: boolean;
  blanks: ClozeBlankResult[];
}

export interface TranslationEvaluationResult {
  score: number;
  isExactMatch: boolean;
  matchedAnswer: string;
  missingTokens: string[];
  extraTokens: string[];
  matchedKeywords: string[];
  missingKeywords: string[];
  feedbackVi: string;
}

export interface GrammarFixEvaluationResult {
  score: number;
  isExactMatch: boolean;
  isUnchangedError: boolean;
  bestReference: string;
  missingTokens: string[];
  wrongOrExtraTokens: string[];
  feedbackVi: string;
  explanationVi: string;
}

export type PracticeGradingMode = "ai_live" | "keyword_offline";

export const GRADING_LABELS: Record<PracticeGradingMode, string> = {
  ai_live: "AI chấm trực tiếp",
  keyword_offline: "Chấm theo từ khóa (offline)",
};

export interface RoleplayEvaluationResult {
  score: number;
  gradingMode: PracticeGradingMode;
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
// NORMALIZATION & TEXT UTILITIES (EN + CJK READY)
// ============================================================================

const VALID_LANGUAGES: readonly LanguageCode[] = ["en-US", "en-GB", "ja-JP", "zh-CN"];
const VALID_DIFFICULTIES: readonly DifficultyLevel[] = ["easy", "medium", "hard"];

export function normalizeLanguageCode(
  raw: unknown,
  fallback: LanguageCode = "en-US"
): LanguageCode {
  if (typeof raw !== "string") return fallback;
  const trimmed = raw.trim();
  if ((VALID_LANGUAGES as readonly string[]).includes(trimmed)) {
    return trimmed as LanguageCode;
  }
  const lower = trimmed.toLowerCase();
  if (lower === "en" || lower === "en_us" || lower === "en-us") return "en-US";
  if (lower === "en_gb" || lower === "en-gb" || lower === "uk") return "en-GB";
  if (lower === "ja" || lower === "jp" || lower === "ja_jp" || lower === "ja-jp") return "ja-JP";
  if (lower === "zh" || lower === "cn" || lower === "zh_cn" || lower === "zh-cn") return "zh-CN";
  return fallback;
}

export function normalizeDifficulty(
  raw: unknown,
  fallback: DifficultyLevel = "medium"
): DifficultyLevel {
  if (typeof raw !== "string") return fallback;
  const lower = raw.trim().toLowerCase();
  if ((VALID_DIFFICULTIES as readonly string[]).includes(lower)) {
    return lower as DifficultyLevel;
  }
  if (lower === "de" || lower === "dễ" || lower === "a1" || lower === "a2" || lower === "beginner") {
    return "easy";
  }
  if (lower === "kho" || lower === "khó" || lower === "b2" || lower === "c1" || lower === "advanced") {
    return "hard";
  }
  return fallback;
}

export function isCjkLanguage(lang: LanguageCode): boolean {
  return lang === "ja-JP" || lang === "zh-CN";
}

const ENGLISH_CONTRACTIONS: ReadonlyArray<[RegExp, string]> = [
  [/\bi'm\b/g, "i am"],
  [/\byou're\b/g, "you are"],
  [/\bhe's\b/g, "he is"],
  [/\bshe's\b/g, "she is"],
  [/\bit's\b/g, "it is"],
  [/\bwe're\b/g, "we are"],
  [/\bthey're\b/g, "they are"],
  [/\bthat's\b/g, "that is"],
  [/\bthere's\b/g, "there is"],
  [/\bwhat's\b/g, "what is"],
  [/\bwho's\b/g, "who is"],
  [/\bwhere's\b/g, "where is"],
  [/\bhere's\b/g, "here is"],
  [/\bi've\b/g, "i have"],
  [/\byou've\b/g, "you have"],
  [/\bwe've\b/g, "we have"],
  [/\bthey've\b/g, "they have"],
  [/\bi'll\b/g, "i will"],
  [/\byou'll\b/g, "you will"],
  [/\bhe'll\b/g, "he will"],
  [/\bshe'll\b/g, "she will"],
  [/\bwe'll\b/g, "we will"],
  [/\bthey'll\b/g, "they will"],
  [/\bit'll\b/g, "it will"],
  [/\bi'd\b/g, "i would"],
  [/\byou'd\b/g, "you would"],
  [/\bhe'd\b/g, "he would"],
  [/\bshe'd\b/g, "she would"],
  [/\bwe'd\b/g, "we would"],
  [/\bthey'd\b/g, "they would"],
  [/\blet's\b/g, "let us"],
  [/\bdon't\b/g, "do not"],
  [/\bdoesn't\b/g, "does not"],
  [/\bdidn't\b/g, "did not"],
  [/\bcan't\b/g, "cannot"],
  [/\bcannot\b/g, "can not"],
  [/\bwon't\b/g, "will not"],
  [/\bwouldn't\b/g, "would not"],
  [/\bcouldn't\b/g, "could not"],
  [/\bshouldn't\b/g, "should not"],
  [/\bisn't\b/g, "is not"],
  [/\baren't\b/g, "are not"],
  [/\bwasn't\b/g, "was not"],
  [/\bweren't\b/g, "were not"],
  [/\bhaven't\b/g, "have not"],
  [/\bhasn't\b/g, "has not"],
  [/\bhadn't\b/g, "had not"],
  [/\bmustn't\b/g, "must not"],
  [/\bneedn't\b/g, "need not"],
];

export function normalizeTextForComparison(
  input: string,
  lang: LanguageCode = "en-US"
): string {
  if (!input) return "";
  let s = input
    .normalize("NFKC")
    .replace(/[’‘`´]/g, "'")
    .replace(/[“”]/g, '"')
    .trim()
    .toLowerCase();

  if (isCjkLanguage(lang)) {
    // Strip CJK & ASCII punctuation and whitespace for clean CJK comparison
    return s
      .replace(/[\s。、，．！？；：「」『』（）()【】\[\],.!?;:'"~～\-—–·•]/g, "")
      .trim();
  }

  // Convert hyphens inside compound words ("long-term" -> "long term")
  s = s.replace(/[-–—]/g, " ");

  for (const [pattern, replacement] of ENGLISH_CONTRACTIONS) {
    s = s.replace(pattern, replacement);
  }

  // Strip remaining punctuation except internal apostrophes (possessives)
  s = s.replace(/[^a-z0-9'\s]/g, " ");
  s = s.replace(/(^|\s)'+|'+(\s|$)/g, " ");
  s = s.replace(/\s+/g, " ").trim();
  return s;
}

export function cleanWordToken(word: string, lang: LanguageCode = "en-US"): string {
  if (!word) return "";
  if (isCjkLanguage(lang)) {
    return normalizeTextForComparison(word, lang);
  }
  return word
    .normalize("NFKC")
    .replace(/[’‘`´]/g, "'")
    .toLowerCase()
    .replace(/[^a-z0-9']/g, "")
    .replace(/^'+|'+$/g, "")
    .trim();
}

export function tokenizeText(text: string, lang: LanguageCode = "en-US"): string[] {
  if (!text || !text.trim()) return [];
  if (isCjkLanguage(lang)) {
    const trimmed = text.trim();
    // If space-separated (e.g. romaji / pinyin / segmented), split by spaces; otherwise segment by characters
    if (/\s/.test(trimmed) && !/[\u3040-\u30ff\u3400-\u9fff]/.test(trimmed)) {
      return trimmed
        .split(/\s+/)
        .map((t) => cleanWordToken(t, "en-US"))
        .filter(Boolean);
    }
    const cleaned = normalizeTextForComparison(trimmed, lang);
    return Array.from(cleaned).filter(Boolean);
  }
  const normalized = normalizeTextForComparison(text, lang);
  return normalized ? normalized.split(/\s+/).filter(Boolean) : [];
}

export function levenshteinDistance(a: string, b: string): number {
  if (a === b) return 0;
  const m = a.length;
  const n = b.length;
  if (m === 0) return n;
  if (n === 0) return m;

  let prev = new Array<number>(n + 1);
  let curr = new Array<number>(n + 1);
  for (let j = 0; j <= n; j++) prev[j] = j;

  for (let i = 1; i <= m; i++) {
    curr[0] = i;
    const ca = a[i - 1];
    for (let j = 1; j <= n; j++) {
      const cost = ca === b[j - 1] ? 0 : 1;
      curr[j] = Math.min(prev[j] + 1, curr[j - 1] + 1, prev[j - 1] + cost);
    }
    [prev, curr] = [curr, prev];
  }
  return prev[n];
}

// ============================================================================
// 1. PRONUNCIATION EVALUATION & VIETNAMESE LEARNER TIPS
// ============================================================================

function isNearMatchWord(target: string, spoken: string, lang: LanguageCode): {
  status: WordPronunciationStatus;
  distance: number;
} {
  if (!target || !spoken) {
    return { status: "wrong", distance: Math.max(target.length, spoken.length) };
  }
  if (target === spoken) {
    return { status: "correct", distance: 0 };
  }

  // Handle contractions or hyphenless forms ("longterm" vs "long-term")
  const tFlat = target.replace(/['-]/g, "");
  const sFlat = spoken.replace(/['-]/g, "");
  if (tFlat === sFlat) {
    return { status: "correct", distance: 0 };
  }

  if (isCjkLanguage(lang)) {
    const dist = levenshteinDistance(target, spoken);
    if (target.length >= 2 && dist === 1) {
      return { status: "near", distance: dist };
    }
    return { status: "wrong", distance: dist };
  }

  const dist = levenshteinDistance(tFlat, sFlat);
  const maxLen = Math.max(tFlat.length, sFlat.length);

  // Common dropped ending sounds (-s, -es, -ed, -t, -d) or small phonetic slip
  if (tFlat.length >= 3 && dist === 1) {
    return { status: "near", distance: dist };
  }
  if (tFlat.length >= 5 && dist <= 2 && dist / maxLen <= 0.35) {
    return { status: "near", distance: dist };
  }
  if (
    tFlat.length >= 4 &&
    sFlat.length >= 3 &&
    ((tFlat.startsWith(sFlat) && tFlat.length - sFlat.length <= 2) ||
      (sFlat.startsWith(tFlat) && sFlat.length - tFlat.length <= 2))
  ) {
    return { status: "near", distance: dist };
  }

  return { status: "wrong", distance: dist };
}

export function evaluatePronunciationWords(
  targetSentence: string,
  spokenTranscript: string,
  lang: LanguageCode = "en-US"
): PronunciationEvaluationResult {
  const isCjk = isCjkLanguage(lang);

  // Split display target tokens cleanly (expand hyphenated English tokens? Keep display token but compare flexibly)
  const displayTargets: string[] = isCjk
    ? Array.from(normalizeTextForComparison(targetSentence, lang))
    : targetSentence
        .trim()
        .split(/\s+/)
        .filter(Boolean);

  if (displayTargets.length === 0) {
    return {
      score: 0,
      words: [],
      correctCount: 0,
      nearCount: 0,
      wrongCount: 0,
      missedWords: [],
      tips: [],
    };
  }

  if (!spokenTranscript || !spokenTranscript.trim()) {
    const emptyWords: PronunciationWordResult[] = displayTargets.map((w) => ({
      word: w,
      cleanTarget: cleanWordToken(w, lang),
      status: "wrong",
      distance: cleanWordToken(w, lang).length,
    }));
    return {
      score: 0,
      words: emptyWords,
      correctCount: 0,
      nearCount: 0,
      wrongCount: emptyWords.length,
      missedWords: displayTargets,
      tips: [],
    };
  }

  // Expand hyphenated words in target for alignment if spoken text split them ("long-term" vs "long term")
  const expandedTargets: Array<{ displayIdx: number; token: string }> = [];
  if (isCjk) {
    displayTargets.forEach((ch, idx) => {
      expandedTargets.push({ displayIdx: idx, token: ch });
    });
  } else {
    displayTargets.forEach((raw, idx) => {
      const normalized = normalizeTextForComparison(raw, lang);
      const parts = normalized.split(/\s+/).filter(Boolean);
      if (parts.length === 0) {
        expandedTargets.push({ displayIdx: idx, token: cleanWordToken(raw, lang) });
      } else {
        for (const p of parts) {
          expandedTargets.push({ displayIdx: idx, token: p });
        }
      }
    });
  }

  const spokenTokens = tokenizeText(spokenTranscript, lang);
  const m = expandedTargets.length;
  const n = spokenTokens.length;

  // Needleman-Wunsch style DP alignment so dropped or inserted words don't misalign the rest of the sentence
  const dp: number[][] = Array.from({ length: m + 1 }, () => Array<number>(n + 1).fill(0));
  const choice: number[][] = Array.from({ length: m + 1 }, () => Array<number>(n + 1).fill(0));
  // choice: 1 = match/sub (i-1, j-1), 2 = delete target (i-1, j), 3 = insert spoken (i, j-1)

  for (let i = 1; i <= m; i++) {
    dp[i][0] = i * 2;
    choice[i][0] = 2;
  }
  for (let j = 1; j <= n; j++) {
    dp[0][j] = j * 2;
    choice[0][j] = 3;
  }

  for (let i = 1; i <= m; i++) {
    const tTok = expandedTargets[i - 1].token;
    for (let j = 1; j <= n; j++) {
      const sTok = spokenTokens[j - 1];
      const evalPair = isNearMatchWord(tTok, sTok, lang);
      const subCost = evalPair.status === "correct" ? 0 : evalPair.status === "near" ? 1 : 3;

      const costSub = dp[i - 1][j - 1] + subCost;
      const costDel = dp[i - 1][j] + 2;
      const costIns = dp[i][j - 1] + 2;

      if (costSub <= costDel && costSub <= costIns) {
        dp[i][j] = costSub;
        choice[i][j] = 1;
      } else if (costDel <= costIns) {
        dp[i][j] = costDel;
        choice[i][j] = 2;
      } else {
        dp[i][j] = costIns;
        choice[i][j] = 3;
      }
    }
  }

  // Backtrack alignment
  const alignedForExpanded: Array<{ spoken?: string; status: WordPronunciationStatus; distance: number }> =
    Array.from({ length: m }, (_, i) => ({
      status: "wrong",
      distance: expandedTargets[i].token.length,
    }));

  let i = m;
  let j = n;
  while (i > 0 || j > 0) {
    if (i > 0 && j > 0 && choice[i][j] === 1) {
      const tTok = expandedTargets[i - 1].token;
      const sTok = spokenTokens[j - 1];
      const evalPair = isNearMatchWord(tTok, sTok, lang);
      alignedForExpanded[i - 1] = {
        spoken: sTok,
        status: evalPair.status,
        distance: evalPair.distance,
      };
      i--;
      j--;
    } else if (i > 0 && (j === 0 || choice[i][j] === 2)) {
      alignedForExpanded[i - 1] = {
        spoken: undefined,
        status: "wrong",
        distance: expandedTargets[i - 1].token.length,
      };
      i--;
    } else {
      j--;
    }
  }

  // Collapse expanded parts back into displayTargets
  const partsByDisplay = new Map<
    number,
    Array<{ spoken?: string; status: WordPronunciationStatus; distance: number }>
  >();
  expandedTargets.forEach((et, idx) => {
    const arr = partsByDisplay.get(et.displayIdx) ?? [];
    arr.push(alignedForExpanded[idx]);
    partsByDisplay.set(et.displayIdx, arr);
  });

  const words: PronunciationWordResult[] = displayTargets.map((rawWord, idx) => {
    const parts = partsByDisplay.get(idx) ?? [];
    const cleanTarget = cleanWordToken(rawWord, lang);
    const matchedParts = parts.map((p) => p.spoken).filter(Boolean) as string[];
    const totalDist = parts.reduce((acc, p) => acc + p.distance, 0);

    let status: WordPronunciationStatus = "wrong";
    if (parts.length > 0 && parts.every((p) => p.status === "correct")) {
      status = "correct";
    } else if (parts.some((p) => p.status === "correct" || p.status === "near")) {
      status = "near";
    } else {
      status = "wrong";
    }

    return {
      word: rawWord,
      cleanTarget,
      matchedSpoken: matchedParts.length > 0 ? matchedParts.join(" ") : undefined,
      status,
      distance: totalDist,
    };
  });

  const correctCount = words.filter((w) => w.status === "correct").length;
  const nearCount = words.filter((w) => w.status === "near").length;
  const wrongCount = words.filter((w) => w.status === "wrong").length;
  const score = Math.max(
    0,
    Math.min(100, Math.round(((correctCount + nearCount * 0.6) / words.length) * 100))
  );
  const missedWords = words
    .filter((w) => w.status === "wrong" || w.status === "near")
    .map((w) => w.word);

  const tips = detectVietnamesePronunciationTips(words, targetSentence, lang);

  return {
    score,
    words,
    correctCount,
    nearCount,
    wrongCount,
    missedWords,
    tips,
  };
}

// Vietnamese learner pronunciation trap sets
const NON_ED_SUFFIX_WORDS = new Set([
  "bed", "red", "fed", "led", "wed", "shed", "sled", "need", "seed", "feed",
  "speed", "breed", "bleed", "indeed", "hundred", "naked", "sacred", "wicked",
]);

const NON_S_SUFFIX_WORDS = new Set([
  "is", "as", "us", "his", "has", "was", "yes", "this", "thus", "plus",
  "bus", "gas", "focus", "bias", "basis", "status", "campus", "series",
  "analysis", "process", "progress", "success", "access", "business", "address",
  "less", "more", "press", "stress", "class", "glass", "across", "always",
]);

const SHORT_LONG_VOWEL_TRAPS = new Set([
  "ship", "sheep", "live", "leave", "sit", "seat", "bit", "beat", "fit", "feet",
  "fill", "feel", "slip", "sleep", "rich", "reach", "hit", "heat", "chip", "cheap",
  "full", "fool", "pull", "pool", "look", "luke", "good", "food", "work", "walk",
  "bad", "bed", "man", "men", "said", "sad", "hat", "heart", "still", "steal",
  "meet", "meat", "pick", "peak", "week", "weak",
]);

const NON_TH_WORDS = new Set(["thomas", "thailand", "lighthouse", "shorthand", "adulthood"]);

function matchesThSound(clean: string): boolean {
  return clean.includes("th") && !NON_TH_WORDS.has(clean);
}

function matchesSEsOrEdEnding(clean: string): boolean {
  if (clean.length < 4) return false;
  if (clean.endsWith("ed") && !NON_ED_SUFFIX_WORDS.has(clean)) {
    return true;
  }
  if (
    clean.endsWith("s") &&
    !clean.endsWith("ss") &&
    !clean.endsWith("us") &&
    !clean.endsWith("is") &&
    !NON_S_SUFFIX_WORDS.has(clean)
  ) {
    return true;
  }
  return false;
}

function matchesEndingSound(clean: string): boolean {
  if (clean.length < 3) return false;
  return /(?:ts|ks|st|ld|nd|nt|ct|pt|ft|sk|sp|lk|rk|mp|nk|ck|[tdkpfv]|(?:[aeiou]|r)[tdkpfv]e)$/.test(
    clean
  );
}

function matchesConfusablePairs(clean: string): boolean {
  if (clean.length < 3) return false;
  if (SHORT_LONG_VOWEL_TRAPS.has(clean)) return true;
  if (/(?:sh|ch|tr)/.test(clean)) return true;
  if (/(?:tion|sion|cial|tial|cious)/.test(clean)) return true;
  if (/(?:br|cr|dr|fr|gr|pr|bl|cl|fl|gl|pl|sl)/.test(clean)) return true;
  if (clean.includes("r") && clean.includes("l")) return true;
  return false;
}

export function detectVietnamesePronunciationTips(
  wordResults: PronunciationWordResult[],
  targetSentence?: string,
  lang: LanguageCode = "en-US"
): VietnamesePronunciationTip[] {
  const problemItems = wordResults.filter((w) => w.status === "wrong" || w.status === "near");

  if (isCjkLanguage(lang)) {
    if (problemItems.length === 0) return [];
    const sampleChars = Array.from(new Set(problemItems.map((p) => p.word))).slice(0, 6);
    if (lang === "ja-JP") {
      return [
        {
          id: "cjk-ja-pitch",
          category: "cjk_tone_pitch",
          title: "Cao độ (Pitch Accent), trường âm & âm ngắt tiếng Nhật",
          adviceVi:
            "Chú ý kéo dài đúng trường âm (ー / おう / えい), ngừng nhẹ 1 nhịp ở âm ngắt (っ) và giữ cao độ tự nhiên thay vì thêm thanh sắc/huyền tiếng Việt.",
          exampleWords: sampleChars,
        },
      ];
    }
    return [
      {
        id: "cjk-zh-tone",
        category: "cjk_tone_pitch",
        title: "Thanh điệu (4 thanh) & âm bật hơi tiếng Trung",
        adviceVi:
          "Phân biệt rõ 4 thanh điệu (ngang, lên, xuống-lên, dứt khoát xuống) và nhóm âm đầu lưỡi/uốn lưỡi (z/c/s vs zh/ch/sh, j/q/x).",
          exampleWords: sampleChars,
      },
    ];
  }

  // If everything was pronounced correctly and no problem words exist, return no error tips
  if (problemItems.length === 0 && wordResults.length > 0) {
    return [];
  }

  // Primary pool: wrong/near words. If wordResults is empty (direct call with targetSentence only), inspect targetSentence.
  const primaryWords =
    problemItems.length > 0
      ? problemItems.map((w) => ({
          display: w.word.replace(/^[^A-Za-z0-9']+|[^A-Za-z0-9']+$/g, "") || w.word,
          clean: w.cleanTarget || cleanWordToken(w.word, lang),
        }))
      : (targetSentence || "")
          .split(/\s+/)
          .filter(Boolean)
          .map((w) => ({
            display: w.replace(/^[^A-Za-z0-9']+|[^A-Za-z0-9']+$/g, "") || w,
            clean: cleanWordToken(w, lang),
          }));

  const collectTips = (
    pool: Array<{ display: string; clean: string }>
  ): VietnamesePronunciationTip[] => {
    const thWords = Array.from(new Set(pool.filter((w) => matchesThSound(w.clean)).map((w) => w.display)));
    const endingWords = Array.from(
      new Set(pool.filter((w) => matchesEndingSound(w.clean)).map((w) => w.display))
    );
    const sEdWords = Array.from(
      new Set(pool.filter((w) => matchesSEsOrEdEnding(w.clean)).map((w) => w.display))
    );
    const confusableWords = Array.from(
      new Set(pool.filter((w) => matchesConfusablePairs(w.clean)).map((w) => w.display))
    );

    const tips: VietnamesePronunciationTip[] = [];

    if (thWords.length > 0) {
      tips.push({
        id: "vn-tip-th",
        category: "th_sound",
        title: 'Âm /θ/ và /ð/ (tổ hợp "th")',
        adviceVi:
          'Đặt nhẹ đầu lưỡi giữa hai hàm răng rồi đẩy luồng hơi ra (/θ/ vô thanh như "think, with"; /ð/ hữu thanh rung dây thanh như "the, this, that, weather"). Tránh đọc cứng thành "t", "đ" hay "s".',
        exampleWords: thWords.slice(0, 6),
      });
    }

    if (endingWords.length > 0) {
      tips.push({
        id: "vn-tip-ending",
        category: "ending_sounds",
        title: "Âm cuối (Ending Sounds: -t, -d, -k, -p, -f, -v, -ts, -ks)",
        adviceVi:
          "Người Việt rất dễ nuốt âm cuối làm người nghe nhầm nghĩa từ. Hãy bật nhẹ âm gió/âm bật ở cuối mỗi từ (ví dụ: build, trust, look, work, accept, projects).",
        exampleWords: endingWords.slice(0, 6),
      });
    }

    if (sEdWords.length > 0) {
      tips.push({
        id: "vn-tip-s-ed",
        category: "s_es_ed_endings",
        title: "Đuôi -s / -es và -ed",
        adviceVi:
          "Giữ rõ đuôi ngữ pháp: -s/-es đọc là /s/ (sau p, t, k, f, θ), /ɪz/ (sau s, z, ʃ, tʃ, dʒ) hoặc /z/ (còn lại); -ed đọc là /ɪd/ (sau t, d), /t/ (sau âm vô thanh như walked, passed) hoặc /d/ (sau âm hữu thanh như encouraged).",
        exampleWords: sEdWords.slice(0, 6),
      });
    }

    if (confusableWords.length > 0) {
      tips.push({
        id: "vn-tip-confusable",
        category: "confusable_pairs",
        title: "Phụ âm kép & cặp âm dễ nhầm (r/l, s/sh, tr/ch, nguyên âm ngắn/dài)",
        adviceVi:
          'Uốn cong đầu lưỡi với âm "r" để phân biệt với "l"; chu môi đẩy hơi với "sh" (/ʃ/) khác "s"; phân biệt "tr/ch" và giữ đúng độ dài nguyên âm ngắn/dài (ví dụ: ship /ɪ/ vs sheep /iː/, live vs leave).',
        exampleWords: confusableWords.slice(0, 6),
      });
    }

    return tips;
  };

  let tips = collectTips(primaryWords);

  // If the missed word itself didn't trigger any specific trap, scan the full target sentence as helpful context
  if (tips.length === 0 && targetSentence) {
    const fallbackPool = targetSentence
      .split(/\s+/)
      .filter(Boolean)
      .map((w) => ({
        display: w.replace(/^[^A-Za-z0-9']+|[^A-Za-z0-9']+$/g, "") || w,
        clean: cleanWordToken(w, lang),
      }));
    tips = collectTips(fallbackPool);
  }

  return tips;
}

// ============================================================================
// 2. CLOZE EVALUATION & TEMPLATE PARSER
// ============================================================================

export function parseClozeTemplate(
  template: string,
  blanksCount: number
): ClozeTemplateSegment[] {
  if (!template) return [];

  // Format A: explicit indexed placeholders {{0}}, {{1}}...
  if (/\{\{\d+\}\}/.test(template)) {
    const segments: ClozeTemplateSegment[] = [];
    const regex = /\{\{(\d+)\}\}/g;
    let lastIndex = 0;
    let match: RegExpExecArray | null;
    while ((match = regex.exec(template)) !== null) {
      if (match.index > lastIndex) {
        segments.push({ type: "text", text: template.slice(lastIndex, match.index) });
      }
      const idx = Number.parseInt(match[1], 10);
      segments.push({ type: "blank", blankIndex: idx });
      lastIndex = regex.lastIndex;
    }
    if (lastIndex < template.length) {
      segments.push({ type: "text", text: template.slice(lastIndex) });
    }
    return segments;
  }

  // Format B: sequential ___ or ____ blanks
  if (/_{3,}/.test(template)) {
    const segments: ClozeTemplateSegment[] = [];
    const regex = /_{3,}/g;
    let lastIndex = 0;
    let blankCursor = 0;
    let match: RegExpExecArray | null;
    while ((match = regex.exec(template)) !== null) {
      if (match.index > lastIndex) {
        segments.push({ type: "text", text: template.slice(lastIndex, match.index) });
      }
      segments.push({ type: "blank", blankIndex: blankCursor });
      blankCursor++;
      lastIndex = regex.lastIndex;
    }
    if (lastIndex < template.length) {
      segments.push({ type: "text", text: template.slice(lastIndex) });
    }
    return segments;
  }

  // Fallback: render full text followed by blanks
  const fallbackSegments: ClozeTemplateSegment[] = [{ type: "text", text: template }];
  for (let i = 0; i < blanksCount; i++) {
    fallbackSegments.push({ type: "blank", blankIndex: i });
  }
  return fallbackSegments;
}

export function evaluateClozeAnswers(
  item: ClozePracticeItem,
  userInputs: string[]
): ClozeEvaluationResult {
  const totalBlanks = item.blanks.length;
  if (totalBlanks === 0) {
    return {
      score: 100,
      correctCount: 0,
      totalBlanks: 0,
      isAllCorrect: true,
      blanks: [],
    };
  }

  const results: ClozeBlankResult[] = item.blanks.map((spec, idx) => {
    const rawUser = (userInputs[idx] ?? "").trim();
    const normUser = normalizeTextForComparison(rawUser, item.lang);
    const isCorrect =
      normUser.length > 0 &&
      spec.acceptedAnswers.some(
        (ans) => normalizeTextForComparison(ans, item.lang) === normUser
      );

    return {
      index: idx,
      userAnswer: rawUser,
      acceptedAnswers: spec.acceptedAnswers,
      isCorrect,
      hintVi: spec.hintVi,
      explanationVi: spec.explanationVi,
    };
  });

  const correctCount = results.filter((r) => r.isCorrect).length;
  const score = Math.round((correctCount / totalBlanks) * 100);

  return {
    score,
    correctCount,
    totalBlanks,
    isAllCorrect: correctCount === totalBlanks,
    blanks: results,
  };
}

// ============================================================================
// 3. TRANSLATION (VI -> TARGET LANG) EVALUATION
// ============================================================================

const EN_STOP_WORDS = new Set([
  "a", "an", "the", "to", "of", "in", "on", "at", "for", "is", "am", "are",
]);

function computeTokenDiffAndScore(
  reference: string,
  userAnswer: string,
  lang: LanguageCode,
  hintKeywords: string[] = []
): {
  isExact: boolean;
  score: number;
  missingTokens: string[];
  extraTokens: string[];
  matchedKeywords: string[];
  missingKeywords: string[];
} {
  const normRef = normalizeTextForComparison(reference, lang);
  const normUser = normalizeTextForComparison(userAnswer, lang);

  if (!normUser) {
    return {
      isExact: false,
      score: 0,
      missingTokens: tokenizeText(reference, lang),
      extraTokens: [],
      matchedKeywords: [],
      missingKeywords: [...hintKeywords],
    };
  }

  if (normRef === normUser) {
    return {
      isExact: true,
      score: 100,
      missingTokens: [],
      extraTokens: [],
      matchedKeywords: [...hintKeywords],
      missingKeywords: [],
    };
  }

  const refTokens = tokenizeText(reference, lang);
  const userTokens = tokenizeText(userAnswer, lang);

  // Multiset overlap
  const userBag = new Map<string, number>();
  for (const t of userTokens) {
    userBag.set(t, (userBag.get(t) ?? 0) + 1);
  }

  const missingTokens: string[] = [];
  let matchedCount = 0;
  let contentTotal = 0;
  let contentMatched = 0;

  for (const rt of refTokens) {
    const isContent = isCjkLanguage(lang) || !EN_STOP_WORDS.has(rt);
    if (isContent) contentTotal++;

    const count = userBag.get(rt) ?? 0;
    if (count > 0) {
      userBag.set(rt, count - 1);
      matchedCount++;
      if (isContent) contentMatched++;
    } else {
      // Allow slight typo (Levenshtein 1 on words >= 4 chars) for partial credit
      let nearKey: string | null = null;
      if (!isCjkLanguage(lang) && rt.length >= 4) {
        for (const [ut, uCnt] of userBag.entries()) {
          if (uCnt > 0 && levenshteinDistance(rt, ut) === 1) {
            nearKey = ut;
            break;
          }
        }
      }
      if (nearKey) {
        userBag.set(nearKey, (userBag.get(nearKey) ?? 1) - 1);
        matchedCount += 0.7;
        if (isContent) contentMatched += 0.7;
        missingTokens.push(rt);
      } else {
        missingTokens.push(rt);
      }
    }
  }

  const extraTokens: string[] = [];
  for (const [ut, remain] of userBag.entries()) {
    for (let k = 0; k < remain; k++) {
      extraTokens.push(ut);
    }
  }

  const matchedKeywords: string[] = [];
  const missingKeywords: string[] = [];
  for (const kw of hintKeywords) {
    const normKw = normalizeTextForComparison(kw, lang);
    if (!normKw) continue;
    if (normUser.includes(normKw)) {
      matchedKeywords.push(kw);
    } else {
      missingKeywords.push(kw);
    }
  }

  const recall = refTokens.length > 0 ? matchedCount / refTokens.length : 0;
  const precision = userTokens.length > 0 ? matchedCount / userTokens.length : 0;
  const f1 = recall + precision > 0 ? (2 * recall * precision) / (recall + precision) : 0;
  const contentRecall = contentTotal > 0 ? contentMatched / contentTotal : recall;
  const keywordRatio =
    hintKeywords.length > 0 ? matchedKeywords.length / hintKeywords.length : contentRecall;

  // Weighted blend capped at 95 because it's not an exact match
  const rawScore = Math.round((f1 * 0.55 + contentRecall * 0.3 + keywordRatio * 0.15) * 100);
  const score = Math.max(0, Math.min(95, rawScore));

  return {
    isExact: false,
    score,
    missingTokens: Array.from(new Set(missingTokens)),
    extraTokens: Array.from(new Set(extraTokens)),
    matchedKeywords,
    missingKeywords,
  };
}

export function evaluateTranslationAnswer(
  item: TranslatePracticeItem,
  userAnswer: string
): TranslationEvaluationResult {
  const candidates = Array.from(
    new Set([item.sampleAnswer, ...(item.acceptedAnswers ?? [])].filter((s) => Boolean(s && s.trim())))
  );

  if (candidates.length === 0) {
    return {
      score: 0,
      isExactMatch: false,
      matchedAnswer: "",
      missingTokens: [],
      extraTokens: [],
      matchedKeywords: [],
      missingKeywords: [...(item.hintKeywords ?? [])],
      feedbackVi: "Chưa có đáp án mẫu để đối chiếu.",
    };
  }

  if (!userAnswer || !userAnswer.trim()) {
    return {
      score: 0,
      isExactMatch: false,
      matchedAnswer: candidates[0],
      missingTokens: tokenizeText(candidates[0], item.lang),
      extraTokens: [],
      matchedKeywords: [],
      missingKeywords: [...(item.hintKeywords ?? [])],
      feedbackVi: "Bạn chưa nhập bản dịch. Hãy thử viết câu tiếng Anh trước khi kiểm tra nhé.",
    };
  }

  let bestCandidate = candidates[0];
  let bestResult = computeTokenDiffAndScore(candidates[0], userAnswer, item.lang, item.hintKeywords);

  for (const cand of candidates) {
    const res = computeTokenDiffAndScore(cand, userAnswer, item.lang, item.hintKeywords);
    if (res.isExact) {
      bestCandidate = cand;
      bestResult = res;
      break;
    }
    if (res.score > bestResult.score) {
      bestCandidate = cand;
      bestResult = res;
    }
  }

  let feedbackVi = "";
  if (bestResult.isExact) {
    feedbackVi = "Chính xác hoàn toàn! Bản dịch của bạn khớp tự nhiên với đáp án chuẩn.";
  } else if (bestResult.score >= 80) {
    feedbackVi =
      bestResult.missingTokens.length > 0
        ? `Rất gần chuẩn (${bestResult.score}%). Bạn chỉ cần chỉnh/bổ sung thêm: ${bestResult.missingTokens.slice(0, 5).join(", ")}.`
        : `Ý nghĩa rất sát (${bestResult.score}%). Hãy đối chiếu trật tự từ với đáp án tham khảo bên dưới.`;
  } else if (bestResult.score >= 50) {
    feedbackVi = `Đã nắm được phần ý chính (${bestResult.score}%), nhưng còn thiếu/lệch một số từ quan trọng: ${bestResult.missingTokens.slice(0, 6).join(", ")}.`;
  } else {
    feedbackVi = `Bản dịch còn cách khá xa câu chuẩn (${bestResult.score}%). Hãy xem lại các từ khóa gợi ý và đáp án tham khảo rồi viết lại một lần nữa.`;
  }

  return {
    score: bestResult.score,
    isExactMatch: bestResult.isExact,
    matchedAnswer: bestCandidate,
    missingTokens: bestResult.missingTokens,
    extraTokens: bestResult.extraTokens,
    matchedKeywords: bestResult.matchedKeywords,
    missingKeywords: bestResult.missingKeywords,
    feedbackVi,
  };
}

// ============================================================================
// 4. GRAMMAR FIX EVALUATION
// ============================================================================

export function evaluateGrammarFixAnswer(
  item: GrammarFixPracticeItem,
  userAnswer: string
): GrammarFixEvaluationResult {
  const candidates = Array.from(
    new Set(
      [item.correctedSentence, ...(item.acceptedAnswers ?? [])].filter((s) =>
        Boolean(s && s.trim())
      )
    )
  );
  const primaryRef = candidates[0] || item.correctedSentence;

  if (!userAnswer || !userAnswer.trim()) {
    return {
      score: 0,
      isExactMatch: false,
      isUnchangedError: false,
      bestReference: primaryRef,
      missingTokens: tokenizeText(primaryRef, item.lang),
      wrongOrExtraTokens: [],
      feedbackVi: "Bạn chưa nhập câu đã sửa lỗi.",
      explanationVi: item.explanationVi,
    };
  }

  const normUser = normalizeTextForComparison(userAnswer, item.lang);
  const normIncorrect = normalizeTextForComparison(item.incorrectSentence, item.lang);
  const isUnchangedError = normUser === normIncorrect;

  let bestRef = primaryRef;
  let bestDiff = computeTokenDiffAndScore(primaryRef, userAnswer, item.lang);

  for (const cand of candidates) {
    const res = computeTokenDiffAndScore(cand, userAnswer, item.lang);
    if (res.isExact) {
      bestRef = cand;
      bestDiff = res;
      break;
    }
    if (res.score > bestDiff.score) {
      bestRef = cand;
      bestDiff = res;
    }
  }

  if (bestDiff.isExact) {
    return {
      score: 100,
      isExactMatch: true,
      isUnchangedError: false,
      bestReference: bestRef,
      missingTokens: [],
      wrongOrExtraTokens: [],
      feedbackVi: "Xuất sắc! Bạn đã phát hiện và sửa đúng hoàn toàn lỗi ngữ pháp.",
      explanationVi: item.explanationVi,
    };
  }

  if (isUnchangedError) {
    return {
      score: 20,
      isExactMatch: false,
      isUnchangedError: true,
      bestReference: bestRef,
      missingTokens: bestDiff.missingTokens,
      wrongOrExtraTokens: bestDiff.extraTokens,
      feedbackVi: `Bạn đang nộp lại nguyên câu sai gốc. Gợi ý loại lỗi: ${item.errorTypeHintVi}.`,
      explanationVi: item.explanationVi,
    };
  }

  // Penalize slightly if the specific error tokens from the incorrect sentence are still present
  const score = Math.min(85, bestDiff.score);
  const diffParts: string[] = [];
  if (bestDiff.missingTokens.length > 0) {
    diffParts.push(`còn thiếu/cần đổi sang: ${bestDiff.missingTokens.slice(0, 5).join(", ")}`);
  }
  if (bestDiff.extraTokens.length > 0) {
    diffParts.push(`từ thừa/chưa đúng: ${bestDiff.extraTokens.slice(0, 5).join(", ")}`);
  }

  return {
    score,
    isExactMatch: false,
    isUnchangedError: false,
    bestReference: bestRef,
    missingTokens: bestDiff.missingTokens,
    wrongOrExtraTokens: bestDiff.extraTokens,
    feedbackVi:
      diffParts.length > 0
        ? `Chưa sửa trọn vẹn (${score}%): ${diffParts.join("; ")}.`
        : `Câu đã gần đúng (${score}%), hãy đối chiếu lại trật tự từ với câu chuẩn.`,
    explanationVi: item.explanationVi,
  };
}

// ============================================================================
// 5. ROLEPLAY OFFLINE DETERMINISTIC EVALUATION (ZERO-FACADE)
// ============================================================================

export function evaluateRoleplayOffline(
  item: RoleplayPracticeItem,
  userResponse: string
): RoleplayEvaluationResult {
  const trimmed = (userResponse || "").trim();
  const sampleRef = item.sampleResponses[0] || "";
  const keywords = item.expectedKeywords ?? [];
  const minWords = Math.max(2, item.minWords || (isCjkLanguage(item.lang) ? 4 : 5));

  if (!trimmed) {
    return {
      score: 0,
      gradingMode: "keyword_offline",
      usedLiveEndpoint: false,
      gradingLabelVi: GRADING_LABELS.keyword_offline,
      feedbackVi:
        "Bạn chưa nhập hoặc nói câu trả lời. Hãy phản hồi lời thoại trên để chấm điểm theo từ khóa.",
      correctedVersion: sampleRef,
      strengths: [],
      improvements: [
        `Hãy trả lời ít nhất ${minWords} ${isCjkLanguage(item.lang) ? "ký tự" : "từ"} phù hợp với tình huống.`,
      ],
      matchedKeywords: [],
      missingKeywords: [...keywords],
    };
  }

  const normResponse = normalizeTextForComparison(trimmed, item.lang);
  const responseTokens = tokenizeText(trimmed, item.lang);
  const unitCount = responseTokens.length;

  const matchedKeywords: string[] = [];
  const missingKeywords: string[] = [];

  for (const kw of keywords) {
    const normKw = normalizeTextForComparison(kw, item.lang);
    if (!normKw) continue;
    const kwTokens = tokenizeText(kw, item.lang);
    const directHit = normResponse.includes(normKw);
    const tokenAllHit =
      kwTokens.length > 0 &&
      kwTokens.every((kt) =>
        responseTokens.some(
          (rt) => rt === kt || (!isCjkLanguage(item.lang) && kt.length >= 4 && levenshteinDistance(rt, kt) <= 1)
        )
      );

    if (directHit || tokenAllHit) {
      matchedKeywords.push(kw);
    } else {
      missingKeywords.push(kw);
    }
  }

  // Scoring breakdown (deterministic):
  // - Keywords coverage: up to 65 points
  // - Length / completeness vs minWords: up to 25 points
  // - Alignment with sample responses: up to 10 points
  const keywordRatio = keywords.length > 0 ? matchedKeywords.length / keywords.length : 0.7;
  const keywordPoints = keywordRatio * 65;

  const lengthRatio = Math.min(1, unitCount / minWords);
  const lengthPoints = lengthRatio * 25;

  let bestSampleOverlap = 0;
  for (const sample of item.sampleResponses ?? []) {
    const diff = computeTokenDiffAndScore(sample, trimmed, item.lang);
    bestSampleOverlap = Math.max(bestSampleOverlap, diff.score / 100);
  }
  const samplePoints = bestSampleOverlap * 10;

  let score = Math.round(keywordPoints + lengthPoints + samplePoints);
  if (unitCount < Math.max(2, Math.floor(minWords * 0.5))) {
    score = Math.min(score, 45);
  }
  score = Math.max(0, Math.min(100, score));

  const strengths: string[] = [];
  const improvements: string[] = [];

  if (matchedKeywords.length > 0) {
    strengths.push(
      `Đã sử dụng được ${matchedKeywords.length}/${keywords.length} từ khóa trọng tâm: ${matchedKeywords.join(", ")}.`
    );
  }
  if (unitCount >= minWords) {
    strengths.push(
      `Độ dài câu trả lời hợp lý (${unitCount} ${isCjkLanguage(item.lang) ? "ký tự" : "từ"}), đủ ý để duy trì hội thoại.`
    );
  } else {
    improvements.push(
      `Câu trả lời còn hơi ngắn (${unitCount}/${minWords} ${isCjkLanguage(item.lang) ? "ký tự" : "từ"}). Hãy mở rộng thêm 1 mệnh đề lịch sự hoặc câu hỏi nối tiếp.`
    );
  }
  if (missingKeywords.length > 0) {
    improvements.push(
      `Nên lồng ghép thêm các cụm từ khóa tự nhiên: ${missingKeywords.slice(0, 4).join(", ")}.`
    );
  }
  if (strengths.length === 0) {
    strengths.push("Bạn đã chủ động phản hồi tình huống hội thoại.");
  }

  const feedbackVi =
    score >= 80
      ? `Phản hồi rất tốt (${score}/100 · Chấm theo từ khóa offline)! Bạn đã bám sát tình huống và dùng ${matchedKeywords.length}/${keywords.length} từ khóa mục tiêu.`
      : score >= 60
      ? `Phản hồi khá ổn (${score}/100 · Chấm theo từ khóa offline), đã dùng ${matchedKeywords.length}/${keywords.length} từ khóa. Hãy bổ sung thêm các cụm từ gợi ý để tự nhiên hơn.`
      : `Câu trả lời cần mở rộng thêm (${score}/100 · Chấm theo từ khóa offline). Hãy tham khảo mẫu câu gợi ý và sử dụng các từ khóa trọng tâm của tình huống.`;

  return {
    score,
    gradingMode: "keyword_offline",
    usedLiveEndpoint: false,
    gradingLabelVi: GRADING_LABELS.keyword_offline,
    feedbackVi,
    correctedVersion: sampleRef,
    strengths,
    improvements,
    matchedKeywords,
    missingKeywords,
  };
}

// ============================================================================
// SCHEMA NORMALIZER (LEGACY SINGLE CONFIG + EXTENDED MULTI-ITEM CONFIG)
// ============================================================================

function safeParseJson(raw?: string): Record<string, any> | null {
  if (!raw || !raw.trim()) return null;
  try {
    const parsed = JSON.parse(raw);
    return parsed && typeof parsed === "object" ? parsed : null;
  } catch {
    return null;
  }
}

function asStringArray(val: unknown): string[] {
  if (!Array.isArray(val)) return [];
  return val.map((x) => (typeof x === "string" ? x.trim() : "")).filter(Boolean);
}

function normalizeWordMapNodes(val: unknown): WordMapNodeItem[] {
  if (!Array.isArray(val)) return [];
  return val
    .filter((x) => x && typeof x === "object" && typeof (x as any).text === "string")
    .map((x: any) => ({
      text: String(x.text).trim(),
      ipa: typeof x.ipa === "string" ? x.ipa : undefined,
      reading: typeof x.reading === "string" ? x.reading : undefined,
      meaningVi: typeof x.meaningVi === "string" ? x.meaningVi : "",
      example: typeof x.example === "string" ? x.example : undefined,
    }));
}

function inferDifficultyFromIndex(idx: number): DifficultyLevel {
  if (idx % 3 === 0) return "easy";
  if (idx % 3 === 1) return "medium";
  return "hard";
}

function normalizeSentenceEntry(
  entry: Record<string, any>,
  act: RawPracticeActivity,
  fallbackId: string,
  defaultLang: LanguageCode,
  defaultDifficulty: DifficultyLevel
): SentenceDrillItem | null {
  const sentence = typeof entry.sentence === "string" ? entry.sentence.trim() : "";
  if (!sentence) return null;

  const lang = normalizeLanguageCode(entry.lang, defaultLang);
  const difficulty = normalizeDifficulty(entry.difficulty, defaultDifficulty);
  const keyPhrases = asStringArray(entry.keyPhrases);
  const targetWords = asStringArray(entry.targetWords);
  const wordChips = asStringArray(entry.wordChips);
  const distractors = asStringArray(entry.distractors);
  const speakingOutline = asStringArray(entry.speakingOutline);

  return {
    id: typeof entry.id === "string" && entry.id.trim() ? entry.id.trim() : fallbackId,
    lang,
    difficulty,
    topic:
      typeof entry.topic === "string" && entry.topic.trim()
        ? entry.topic.trim()
        : act.title || "Bài luyện tập ngôn ngữ",
    sentence,
    ipa: typeof entry.ipa === "string" ? entry.ipa : "",
    reading: typeof entry.reading === "string" ? entry.reading : undefined,
    meaningVi:
      typeof entry.meaningVi === "string" && entry.meaningVi.trim()
        ? entry.meaningVi.trim()
        : "Nội dung bài tập chuẩn hóa từ hệ thống.",
    audioUrl: typeof entry.audioUrl === "string" ? entry.audioUrl : null,
    speakingPrompt:
      typeof entry.speakingPrompt === "string" && entry.speakingPrompt.trim()
        ? entry.speakingPrompt.trim()
        : `Luyện phát âm và diễn đạt tự nhiên câu mẫu: "${sentence}"`,
    speakingOutline:
      speakingOutline.length > 0
        ? speakingOutline
        : [
            "Nghe câu mẫu và chú ý trọng âm, nối âm",
            "Đọc rõ âm cuối và ngữ điệu lên xuống tự nhiên",
            "Áp dụng cụm từ khóa vào tình huống hội thoại thực tế",
          ],
    sampleAnswer:
      typeof entry.sampleAnswer === "string" && entry.sampleAnswer.trim()
        ? entry.sampleAnswer.trim()
        : sentence,
    keyPhrases:
      keyPhrases.length > 0
        ? keyPhrases
        : targetWords.length > 0
        ? targetWords.slice(0, 6)
        : sentence
            .split(/\s+/)
            .filter((w) => w.length >= 4)
            .slice(0, 4),
    targetWords: targetWords.length > 0 ? targetWords : undefined,
    wordChips: wordChips.length > 0 ? wordChips : undefined,
    distractors: distractors.length > 0 ? distractors : undefined,
    sourceActivityId: act.id,
    passingScore: act.passingScore ?? 70,
  };
}

export function normalizePracticeActivities(
  activities: RawPracticeActivity[],
  defaultLang: LanguageCode = "en-US"
): NormalizedLanguagePracticeBank {
  const bank: NormalizedLanguagePracticeBank = {
    shadowing: [],
    dictation: [],
    pronunciation: [],
    wordMaps: [],
    cloze: [],
    translate: [],
    grammarFix: [],
    roleplay: [],
  };

  const sorted = [...(activities || [])].sort(
    (a, b) => (a.orderIndex ?? 0) - (b.orderIndex ?? 0)
  );

  for (const act of sorted) {
    const cfg = safeParseJson(act.configJson);
    if (!cfg) continue;

    const type = (act.type || "").trim().toLowerCase();
    const actLang = normalizeLanguageCode(cfg.lang, defaultLang);
    const actDifficulty = normalizeDifficulty(cfg.difficulty, "medium");
    const rawItems: Array<Record<string, any>> = Array.isArray(cfg.items)
      ? cfg.items.filter((x: unknown) => Boolean(x && typeof x === "object"))
      : [cfg];

    if (type === "shadowing" || type === "dictation" || type === "pronunciation") {
      rawItems.forEach((entry, idx) => {
        const fallbackId = `${act.id || type}-${idx + 1}`;
        const defaultDiff = entry.difficulty
          ? normalizeDifficulty(entry.difficulty, actDifficulty)
          : rawItems.length > 1
          ? inferDifficultyFromIndex(idx)
          : actDifficulty;
        const normalized = normalizeSentenceEntry(entry, act, fallbackId, actLang, defaultDiff);
        if (!normalized) return;

        const targetBucket =
          type === "shadowing"
            ? bank.shadowing
            : type === "dictation"
            ? bank.dictation
            : bank.pronunciation;
        if (!targetBucket.some((x) => x.id === normalized.id)) {
          targetBucket.push(normalized);
        }
      });
    } else if (type === "word_map") {
      rawItems.forEach((entry, idx) => {
        const centralWord = typeof entry.centralWord === "string" ? entry.centralWord.trim() : "";
        if (!centralWord) return;
        const id =
          typeof entry.id === "string" && entry.id.trim()
            ? entry.id.trim()
            : `${act.id || "word_map"}-${idx + 1}`;
        if (bank.wordMaps.some((x) => x.id === id)) return;

        const contextExamples = Array.isArray(entry.contextExamples)
          ? entry.contextExamples
              .filter((ex: any) => ex && typeof ex.sentence === "string")
              .map((ex: any) => ({
                sentence: String(ex.sentence),
                meaningVi: typeof ex.meaningVi === "string" ? ex.meaningVi : "",
              }))
          : [];

        bank.wordMaps.push({
          id,
          lang: normalizeLanguageCode(entry.lang, actLang),
          difficulty: normalizeDifficulty(
            entry.difficulty,
            rawItems.length > 1 ? inferDifficultyFromIndex(idx) : actDifficulty
          ),
          title:
            typeof entry.title === "string" && entry.title.trim()
              ? entry.title.trim()
              : act.title || `Sơ đồ từ vựng: ${centralWord}`,
          centralWord,
          ipa: typeof entry.ipa === "string" ? entry.ipa : undefined,
          reading: typeof entry.reading === "string" ? entry.reading : undefined,
          meaningVi: typeof entry.meaningVi === "string" ? entry.meaningVi : "",
          collocations: normalizeWordMapNodes(entry.collocations),
          phrasalVerbs: normalizeWordMapNodes(entry.phrasalVerbs),
          synonyms: normalizeWordMapNodes(entry.synonyms),
          contextExamples,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "cloze") {
      rawItems.forEach((entry, idx) => {
        const template = typeof entry.template === "string" ? entry.template.trim() : "";
        const rawBlanks = Array.isArray(entry.blanks) ? entry.blanks : [];
        if (!template || rawBlanks.length === 0) return;

        const blanks: ClozeBlankSpec[] = rawBlanks
          .map((b: any, bIdx: number): ClozeBlankSpec | null => {
            if (typeof b === "string") {
              return { id: `b-${bIdx}`, acceptedAnswers: [b] };
            }
            if (b && typeof b === "object") {
              const accepted = asStringArray(b.acceptedAnswers);
              if (accepted.length === 0 && typeof b.answer === "string" && b.answer.trim()) {
                accepted.push(b.answer.trim());
              }
              if (accepted.length === 0) return null;
              return {
                id: typeof b.id === "string" ? b.id : `b-${bIdx}`,
                acceptedAnswers: accepted,
                hintVi: typeof b.hintVi === "string" ? b.hintVi : undefined,
                explanationVi: typeof b.explanationVi === "string" ? b.explanationVi : undefined,
              };
            }
            return null;
          })
          .filter((x: ClozeBlankSpec | null): x is ClozeBlankSpec => x !== null);

        if (blanks.length === 0) return;
        const id =
          typeof entry.id === "string" && entry.id.trim()
            ? entry.id.trim()
            : `${act.id || "cloze"}-${idx + 1}`;
        if (bank.cloze.some((x) => x.id === id)) return;

        bank.cloze.push({
          id,
          lang: normalizeLanguageCode(entry.lang, actLang),
          difficulty: normalizeDifficulty(
            entry.difficulty,
            rawItems.length > 1 ? inferDifficultyFromIndex(idx) : actDifficulty
          ),
          title:
            typeof entry.title === "string" && entry.title.trim()
              ? entry.title.trim()
              : act.title || "Điền chỗ trống (Cloze)",
          promptVi:
            typeof entry.promptVi === "string" && entry.promptVi.trim()
              ? entry.promptVi.trim()
              : "Tự nhớ và điền từ/cụm từ chính xác vào các ô trống bên dưới:",
          template,
          blanks,
          fullSentence: typeof entry.fullSentence === "string" ? entry.fullSentence : undefined,
          meaningVi: typeof entry.meaningVi === "string" ? entry.meaningVi : "",
          explanationVi: typeof entry.explanationVi === "string" ? entry.explanationVi : undefined,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "translate") {
      rawItems.forEach((entry, idx) => {
        const sourceVi = typeof entry.sourceVi === "string" ? entry.sourceVi.trim() : "";
        const acceptedAnswers = asStringArray(entry.acceptedAnswers);
        const sampleAnswer =
          typeof entry.sampleAnswer === "string" && entry.sampleAnswer.trim()
            ? entry.sampleAnswer.trim()
            : acceptedAnswers[0] || "";
        if (!sourceVi || !sampleAnswer) return;

        const id =
          typeof entry.id === "string" && entry.id.trim()
            ? entry.id.trim()
            : `${act.id || "translate"}-${idx + 1}`;
        if (bank.translate.some((x) => x.id === id)) return;

        bank.translate.push({
          id,
          lang: normalizeLanguageCode(entry.lang, actLang),
          difficulty: normalizeDifficulty(
            entry.difficulty,
            rawItems.length > 1 ? inferDifficultyFromIndex(idx) : actDifficulty
          ),
          title:
            typeof entry.title === "string" && entry.title.trim()
              ? entry.title.trim()
              : act.title || "Dịch Việt → Anh",
          sourceVi,
          hintKeywords: asStringArray(entry.hintKeywords),
          acceptedAnswers: acceptedAnswers.length > 0 ? acceptedAnswers : [sampleAnswer],
          sampleAnswer,
          grammarNoteVi: typeof entry.grammarNoteVi === "string" ? entry.grammarNoteVi : undefined,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "grammar_fix") {
      rawItems.forEach((entry, idx) => {
        const incorrectSentence =
          typeof entry.incorrectSentence === "string" ? entry.incorrectSentence.trim() : "";
        const correctedSentence =
          typeof entry.correctedSentence === "string" ? entry.correctedSentence.trim() : "";
        if (!incorrectSentence || !correctedSentence) return;

        const id =
          typeof entry.id === "string" && entry.id.trim()
            ? entry.id.trim()
            : `${act.id || "grammar_fix"}-${idx + 1}`;
        if (bank.grammarFix.some((x) => x.id === id)) return;

        const acceptedAnswers = asStringArray(entry.acceptedAnswers);
        bank.grammarFix.push({
          id,
          lang: normalizeLanguageCode(entry.lang, actLang),
          difficulty: normalizeDifficulty(
            entry.difficulty,
            rawItems.length > 1 ? inferDifficultyFromIndex(idx) : actDifficulty
          ),
          title:
            typeof entry.title === "string" && entry.title.trim()
              ? entry.title.trim()
              : act.title || "Sửa lỗi ngữ pháp",
          incorrectSentence,
          errorTypeHintVi:
            typeof entry.errorTypeHintVi === "string" && entry.errorTypeHintVi.trim()
              ? entry.errorTypeHintVi.trim()
              : "Kiểm tra thì động từ, giới từ hoặc chia số ít/số nhiều.",
          correctedSentence,
          acceptedAnswers: Array.from(new Set([correctedSentence, ...acceptedAnswers])),
          meaningVi: typeof entry.meaningVi === "string" ? entry.meaningVi : undefined,
          explanationVi:
            typeof entry.explanationVi === "string" && entry.explanationVi.trim()
              ? entry.explanationVi.trim()
              : `Câu chuẩn cần viết là: "${correctedSentence}".`,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (type === "roleplay") {
      rawItems.forEach((entry, idx) => {
        const aiPrompt = typeof entry.aiPrompt === "string" ? entry.aiPrompt.trim() : "";
        if (!aiPrompt) return;

        const id =
          typeof entry.id === "string" && entry.id.trim()
            ? entry.id.trim()
            : `${act.id || "roleplay"}-${idx + 1}`;
        if (bank.roleplay.some((x) => x.id === id)) return;

        const sampleResponses = asStringArray(entry.sampleResponses);
        if (sampleResponses.length === 0 && typeof entry.sampleAnswer === "string" && entry.sampleAnswer.trim()) {
          sampleResponses.push(entry.sampleAnswer.trim());
        }

        bank.roleplay.push({
          id,
          lang: normalizeLanguageCode(entry.lang, actLang),
          difficulty: normalizeDifficulty(
            entry.difficulty,
            rawItems.length > 1 ? inferDifficultyFromIndex(idx) : actDifficulty
          ),
          title:
            typeof entry.title === "string" && entry.title.trim()
              ? entry.title.trim()
              : act.title || "Nhập vai hội thoại (Roleplay)",
          scenarioVi:
            typeof entry.scenarioVi === "string" && entry.scenarioVi.trim()
              ? entry.scenarioVi.trim()
              : "Tình huống giao tiếp thực tế theo chủ đề bài học.",
          aiRole:
            typeof entry.aiRole === "string" && entry.aiRole.trim()
              ? entry.aiRole.trim()
              : "Đối tác giao tiếp",
          aiPrompt,
          aiPromptVi: typeof entry.aiPromptVi === "string" ? entry.aiPromptVi : "",
          userGoalVi:
            typeof entry.userGoalVi === "string" && entry.userGoalVi.trim()
              ? entry.userGoalVi.trim()
              : "Phản hồi lịch sự, đúng trọng tâm và dùng các cụm từ khóa gợi ý.",
          expectedKeywords: asStringArray(entry.expectedKeywords),
          sampleResponses,
          minWords:
            typeof entry.minWords === "number" && entry.minWords > 0
              ? entry.minWords
              : isCjkLanguage(actLang)
              ? 6
              : 6,
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    }
  }

  return bank;
}

export function filterItemsByDifficulty<T extends { difficulty: DifficultyLevel }>(
  items: T[],
  filter: DifficultyFilter
): T[] {
  if (filter === "all") return items;
  return items.filter((item) => item.difficulty === filter);
}

// ============================================================================
// DEFAULT ENGLISH WRITE-IN PACKS (WHEN COURSE HAS NO SEEDED WRITE-IN ACTIVITIES)
// ============================================================================

export function buildDefaultEnglishWriteInPacks(
  drills: SentenceDrillItem[],
  lessonTitle: string
): {
  cloze: ClozePracticeItem[];
  translate: TranslatePracticeItem[];
  grammarFix: GrammarFixPracticeItem[];
  roleplay: RoleplayPracticeItem[];
} {
  const cloze: ClozePracticeItem[] = [
    {
      id: "default-cloze-1",
      lang: "en-US",
      difficulty: "easy",
      title: "Cloze: Mở đầu giao tiếp lịch sự",
      promptVi: "Điền 2 từ còn thiếu để hoàn thành câu chào hỏi công sở chuẩn:",
      template: "It is a {{0}} to meet you, and I look {{1}} to working with your team.",
      blanks: [
        {
          acceptedAnswers: ["pleasure"],
          hintVi: "niềm vinh hạnh (bắt đầu bằng p)",
          explanationVi: 'Cụm cố định "It is a pleasure to meet you" = Rất hân hạnh được gặp bạn.',
        },
        {
          acceptedAnswers: ["forward"],
          hintVi: "mong đợi (look ___ to)",
          explanationVi: 'Cụm "look forward to + V-ing/Noun" = rất mong đợi điều gì.',
        },
      ],
      fullSentence: "It is a pleasure to meet you, and I look forward to working with your team.",
      meaningVi: "Rất hân hạnh được gặp bạn, và tôi rất mong đợi được làm việc cùng đội ngũ của bạn.",
      explanationVi: 'Chú ý sau "look forward to" là danh từ hoặc V-ing (working).',
    },
    {
      id: "default-cloze-2",
      lang: "en-US",
      difficulty: "medium",
      title: "Cloze: Cập nhật tiến độ công việc",
      promptVi: "Điền 2 từ vựng công sở phù hợp vào chỗ trống:",
      template: "Could you give me a quick {{0}} on the project {{1}} before our afternoon sync?",
      blanks: [
        {
          acceptedAnswers: ["update"],
          hintVi: "bản cập nhật (bắt đầu bằng u)",
          explanationVi: '"give a quick update on..." = cập nhật nhanh về...',
        },
        {
          acceptedAnswers: ["milestones", "milestone"],
          hintVi: "các cột mốc quan trọng (bắt đầu bằng m)",
          explanationVi: '"project milestones" = các cột mốc quan trọng của dự án.',
        },
      ],
      fullSentence: "Could you give me a quick update on the project milestones before our afternoon sync?",
      meaningVi: "Bạn có thể cập nhật nhanh các mốc quan trọng của dự án trước buổi họp chiều không?",
      explanationVi: "Chú ý bật rõ âm cuối -s trong từ milestones.",
    },
    {
      id: "default-cloze-3",
      lang: "en-US",
      difficulty: "hard",
      title: "Cloze: Hợp tác & xây dựng niềm tin",
      promptVi: "Điền 2 từ học thuật/công sở ở mức độ nâng cao:",
      template:
        "Transparent communication helps {{0}} long-term trust and encourages cross-disciplinary {{1}}.",
      blanks: [
        {
          acceptedAnswers: ["build", "to build", "establish", "foster"],
          hintVi: "xây dựng (bắt đầu bằng b)",
          explanationVi: '"build long-term trust" = xây dựng sự tin cậy lâu dài.',
        },
        {
          acceptedAnswers: ["collaboration", "cooperation", "teamwork"],
          hintVi: "sự hợp tác (bắt đầu bằng c)",
          explanationVi: '"cross-disciplinary collaboration" = sự hợp tác liên ngành.',
        },
      ],
      fullSentence:
        "Transparent communication helps build long-term trust and encourages cross-disciplinary collaboration.",
      meaningVi: "Giao tiếp minh bạch giúp xây dựng niềm tin lâu dài và khuyến khích hợp tác liên ngành.",
      explanationVi: 'Động từ "helps" đi với V nguyên mẫu (build) hoặc to-V.',
    },
  ];

  // If the first drill has a custom sentence from markdown/lesson, prepend a dynamic cloze from it if needed
  const translate: TranslatePracticeItem[] = [
    {
      id: "default-translate-1",
      lang: "en-US",
      difficulty: "easy",
      title: "Dịch Việt → Anh: Chào hỏi & giới thiệu",
      sourceVi: "Rất vui được gặp bạn. Tôi đang làm việc trong một dự án phần mềm mới.",
      hintKeywords: ["nice to meet you", "working on", "software project"],
      acceptedAnswers: [
        "Nice to meet you. I am working on a new software project.",
        "Nice to meet you. I'm working on a new software project.",
        "It is nice to meet you. I am working on a new software project.",
        "Pleased to meet you. I am working on a new software project.",
      ],
      sampleAnswer: "Nice to meet you. I'm working on a new software project.",
      grammarNoteVi: 'Dùng thì hiện tại tiếp diễn "am working on" cho dự án đang diễn ra.',
    },
    {
      id: "default-translate-2",
      lang: "en-US",
      difficulty: "medium",
      title: "Dịch Việt → Anh: Dời lịch họp lịch sự",
      sourceVi: "Tôi sẽ rất cảm kích nếu chúng ta có thể dời lịch họp sang chiều thứ Năm.",
      hintKeywords: ["appreciate", "reschedule", "Thursday afternoon"],
      acceptedAnswers: [
        "I would really appreciate it if we could reschedule our meeting to Thursday afternoon.",
        "I'd really appreciate it if we could reschedule our meeting to Thursday afternoon.",
        "I would appreciate it if we could reschedule the meeting to Thursday afternoon.",
        "I would appreciate it if we could move our meeting to Thursday afternoon.",
      ],
      sampleAnswer: "I would really appreciate it if we could reschedule our meeting to Thursday afternoon.",
      grammarNoteVi: 'Cấu trúc lịch sự: "I would appreciate it if we could + V". Đừng quên đại từ "it" sau appreciate!',
    },
    {
      id: "default-translate-3",
      lang: "en-US",
      difficulty: "hard",
      title: "Dịch Việt → Anh: Trình bày quan điểm",
      sourceVi: "Theo quan điểm của tôi, tập trung vào trải nghiệm người dùng sẽ tạo ra lợi thế cạnh tranh lớn.",
      hintKeywords: ["from my perspective", "user experience", "competitive edge"],
      acceptedAnswers: [
        "From my perspective, focusing on user experience will give us a strong competitive edge.",
        "From my perspective, focusing on user experience will create a strong competitive edge.",
        "In my opinion, focusing on user experience will create a strong competitive advantage.",
        "From my perspective, focusing on user experience will create a big competitive advantage.",
      ],
      sampleAnswer: "From my perspective, focusing on user experience will create a strong competitive edge.",
      grammarNoteVi: 'Danh động từ "Focusing on..." làm chủ ngữ trong câu, đi với động từ chia số ít/modal verb.',
    },
  ];

  const grammarFix: GrammarFixPracticeItem[] = [
    {
      id: "default-grammar-1",
      lang: "en-US",
      difficulty: "easy",
      title: "Sửa lỗi: Động từ 'agree' (lỗi hay gặp của người Việt)",
      incorrectSentence: "I am agree with your suggestion about the timeline.",
      errorTypeHintVi: 'Động từ thường "agree" không đi kèm "am/is/are".',
      correctedSentence: "I agree with your suggestion about the timeline.",
      acceptedAnswers: [
        "I agree with your suggestion about the timeline.",
        "I totally agree with your suggestion about the timeline.",
      ],
      meaningVi: "Tôi đồng ý với đề xuất của bạn về tiến độ thời gian.",
      explanationVi:
        'Trong tiếng Anh, "agree" là động từ thường, KHÔNG phải tính từ, nên nói "I agree with..." thay vì "I am agree".',
    },
    {
      id: "default-grammar-2",
      lang: "en-US",
      difficulty: "medium",
      title: "Sửa lỗi: Cấu trúc 'look forward to + V-ing'",
      incorrectSentence: "I look forward to meet you at the conference tomorrow.",
      errorTypeHintVi: '"to" trong "look forward to" là giới từ, không phải to-infinitive.',
      correctedSentence: "I look forward to meeting you at the conference tomorrow.",
      acceptedAnswers: [
        "I look forward to meeting you at the conference tomorrow.",
        "I am looking forward to meeting you at the conference tomorrow.",
      ],
      meaningVi: "Tôi rất mong đợi được gặp bạn tại hội nghị vào ngày mai.",
      explanationVi:
        'Sau cụm "look forward to", động từ bắt buộc phải chia ở dạng V-ing ("meeting"), không dùng nguyên mẫu ("meet").',
    },
    {
      id: "default-grammar-3",
      lang: "en-US",
      difficulty: "hard",
      title: "Sửa lỗi: Danh từ không đếm được & hòa hợp chủ ngữ",
      incorrectSentence: "He gave me many useful advices that helps improve my presentation.",
      errorTypeHintVi: '"advice" là danh từ không đếm được và mệnh đề quan hệ cần hòa hợp số ít/nhiều.',
      correctedSentence: "He gave me a lot of useful advice that helps improve my presentation.",
      acceptedAnswers: [
        "He gave me a lot of useful advice that helps improve my presentation.",
        "He gave me much useful advice that helps improve my presentation.",
        "He gave me some useful advice that helps improve my presentation.",
        "He gave me useful advice that helps improve my presentation.",
      ],
      meaningVi: "Anh ấy đã cho tôi rất nhiều lời khuyên hữu ích giúp cải thiện bài thuyết trình.",
      explanationVi:
        '"advice" là danh từ không đếm được nên không thêm "-s" và không dùng với "many"; dùng "a lot of / some / pieces of advice".',
    },
  ];

  const primaryTopic = drills[0]?.topic || lessonTitle || "Giao tiếp công sở";
  const roleplay: RoleplayPracticeItem[] = [
    {
      id: "default-roleplay-1",
      lang: "en-US",
      difficulty: "easy",
      title: `Nhập vai: Làm quen trong sự kiện (${primaryTopic})`,
      scenarioVi: "Bạn gặp một đồng nghiệp quốc tế lần đầu tại giờ giải lao hội thảo.",
      aiRole: "Alex (Kỹ sư từ Melbourne)",
      aiPrompt: "Hi there! Is this seat taken? By the way, I'm Alex. What brings you to this workshop today?",
      aiPromptVi: "Chào bạn! Ghế này có ai ngồi chưa? Nhân tiện, tôi là Alex. Điều gì đưa bạn đến buổi workshop hôm nay vậy?",
      userGoalVi: "Mời Alex ngồi, giới thiệu tên & công việc của bạn, và bày tỏ niềm vui khi được gặp.",
      expectedKeywords: ["go ahead", "pleasure", "meet", "work", "workshop"],
      sampleResponses: [
        "No, please go ahead! Nice to meet you, Alex. I'm Minh, and I work in software engineering. It's a pleasure to join this workshop and learn new practices.",
      ],
      minWords: 8,
    },
    {
      id: "default-roleplay-2",
      lang: "en-US",
      difficulty: "medium",
      title: "Nhập vai: Cập nhật tiến độ & đề xuất phương án",
      scenarioVi: "Quản lý dự án hỏi bạn về tiến độ hiện tại và rủi ro tiềm ẩn trước buổi họp khách hàng.",
      aiRole: "Sarah (Quản lý dự án)",
      aiPrompt: "Could you give me a quick update on our project milestones? Are we still on track for Friday?",
      aiPromptVi: "Bạn cập nhật nhanh các mốc dự án giúp mình nhé? Chúng ta vẫn kịp tiến độ thứ Sáu chứ?",
      userGoalVi: "Xác nhận tiến độ, nêu 1 điểm cần chú ý và đề xuất bước hành động tiếp theo.",
      expectedKeywords: ["update", "track", "milestone", "team", "friday"],
      sampleResponses: [
        "Sure! Here is a quick update: our team completed the core milestones and we are on track for Friday, though we should run one more review tomorrow.",
      ],
      minWords: 10,
    },
    {
      id: "default-roleplay-3",
      lang: "en-US",
      difficulty: "hard",
      title: "Nhập vai: Phản biện lịch sự trong cuộc họp",
      scenarioVi: "Đối tác đề xuất rút ngắn thời gian kiểm thử để ra mắt sớm hơn 1 tuần.",
      aiRole: "David (Giám đốc sản phẩm)",
      aiPrompt: "I think we can skip the second round of testing and launch next Monday to beat our competitors. What do you think?",
      aiPromptVi: "Tôi nghĩ ta có thể bỏ qua vòng kiểm thử thứ hai và ra mắt thứ Hai tới để vượt đối thủ. Bạn nghĩ sao?",
      userGoalVi: "Thể hiện sự tôn trọng góc nhìn của David nhưng phản biện ngoại giao về rủi ro chất lượng và đề xuất giải pháp cân bằng.",
      expectedKeywords: ["point", "perspective", "risk", "testing", "solution"],
      sampleResponses: [
        "I see your point about launching early, David. From my perspective, skipping testing carries a high risk for user trust. As a balanced solution, we could automate critical checks and release by Wednesday.",
      ],
      minWords: 12,
    },
  ];

  return { cloze, translate, grammarFix, roleplay };
}
