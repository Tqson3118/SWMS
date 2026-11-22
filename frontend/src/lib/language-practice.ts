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
  contextExamples: Array<{ sentence: string; reading?: string; meaningVi: string }>;
  sourceActivityId?: string;
  passingScore?: number;
}

export interface ClozeBlankSpec {
  id?: string;
  acceptedAnswers: string[];
  allowKanaEquivalence?: boolean;
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
  reading?: string;
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
  reading?: string;
  allowKanaEquivalence?: boolean;
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
  reading?: string;
  allowKanaEquivalence?: boolean;
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
  aiPromptReading?: string;
  aiPromptVi: string;
  userGoalVi: string;
  expectedKeywords: string[];
  sampleResponses: string[];
  sampleReading?: string;
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
  | "cjk_tone_pitch"
  | "ja_long_vowel"
  | "ja_sokuon"
  | "ja_consonants"
  | "zh_aspirated"
  | "zh_retroflex";

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
  matchedViaTonelessPinyin?: boolean;
  toneNoticeVi?: string;
  hintVi?: string;
  explanationVi?: string;
}

export interface ClozeEvaluationResult {
  score: number;
  correctCount: number;
  totalBlanks: number;
  isAllCorrect: boolean;
  blanks: ClozeBlankResult[];
  toneNoticeVi?: string;
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
  matchedViaTonelessPinyin?: boolean;
  toneNoticeVi?: string;
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

export function detectCourseLanguageFromText(
  courseTitle: string,
  lessonTitle = "",
  markdown = ""
): LanguageCode {
  const stripVi = (s: string) =>
    (s || "")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/đ/g, "d")
      .replace(/Đ/g, "D")
      .toLowerCase();

  const headerNorm = stripVi(`${courseTitle} ${lessonTitle}`);
  if (
    /\b(tieng nhat|japanese|jlpt|n5|n4|n3|hiragana|katakana|kanji|nihongo)\b/.test(headerNorm) ||
    /[\u3040-\u30ff]/.test(`${courseTitle} ${lessonTitle}`)
  ) {
    return "ja-JP";
  }
  if (
    /\b(tieng trung|chinese|hsk|hsk1|hsk2|hsk3|pinyin|hanzi|mandarin|zhongwen)\b/.test(headerNorm)
  ) {
    return "zh-CN";
  }

  const bodySample = markdown.slice(0, 1800);
  if (/[\u3040-\u30ff]/.test(bodySample)) {
    return "ja-JP";
  }
  const bodyNorm = stripVi(bodySample);
  if (/\b(tieng nhat|hiragana|katakana|jlpt|n5)\b/.test(bodyNorm)) {
    return "ja-JP";
  }
  if (/\b(tieng trung|pinyin|hsk1|hsk|thanh dieu|chu han)\b/.test(bodyNorm)) {
    return "zh-CN";
  }
  return "en-US";
}

export function detectLanguageFromActivities(
  activities: RawPracticeActivity[],
  courseTitle = "",
  lessonTitle = "",
  markdown = ""
): LanguageCode {
  for (const act of activities || []) {
    const cfg = safeParseJson(act.configJson);
    if (!cfg) continue;
    if (typeof cfg.lang === "string" && cfg.lang.trim()) {
      const parsed = normalizeLanguageCode(cfg.lang, "en-US");
      if (isCjkLanguage(parsed)) return parsed;
    }
    if (Array.isArray(cfg.items)) {
      for (const it of cfg.items) {
        if (it && typeof it.lang === "string" && it.lang.trim()) {
          const parsed = normalizeLanguageCode(it.lang, "en-US");
          if (isCjkLanguage(parsed)) return parsed;
        }
      }
    }
  }
  return detectCourseLanguageFromText(courseTitle, lessonTitle, markdown);
}

export function hasCjkCharacters(input: string): boolean {
  return /[\u3040-\u30ff\u3400-\u9fff]/.test(input || "");
}

export function hasJapaneseKana(input: string): boolean {
  return /[\u3040-\u30ff]/.test(input || "");
}

export function hasKatakana(input: string): boolean {
  return /[\u30a1-\u30f6]/.test(input || "");
}

export function katakanaToHiragana(input: string): string {
  if (!input) return "";
  return input.replace(/[\u30a1-\u30f6]/g, (ch) =>
    String.fromCharCode(ch.charCodeAt(0) - 0x60)
  );
}

export function hiraganaToKatakana(input: string): string {
  if (!input) return "";
  return input.replace(/[\u3041-\u3096]/g, (ch) =>
    String.fromCharCode(ch.charCodeAt(0) + 0x60)
  );
}

const PINYIN_TONE_MAP: Record<string, [string, number]> = {
  ā: ["a", 1], á: ["a", 2], ǎ: ["a", 3], à: ["a", 4],
  ē: ["e", 1], é: ["e", 2], ě: ["e", 3], è: ["e", 4],
  ī: ["i", 1], í: ["i", 2], ǐ: ["i", 3], ì: ["i", 4],
  ō: ["o", 1], ó: ["o", 2], ǒ: ["o", 3], ò: ["o", 4],
  ū: ["u", 1], ú: ["u", 2], ǔ: ["u", 3], ù: ["u", 4],
  ǖ: ["ü", 1], ǘ: ["ü", 2], ǚ: ["ü", 3], ǜ: ["ü", 4],
};

const PINYIN_VOWEL_SET = new Set(["a", "o", "e", "i", "u", "ü"]);
const PINYIN_INITIALS = [
  "zh", "ch", "sh",
  "b", "p", "m", "f", "d", "t", "n", "l",
  "g", "k", "h", "j", "q", "x", "r", "z", "c", "s", "y", "w",
];
const PINYIN_FINALS = [
  "iang", "iong", "uang", "ueng",
  "iao", "iou", "uai", "uei", "ang", "eng", "ing", "ong",
  "ai", "ei", "ao", "ou", "an", "en", "in", "un", "ün",
  "ia", "ie", "iu", "ua", "uo", "ui", "üe", "ue", "er",
  "a", "o", "e", "i", "u", "ü",
];

export function isPinyinText(input: string): boolean {
  if (!input || !input.trim()) return false;
  if (hasCjkCharacters(input)) return false;
  return /[a-zāáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜü]/i.test(input);
}

export function hasPinyinToneMarksOrNumbers(input: string): boolean {
  if (!input) return false;
  return /[āáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜ]|[a-zü][1-4](?![0-9])/i.test(input);
}

/**
 * Normalizes Mandarin Pinyin across tone marks ("zhōng wén" / "zhōngwén"),
 * tone numbers ("zhong1 wen2" / "zhong1wen2"), and toneless forms ("zhong wen").
 */
export function normalizePinyin(
  input: string,
  mode: "tone_numbers" | "toneless" = "tone_numbers"
): string {
  if (!input) return "";
  const cleaned = input
    .normalize("NFKC")
    .toLowerCase()
    .replace(/u:/g, "ü")
    .replace(/([ln])v/g, "$1ü")
    .replace(/[’‘`´]/g, "'")
    .replace(/[。、，．！？；：「」『』（）()【】\[\]《》〈〉,.!?;:"“”~～\-—–·•]/g, " ")
    .trim();

  if (!cleaned) return "";

  const chunks = cleaned.split(/[\s']+/).filter(Boolean);
  const syllables: string[] = [];

  for (const chunk of chunks) {
    // Separate plain chars and tone marks
    let plain = "";
    const charTones: number[] = [];
    for (const ch of chunk) {
      const mapped = PINYIN_TONE_MAP[ch];
      if (mapped) {
        plain += mapped[0];
        charTones.push(mapped[1]);
      } else {
        plain += ch;
        charTones.push(0);
      }
    }

    let pos = 0;
    while (pos < plain.length) {
      // Non-letter characters (e.g. standalone numbers) pass through cleanly
      if (!/[a-zü]/.test(plain[pos])) {
        if (/[1-5]/.test(plain[pos])) {
          pos++;
          continue;
        }
        let endNum = pos + 1;
        while (endNum < plain.length && !/[a-zü]/.test(plain[endNum])) endNum++;
        syllables.push(plain.slice(pos, endNum));
        pos = endNum;
        continue;
      }

      const startPos = pos;
      let initial = "";
      for (const init of PINYIN_INITIALS) {
        if (plain.startsWith(init, pos)) {
          // Ensure initial is followed by a vowel (or another valid letter)
          const afterInit = plain[pos + init.length];
          if (afterInit && PINYIN_VOWEL_SET.has(afterInit)) {
            initial = init;
            break;
          }
        }
      }

      const afterInitPos = pos + initial.length;
      let finalStr = "";
      for (const fin of PINYIN_FINALS) {
        if (plain.startsWith(fin, afterInitPos)) {
          // Avoid stealing 'g' or 'n' when next char is a vowel starting the next syllable (e.g. "jinan" -> "ji"+"nan")
          const nextCh = plain[afterInitPos + fin.length];
          if (fin.endsWith("ng") && nextCh && PINYIN_VOWEL_SET.has(nextCh)) {
            continue;
          }
          if (fin.endsWith("n") && fin.length > 1 && nextCh && PINYIN_VOWEL_SET.has(nextCh)) {
            continue;
          }
          finalStr = fin;
          break;
        }
      }

      if (!finalStr) {
        // Fallback: consume until next digit or end of chunk
        let endPos = pos + 1;
        while (endPos < plain.length && /[a-zü]/.test(plain[endPos])) endPos++;
        const rawSyl = plain.slice(startPos, endPos);
        let tone = 0;
        for (let k = startPos; k < endPos; k++) {
          if (charTones[k] > 0) tone = charTones[k];
        }
        if (endPos < plain.length && /[1-5]/.test(plain[endPos])) {
          const digit = Number(plain[endPos]);
          if (digit >= 1 && digit <= 4) tone = digit;
          endPos++;
        }
        syllables.push(
          mode === "tone_numbers" && tone >= 1 && tone <= 4 ? `${rawSyl}${tone}` : rawSyl
        );
        pos = endPos;
        continue;
      }

      let endPos = afterInitPos + finalStr.length;
      let tone = 0;
      for (let k = startPos; k < endPos; k++) {
        if (charTones[k] > 0) tone = charTones[k];
      }
      if (endPos < plain.length && /[1-5]/.test(plain[endPos])) {
        const digit = Number(plain[endPos]);
        if (digit >= 1 && digit <= 4) tone = digit;
        else if (digit === 5) tone = 0;
        endPos++;
      }

      const baseSyl = `${initial}${finalStr}`;
      syllables.push(
        mode === "tone_numbers" && tone >= 1 && tone <= 4 ? `${baseSyl}${tone}` : baseSyl
      );
      pos = endPos;
    }
  }

  return syllables.join(" ").trim();
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
    // Strip CJK & ASCII punctuation and whitespace (preserve Japanese prolonged sound mark ー U+30FC)
    return s
      .replace(/[\s。、，．！？；：「」『』（）()【】\[\]《》〈〉,.!?;:'"~～〜\-—–·•・]/g, "")
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
    if (!hasCjkCharacters(trimmed)) {
      if (lang === "zh-CN" && isPinyinText(trimmed)) {
        const py = normalizePinyin(trimmed, "toneless");
        return py ? py.split(/\s+/).filter(Boolean) : [];
      }
      if (/\s/.test(trimmed)) {
        return trimmed
          .split(/\s+/)
          .map((t) => cleanWordToken(t, "en-US"))
          .filter(Boolean);
      }
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

const JA_NEAR_KANA_PAIRS = new Set([
  "つ:す", "す:つ", "つ:ず", "ず:つ", "す:ず", "ず:す",
  "し:ち", "ち:し", "し:じ", "じ:し", "ち:じ", "じ:ち",
  "じ:ぢ", "ぢ:じ", "ず:づ", "づ:ず",
  "は:わ", "わ:は", "お:を", "を:お",
  "ー:う", "う:ー", "ー:い", "い:ー", "ー:あ", "あ:ー", "ー:え", "え:ー", "ー:お", "お:ー",
  "ら:だ", "だ:ら", "り:ぢ", "る:づ", "れ:で", "で:れ", "ろ:ど", "ど:ろ",
]);

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

  if (lang === "ja-JP") {
    const tHira = katakanaToHiragana(target);
    const sHira = katakanaToHiragana(spoken);
    if (tHira === sHira) {
      return { status: "correct", distance: 0 };
    }
    if (tHira.length === 1 && sHira.length === 1 && JA_NEAR_KANA_PAIRS.has(`${tHira}:${sHira}`)) {
      return { status: "near", distance: 1 };
    }
    const dist = levenshteinDistance(tHira, sHira);
    if (tHira.length >= 2 && dist === 1) {
      return { status: "near", distance: dist };
    }
    return { status: "wrong", distance: dist };
  }

  if (lang === "zh-CN") {
    if (isPinyinText(target) && isPinyinText(spoken)) {
      const tToned = normalizePinyin(target, "tone_numbers");
      const sToned = normalizePinyin(spoken, "tone_numbers");
      if (tToned === sToned) return { status: "correct", distance: 0 };
      const tBase = normalizePinyin(target, "toneless");
      const sBase = normalizePinyin(spoken, "toneless");
      if (tBase === sBase) return { status: "near", distance: 1 };
      const pDist = levenshteinDistance(tBase, sBase);
      if (tBase.length >= 3 && pDist === 1) return { status: "near", distance: 1 };
      return { status: "wrong", distance: pDist };
    }
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

function extractHiraganaReading(reading?: string): string {
  if (!reading) return "";
  const kanaOnly = Array.from(reading.normalize("NFKC"))
    .filter((ch) => /[\u3040-\u30ffー]/.test(ch))
    .join("");
  return kanaOnly;
}

function alignPronunciationCore(
  displayTargets: string[],
  spokenTokens: string[],
  lang: LanguageCode,
  targetSentence: string,
  targetReading?: string
): PronunciationEvaluationResult {
  const isCjk = isCjkLanguage(lang);

  const expandedTargets: Array<{ displayIdx: number; token: string }> = [];
  if (isCjk && hasCjkCharacters(displayTargets.join(""))) {
    displayTargets.forEach((ch, idx) => {
      expandedTargets.push({ displayIdx: idx, token: ch });
    });
  } else if (lang === "zh-CN" && isPinyinText(displayTargets.join(" "))) {
    displayTargets.forEach((syl, idx) => {
      expandedTargets.push({ displayIdx: idx, token: syl });
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

  const m = expandedTargets.length;
  const n = spokenTokens.length;

  // Needleman-Wunsch style DP alignment so dropped or inserted words/chars don't misalign the rest of the sentence
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
    Array.from({ length: m }, (_, idx) => ({
      status: "wrong",
      distance: expandedTargets[idx].token.length,
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

  const tips = detectVietnamesePronunciationTips(words, targetSentence, lang, targetReading);

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

export function evaluatePronunciationWords(
  targetSentence: string,
  spokenTranscript: string,
  lang: LanguageCode = "en-US",
  targetReading?: string
): PronunciationEvaluationResult {
  const isCjk = isCjkLanguage(lang);

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

  // Chinese manual Pinyin fallback when spokenTranscript is Pinyin and targetReading has Pinyin
  if (lang === "zh-CN" && isPinyinText(spokenTranscript) && targetReading && isPinyinText(targetReading)) {
    const pyTargets = normalizePinyin(targetReading, "tone_numbers").split(/\s+/).filter(Boolean);
    const pySpoken = normalizePinyin(spokenTranscript, "tone_numbers").split(/\s+/).filter(Boolean);
    if (pyTargets.length > 0 && pySpoken.length > 0) {
      return alignPronunciationCore(pyTargets, pySpoken, lang, targetSentence, targetReading);
    }
  }

  const spokenTokens = tokenizeText(spokenTranscript, lang);
  const directEval = alignPronunciationCore(displayTargets, spokenTokens, lang, targetSentence, targetReading);

  // Japanese Kanji vs Kana fallback: Web Speech API may output pure Hiragana for a Kanji sentence
  if (lang === "ja-JP" && targetReading && directEval.score < 100) {
    const hiraReading = extractHiraganaReading(targetReading);
    if (hiraReading.length >= 2) {
      const hiraTargets = Array.from(normalizeTextForComparison(hiraReading, "ja-JP"));
      const readingEval = alignPronunciationCore(hiraTargets, spokenTokens, "ja-JP", targetSentence, targetReading);
      if (readingEval.score > directEval.score) {
        return readingEval;
      }
    }
  }

  return directEval;
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

const ZH_ASPIRATED_HANZI = new Set(
  Array.from("朋友苹果便宜票旁边胖怕排片他她它太天听同学图铁条痛看开块咖啡课客口快可请去七钱前起气秋菜茶吃车出穿床长常城超草次从错")
);
const ZH_RETROFLEX_HANZI = new Set(
  Array.from("这中张知站找住桌重准着真茶吃车出穿床长常城超十是师书水谁少睡时山商上说人日热认肉让")
);

export function detectVietnamesePronunciationTips(
  wordResults: PronunciationWordResult[],
  targetSentence?: string,
  lang: LanguageCode = "en-US",
  reading?: string
): VietnamesePronunciationTip[] {
  const problemItems = wordResults.filter((w) => w.status === "wrong" || w.status === "near");

  if (isCjkLanguage(lang)) {
    if (problemItems.length === 0) return [];
    const sampleChars = Array.from(new Set(problemItems.map((p) => p.word))).slice(0, 6);
    const problemJoined = problemItems.map((p) => p.word).join("");
    const readingLower = (reading || "").toLowerCase();

    if (lang === "ja-JP") {
      const tips: VietnamesePronunciationTip[] = [];
      const hiraProblem = katakanaToHiragana(problemJoined);
      const hiraSentence = katakanaToHiragana(targetSentence || "");

      // Check if any missed character is directly part of a long vowel (ー / おう / えい / おお / ああ)
      const missedIndices = wordResults
        .map((w, idx) => (w.status === "wrong" || w.status === "near" ? idx : -1))
        .filter((idx) => idx >= 0);
      const fullCleanHira = katakanaToHiragana(
        wordResults.map((w) => w.word).join("")
      );
      const hasLongVowelProblem =
        hiraProblem.includes("ー") ||
        missedIndices.some((idx) => {
          const pairPrev = idx > 0 ? fullCleanHira.slice(idx - 1, idx + 1) : "";
          const pairNext = idx + 1 < fullCleanHira.length ? fullCleanHira.slice(idx, idx + 2) : "";
          return /(?:おう|こう|そう|とう|のう|ほう|もう|よう|ろう|ごう|ぞう|どう|ぼう|ぽう|えい|けい|せい|てい|ねい|へい|めい|れい|げい|ぜい|でい|べい|ぺい|おお|ああ|うう)/.test(
            pairPrev
          ) ||
            /(?:おう|こう|そう|とう|のう|ほう|もう|よう|ろう|ごう|ぞう|どう|ぼう|ぽう|えい|けい|せい|てい|ねい|へい|めい|れい|げい|ぜい|でい|べい|ぺい|おお|ああ|うう)/.test(
              pairNext
            );
        });

      const hasSokuonProblem = hiraProblem.includes("っ") || (readingLower.includes("tt") && hiraSentence.includes("っ"));
      const hasConsonantProblem = /[つすずしちじらりるれろ]/.test(hiraProblem);

      if (hasLongVowelProblem) {
        tips.push({
          id: "cjk-ja-long-vowel",
          category: "ja_long_vowel",
          title: "Trường âm tiếng Nhật (ー / おう / えい)",
          adviceVi:
            "Kéo dài nguyên âm đủ 2 phách (mora) khi gặp dấu trường âm (ー) hoặc tổ hợp おう / えい (ví dụ: コーヒー, ありがとう, せんせい). Rút ngắn trường âm sẽ làm đổi nghĩa từ (ビール bia ≠ ビル tòa nhà).",
          exampleWords: sampleChars,
        });
      }

      if (hasSokuonProblem) {
        tips.push({
          id: "cjk-ja-sokuon",
          category: "ja_sokuon",
          title: "Âm ngắt (Sokuon っ / ッ)",
          adviceVi:
            "Gặp chữ っ / ッ nhỏ, cần khựng lại đúng 1 nhịp (nén hơi trước khi bật phụ âm sau) như ちょっと (chotto), きって (kitte), ゆっくり (yukkuri). Không đọc lướt liền thành âm đơn.",
          exampleWords: sampleChars,
        });
      }

      if (hasConsonantProblem) {
        tips.push({
          id: "cjk-ja-consonants",
          category: "ja_consonants",
          title: "Phân biệt âm つ/す, し/ち và hàng r (r/d)",
          adviceVi:
            "Người Việt dễ nhầm つ (đặt đầu lưỡi sát chân răng trên rồi bật hơi 'ts') với す ('s'), し (mềm 'sh') với ち ('ch'), và đọc hàng ら/り/る/れ/ろ thành 'd' hoặc rung mạnh. Hãy chạm thật nhẹ đầu lưỡi lên lợi trên như âm trung gian giữa l và r.",
          exampleWords: sampleChars,
        });
      }

      if (tips.length === 0) {
        tips.push({
          id: "cjk-ja-pitch",
          category: "cjk_tone_pitch",
          title: "Cao độ (Pitch Accent), trường âm & âm ngắt tiếng Nhật",
          adviceVi:
            "Chú ý kéo dài đúng trường âm (ー / おう / えい), ngừng nhẹ 1 nhịp ở âm ngắt (っ), phân biệt つ/す · し/ち · r/d và giữ cao độ tự nhiên thay vì thêm thanh sắc/huyền tiếng Việt.",
          exampleWords: sampleChars,
        });
      }
      return tips;
    }

    // zh-CN: Chinese 4 tones + aspirated (p/t/k/q/c/ch vs b/d/g/j/z/zh) + retroflex (zh/ch/sh/r)
    const tips: VietnamesePronunciationTip[] = [
      {
        id: "cjk-zh-tone",
        category: "cjk_tone_pitch",
        title: "4 Thanh điệu tiếng Trung (đặc biệt thanh 1 cao ngang & thanh 3 xuống-lên)",
        adviceVi:
          "Giữ thanh 1 (ā) thật cao và phẳng như hát một nốt kéo dài (không tụt giọng như thanh ngang tiếng Việt); thanh 2 (á) lên dứt khoát; thanh 3 (ǎ) hạ thấp rồi vồng lên (hoặc đọc nửa thanh 3 trầm khi đứng trước âm tiết khác); thanh 4 (à) rơi mạnh và gọn.",
        exampleWords: sampleChars,
      },
    ];

    const problemCharsArr = Array.from(problemJoined);
    const hasAspirated =
      problemCharsArr.some((ch) => ZH_ASPIRATED_HANZI.has(ch)) ||
      (readingLower.length > 0 &&
        /(?:^|\s)(?:p|t|k|q|c(?!h)|ch)[aoeiuüāáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜ]/.test(readingLower) &&
        (isPinyinText(problemJoined) || problemItems.length >= 2));

    const hasRetroflex =
      problemCharsArr.some((ch) => ZH_RETROFLEX_HANZI.has(ch)) ||
      (readingLower.length > 0 &&
        /(?:^|\s)(?:zh|ch|sh|r)[aoeiuüāáǎàēéěèīíǐìōóǒòūúǔùǖǘǚǜ]/.test(readingLower) &&
        (isPinyinText(problemJoined) || problemItems.length >= 2));

    if (hasAspirated) {
      tips.push({
        id: "cjk-zh-aspirated",
        category: "zh_aspirated",
        title: "Âm bật hơi (p/t/k/q/c/ch) vs không bật hơi (b/d/g/j/z/zh)",
        adviceVi:
          "Đặt một tờ giấy trước miệng: nhóm bật hơi p, t, k, q, c, ch phải đẩy luồng hơi mạnh làm rung giấy, còn nhóm b, d, g, j, z, zh không bật hơi và không rung dây thanh kiểu 'b/đ' tiếng Việt.",
        exampleWords: problemCharsArr.filter((ch) => ZH_ASPIRATED_HANZI.has(ch)).slice(0, 6).length > 0
          ? problemCharsArr.filter((ch) => ZH_ASPIRATED_HANZI.has(ch)).slice(0, 6)
          : sampleChars,
      });
    }

    if (hasRetroflex) {
      tips.push({
        id: "cjk-zh-retroflex",
        category: "zh_retroflex",
        title: "Nhóm âm uốn lưỡi (zh / ch / sh / r)",
        adviceVi:
          "Uốn cong đầu lưỡi chạm nhẹ vòm miệng trên khi phát âm zh, ch, sh, r (ví dụ: 中国 zhōngguó, 吃饭 chīfàn, 老师 lǎoshī, 认识 rènshi), phân biệt rõ với nhóm đầu lưỡi thẳng z, c, s.",
        exampleWords: problemCharsArr.filter((ch) => ZH_RETROFLEX_HANZI.has(ch)).slice(0, 6).length > 0
          ? problemCharsArr.filter((ch) => ZH_RETROFLEX_HANZI.has(ch)).slice(0, 6)
          : sampleChars,
      });
    }

    return tips;
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

type CjkAnswerMatchKind =
  | "exact"
  | "kana_equiv"
  | "pinyin_toned"
  | "pinyin_toneless_explicit"
  | "pinyin_toneless_fallback"
  | "pinyin_wrong_tone"
  | "none";

function matchSingleAnswerCandidate(
  userRaw: string,
  candidateRaw: string,
  lang: LanguageCode,
  allowKanaEquivalence = false
): CjkAnswerMatchKind {
  const normUser = normalizeTextForComparison(userRaw, lang);
  const normCand = normalizeTextForComparison(candidateRaw, lang);
  if (!normUser || !normCand) return "none";

  if (normUser === normCand) {
    // Distinguish explicit toneless pinyin candidate in zh-CN so we can show a helpful tone reminder
    if (
      lang === "zh-CN" &&
      isPinyinText(userRaw) &&
      isPinyinText(candidateRaw) &&
      !hasPinyinToneMarksOrNumbers(userRaw) &&
      !hasPinyinToneMarksOrNumbers(candidateRaw)
    ) {
      return "pinyin_toneless_explicit";
    }
    return "exact";
  }

  if (lang === "ja-JP" && allowKanaEquivalence) {
    if (katakanaToHiragana(normUser) === katakanaToHiragana(normCand)) {
      return "kana_equiv";
    }
  }

  if (lang === "zh-CN" && isPinyinText(userRaw) && isPinyinText(candidateRaw)) {
    const userToned = normalizePinyin(userRaw, "tone_numbers").replace(/\s+/g, "");
    const candToned = normalizePinyin(candidateRaw, "tone_numbers").replace(/\s+/g, "");
    const userToneless = normalizePinyin(userRaw, "toneless").replace(/\s+/g, "");
    const candToneless = normalizePinyin(candidateRaw, "toneless").replace(/\s+/g, "");

    const candHasTones = hasPinyinToneMarksOrNumbers(candidateRaw);
    const userHasTones = hasPinyinToneMarksOrNumbers(userRaw);

    if (candHasTones) {
      if (userHasTones && userToned === candToned) {
        return "pinyin_toned";
      }
      if (!userHasTones && userToneless === candToneless) {
        return "pinyin_toneless_fallback";
      }
      if (userHasTones && userToneless === candToneless && userToned !== candToned) {
        return "pinyin_wrong_tone";
      }
    } else if (!userHasTones && userToneless === candToneless) {
      return "pinyin_toneless_explicit";
    }
  }

  return "none";
}

function findTonedPinyinReference(candidates: string[], fallbackReading?: string): string {
  const tonedFromCandidates = candidates.find(
    (c) => isPinyinText(c) && hasPinyinToneMarksOrNumbers(c)
  );
  if (tonedFromCandidates) return tonedFromCandidates;
  if (fallbackReading && isPinyinText(fallbackReading) && hasPinyinToneMarksOrNumbers(fallbackReading)) {
    return fallbackReading;
  }
  return "";
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

  const toneNotices: string[] = [];

  const results: ClozeBlankResult[] = item.blanks.map((spec, idx) => {
    const rawUser = (userInputs[idx] ?? "").trim();
    const normUser = normalizeTextForComparison(rawUser, item.lang);

    let isCorrect = false;
    let matchedViaTonelessPinyin = false;
    let toneNoticeVi: string | undefined;

    if (normUser.length > 0) {
      const kinds = spec.acceptedAnswers.map((ans) =>
        matchSingleAnswerCandidate(rawUser, ans, item.lang, Boolean(spec.allowKanaEquivalence))
      );

      if (kinds.includes("exact") || kinds.includes("kana_equiv") || kinds.includes("pinyin_toned")) {
        isCorrect = true;
      } else if (kinds.includes("pinyin_toneless_explicit")) {
        isCorrect = true;
        matchedViaTonelessPinyin = true;
        const tonedRef = findTonedPinyinReference(spec.acceptedAnswers, item.reading);
        toneNoticeVi = tonedRef
          ? `Ô #${idx + 1}: Đã chấp nhận Pinyin không dấu theo quy định bài tập — hãy ghi nhớ thanh điệu chuẩn: "${tonedRef}".`
          : `Ô #${idx + 1}: Đã chấp nhận Pinyin không dấu — hãy chú ý thanh điệu chuẩn khi phát âm.`;
        toneNotices.push(toneNoticeVi);
      } else if (kinds.includes("pinyin_toneless_fallback")) {
        isCorrect = false;
        matchedViaTonelessPinyin = true;
        const tonedRef = findTonedPinyinReference(spec.acceptedAnswers, item.reading);
        toneNoticeVi = tonedRef
          ? `Ô #${idx + 1}: Bạn gõ đúng âm tiết Pinyin nhưng thiếu dấu thanh điệu. Hãy nhập đúng thanh điệu (ví dụ "${tonedRef}" hoặc dùng số thanh 1–4).`
          : `Ô #${idx + 1}: Đúng âm tiết Pinyin nhưng thiếu dấu thanh điệu (dùng dấu thanh hoặc số 1–4).`;
        toneNotices.push(toneNoticeVi);
      } else if (kinds.includes("pinyin_wrong_tone")) {
        isCorrect = false;
        const tonedRef = findTonedPinyinReference(spec.acceptedAnswers, item.reading);
        toneNoticeVi = tonedRef
          ? `Ô #${idx + 1}: Đúng âm tiết Pinyin nhưng sai thanh điệu. Thanh điệu chuẩn: "${tonedRef}".`
          : `Ô #${idx + 1}: Đúng âm tiết Pinyin nhưng sai thanh điệu.`;
        toneNotices.push(toneNoticeVi);
      }
    }

    return {
      index: idx,
      userAnswer: rawUser,
      acceptedAnswers: spec.acceptedAnswers,
      isCorrect,
      matchedViaTonelessPinyin: matchedViaTonelessPinyin || undefined,
      toneNoticeVi,
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
    toneNoticeVi: toneNotices.length > 0 ? toneNotices.join(" ") : undefined,
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

  const targetLangLabel =
    item.lang === "ja-JP" ? "tiếng Nhật" : item.lang === "zh-CN" ? "tiếng Trung" : "tiếng Anh";

  if (!userAnswer || !userAnswer.trim()) {
    return {
      score: 0,
      isExactMatch: false,
      matchedAnswer: candidates[0],
      missingTokens: tokenizeText(candidates[0], item.lang),
      extraTokens: [],
      matchedKeywords: [],
      missingKeywords: [...(item.hintKeywords ?? [])],
      feedbackVi: `Bạn chưa nhập bản dịch. Hãy thử viết câu ${targetLangLabel} trước khi kiểm tra nhé.`,
    };
  }

  // First check CJK / Pinyin / Kana exact & tone-aware match rules across all accepted candidates
  let tonelessExplicitCandidate: string | null = null;
  let tonelessFallbackCandidate: string | null = null;
  let wrongToneCandidate: string | null = null;

  for (const cand of candidates) {
    const matchKind = matchSingleAnswerCandidate(
      userAnswer,
      cand,
      item.lang,
      Boolean(item.allowKanaEquivalence)
    );
    if (matchKind === "exact" || matchKind === "kana_equiv" || matchKind === "pinyin_toned") {
      const toneNote =
        matchKind === "pinyin_toned"
          ? `Khớp Pinyin đúng thanh điệu! Chữ Hán chuẩn: "${item.sampleAnswer}".`
          : undefined;
      return {
        score: 100,
        isExactMatch: true,
        matchedAnswer: cand,
        missingTokens: [],
        extraTokens: [],
        matchedKeywords: [...(item.hintKeywords ?? [])],
        missingKeywords: [],
        feedbackVi:
          matchKind === "pinyin_toned"
            ? "Chính xác hoàn toàn! Pinyin của bạn khớp chuẩn cả âm tiết lẫn thanh điệu."
            : "Chính xác hoàn toàn! Bản dịch của bạn khớp tự nhiên với đáp án chuẩn.",
        toneNoticeVi: toneNote,
      };
    }
    if (matchKind === "pinyin_toneless_explicit" && !tonelessExplicitCandidate) {
      tonelessExplicitCandidate = cand;
    } else if (matchKind === "pinyin_toneless_fallback" && !tonelessFallbackCandidate) {
      tonelessFallbackCandidate = cand;
    } else if (matchKind === "pinyin_wrong_tone" && !wrongToneCandidate) {
      wrongToneCandidate = cand;
    }
  }

  const tonedRef = findTonedPinyinReference(candidates, item.reading);

  if (tonelessExplicitCandidate) {
    const toneNoticeVi = tonedRef
      ? `Lưu ý thanh điệu: bài tập chấp nhận Pinyin không dấu, nhưng hãy ghi nhớ thanh điệu chuẩn "${tonedRef}" (chữ Hán: "${item.sampleAnswer}").`
      : `Lưu ý thanh điệu: bạn đã gõ Pinyin không dấu — hãy ôn lại thanh điệu chuẩn của câu "${item.sampleAnswer}".`;
    return {
      score: 100,
      isExactMatch: true,
      matchedAnswer: tonelessExplicitCandidate,
      missingTokens: [],
      extraTokens: [],
      matchedKeywords: [...(item.hintKeywords ?? [])],
      missingKeywords: [],
      feedbackVi: "Đạt yêu cầu (khớp Pinyin không dấu theo quy định bài học)! Xem ghi chú thanh điệu chuẩn bên dưới.",
      matchedViaTonelessPinyin: true,
      toneNoticeVi,
    };
  }

  if (tonelessFallbackCandidate) {
    const toneNoticeVi = tonedRef
      ? `Bạn đã gõ đúng âm tiết Pinyin nhưng thiếu dấu thanh điệu. Thanh điệu chuẩn: "${tonedRef}" (có thể gõ dấu thanh hoặc số 1–4 như "zhong1 wen2").`
      : `Bạn đã gõ đúng âm tiết Pinyin nhưng thiếu dấu thanh điệu (dùng dấu thanh hoặc số 1–4).`;
    return {
      score: 85,
      isExactMatch: false,
      matchedAnswer: tonelessFallbackCandidate,
      missingTokens: [],
      extraTokens: [],
      matchedKeywords: [...(item.hintKeywords ?? [])],
      missingKeywords: [],
      feedbackVi: "Rất sát (85%)! Đúng toàn bộ âm tiết Pinyin nhưng còn thiếu dấu thanh điệu.",
      matchedViaTonelessPinyin: true,
      toneNoticeVi,
    };
  }

  if (wrongToneCandidate) {
    const toneNoticeVi = tonedRef
      ? `Bạn đã gõ đúng âm tiết Pinyin nhưng sai thanh điệu! Thanh điệu chuẩn cần nhớ: "${tonedRef}".`
      : "Bạn đã gõ đúng âm tiết Pinyin nhưng sai thanh điệu.";
    return {
      score: 75,
      isExactMatch: false,
      matchedAnswer: wrongToneCandidate,
      missingTokens: [],
      extraTokens: [],
      matchedKeywords: [...(item.hintKeywords ?? [])],
      missingKeywords: [],
      feedbackVi: "Đúng âm tiết nhưng sai thanh điệu (75%). Hãy đối chiếu thanh điệu chuẩn bên dưới.",
      toneNoticeVi,
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

  // Exact match check via CJK/Pinyin/Kana-aware matcher first
  for (const cand of candidates) {
    const matchKind = matchSingleAnswerCandidate(
      userAnswer,
      cand,
      item.lang,
      Boolean(item.allowKanaEquivalence)
    );
    if (matchKind === "exact" || matchKind === "kana_equiv" || matchKind === "pinyin_toned") {
      return {
        score: 100,
        isExactMatch: true,
        isUnchangedError: false,
        bestReference: cand,
        missingTokens: [],
        wrongOrExtraTokens: [],
        feedbackVi: "Xuất sắc! Bạn đã phát hiện và sửa đúng hoàn toàn lỗi ngữ pháp.",
        explanationVi: item.explanationVi,
      };
    }
  }

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
  const hiraResponse = item.lang === "ja-JP" ? katakanaToHiragana(normResponse) : normResponse;
  const pinyinTonelessCompact =
    item.lang === "zh-CN" ? normalizePinyin(trimmed, "toneless").replace(/\s+/g, "") : "";
  const responseTokens = tokenizeText(trimmed, item.lang);
  const unitCount = responseTokens.length;

  const matchedKeywords: string[] = [];
  const missingKeywords: string[] = [];

  for (const kw of keywords) {
    const alternatives = kw
      .split("|")
      .map((part) => part.trim())
      .filter(Boolean);
    if (alternatives.length === 0) continue;

    const hit = alternatives.some((alt) => {
      const normKw = normalizeTextForComparison(alt, item.lang);
      if (!normKw) return false;
      if (normResponse.includes(normKw)) return true;

      if (item.lang === "ja-JP") {
        const hiraKw = katakanaToHiragana(normKw);
        if (hiraKw && hiraResponse.includes(hiraKw)) return true;
      }

      if (item.lang === "zh-CN" && isPinyinText(alt)) {
        const altToneless = normalizePinyin(alt, "toneless").replace(/\s+/g, "");
        if (altToneless && pinyinTonelessCompact.includes(altToneless)) return true;
      }

      const kwTokens = tokenizeText(alt, item.lang);
      return (
        kwTokens.length > 0 &&
        kwTokens.every((kt) =>
          responseTokens.some(
            (rt) =>
              rt === kt ||
              (!isCjkLanguage(item.lang) && kt.length >= 4 && levenshteinDistance(rt, kt) <= 1)
          )
        )
      );
    });

    if (hit) {
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
                reading: typeof ex.reading === "string" ? ex.reading : undefined,
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
                allowKanaEquivalence: Boolean(b.allowKanaEquivalence),
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
          reading: typeof entry.reading === "string" ? entry.reading : undefined,
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
          reading: typeof entry.reading === "string" ? entry.reading : undefined,
          allowKanaEquivalence: Boolean(entry.allowKanaEquivalence),
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
          reading: typeof entry.reading === "string" ? entry.reading : undefined,
          allowKanaEquivalence: Boolean(entry.allowKanaEquivalence),
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
          aiPromptReading: typeof entry.aiPromptReading === "string" ? entry.aiPromptReading : undefined,
          aiPromptVi: typeof entry.aiPromptVi === "string" ? entry.aiPromptVi : "",
          userGoalVi:
            typeof entry.userGoalVi === "string" && entry.userGoalVi.trim()
              ? entry.userGoalVi.trim()
              : "Phản hồi lịch sự, đúng trọng tâm và dùng các cụm từ khóa gợi ý.",
          expectedKeywords: asStringArray(entry.expectedKeywords),
          sampleResponses,
          sampleReading: typeof entry.sampleReading === "string" ? entry.sampleReading : undefined,
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

export interface DictationDiffItem {
  targetWord?: string;
  userWord?: string;
  status: "correct" | "typo" | "missing" | "extra";
}

export interface DictationEvaluationResult {
  accuracy: number;
  diffItems: DictationDiffItem[];
}

/**
 * Pure Dictation evaluator supporting both English (space-delimited words)
 * and CJK (ja-JP / zh-CN wordChips segments or character-level tokens without stripping CJK).
 */
export function evaluateDictationAnswer(params: {
  targetSentence: string;
  subMode: "type" | "chips";
  typedText: string;
  placedChips: string[];
  lang: LanguageCode;
  wordChips?: string[];
}): DictationEvaluationResult {
  const { targetSentence, subMode, typedText, placedChips, lang, wordChips } = params;
  const isCjk = isCjkLanguage(lang);

  let targetUnits: string[];
  let userUnits: string[];

  if (subMode === "chips") {
    targetUnits =
      wordChips && wordChips.length > 0
        ? wordChips.map((w) => w.trim()).filter(Boolean)
        : isCjk
        ? Array.from(normalizeTextForComparison(targetSentence, lang))
        : targetSentence
            .replace(/[.,/#!$%^&*;:{}=\-_`~()?!]/g, "")
            .split(/\s+/)
            .filter(Boolean);
    userUnits = placedChips.map((c) => c.trim()).filter(Boolean);
  } else if (isCjk) {
    targetUnits = Array.from(normalizeTextForComparison(targetSentence, lang));
    userUnits = Array.from(normalizeTextForComparison(typedText, lang));
  } else {
    targetUnits = targetSentence
      .replace(/[.,/#!$%^&*;:{}=\-_`~()?!]/g, "")
      .split(/\s+/)
      .filter(Boolean);
    userUnits = typedText
      .replace(/[.,/#!$%^&*;:{}=\-_`~()?!]/g, "")
      .split(/\s+/)
      .filter(Boolean);
  }

  if (targetUnits.length === 0) {
    return { accuracy: 0, diffItems: [] };
  }

  const normUnit = (u: string) =>
    isCjk ? normalizeTextForComparison(u, lang) : cleanWordToken(u, lang);

  // Exact full-sentence match in CJK chips or typing immediately scores 100%
  if (
    isCjk &&
    userUnits.length > 0 &&
    normalizeTextForComparison(userUnits.join(""), lang) ===
      normalizeTextForComparison(targetSentence, lang)
  ) {
    return {
      accuracy: 100,
      diffItems: targetUnits.map((tw) => ({
        targetWord: tw,
        userWord: tw,
        status: "correct",
      })),
    };
  }

  const maxLen = Math.max(targetUnits.length, userUnits.length);
  const diffItems: DictationDiffItem[] = [];
  let hits = 0;

  for (let i = 0; i < maxLen; i++) {
    const tw = targetUnits[i];
    const uw = userUnits[i];
    if (tw && uw) {
      if (normUnit(tw) === normUnit(uw)) {
        hits++;
        diffItems.push({ targetWord: tw, userWord: uw, status: "correct" });
      } else {
        diffItems.push({ targetWord: tw, userWord: uw, status: "typo" });
      }
    } else if (tw && !uw) {
      diffItems.push({ targetWord: tw, status: "missing" });
    } else if (!tw && uw) {
      diffItems.push({ userWord: uw, status: "extra" });
    }
  }

  const accuracy = userUnits.length === 0 ? 0 : Math.round((hits / maxLen) * 100);
  return { accuracy, diffItems };
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
