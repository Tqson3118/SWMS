"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  Activity,
  AlertTriangle,
  Award,
  BookOpen,
  Brain,
  CandlestickChart,
  CheckCircle2,
  Code2,
  Eye,
  Headphones,
  Mic,
  MicOff,
  NotebookPen,
  Play,
  RotateCcw,
  Sparkles,
  Square,
  TrendingDown,
  TrendingUp,
  Volume2,
  XCircle,
} from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";

export interface LessonPracticeLabProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

type PracticeMode = "finance" | "english" | "code" | "recall";

interface Candle {
  label: string;
  o: number;
  h: number;
  l: number;
  c: number;
  v: number;
}

interface FinanceScenario {
  id: string;
  title: string;
  ticker: string;
  timeframe: string;
  context: string;
  supportZone: [number, number];
  resistanceZone: [number, number];
  defaultEntry: number;
  defaultSl: number;
  defaultTp: number;
  bestAction: "long" | "short" | "wait";
  explanation: string;
  initialCandles: Candle[];
  futureCandles: Candle[];
}

interface EnglishDrill {
  id: string;
  topic: string;
  level: string;
  sentence: string;
  phoneticHint: string;
  meaningVi: string;
  speakingPrompt: string;
  sampleAnswer: string;
}

interface CodeChallenge {
  id: string;
  title: string;
  lang: string;
  prompt: string;
  starterCode: string;
  solutionCode: string;
  expectedKeywords: string[];
  blankLines?: Array<{ before: string; answer: string; after: string; hint: string }>;
}

interface CasePracticeData {
  title: string;
  scenario: string;
  criteria: string[];
}

const DEFAULT_FINANCE_SCENARIOS: FinanceScenario[] = [
  {
    id: "pinbar-support",
    title: "Nến rút chân (Bullish Pin Bar) tại vùng Hỗ trợ cứng",
    ticker: "VN30 / BTC-USDT",
    timeframe: "H4",
    context:
      "Giá điều chỉnh 6 phiên liên tiếp về đúng vùng cầu mạnh 98.0 – 100.0 (đáy cũ tháng trước). Ở nến số 16 xuất hiện nến rút chân dưới dài với khối lượng giao dịch tăng vọt, cho thấy phe mua hấp thụ mạnh lực bán.",
    supportZone: [97.5, 100.5],
    resistanceZone: [111.0, 114.0],
    defaultEntry: 101.5,
    defaultSl: 97.0,
    defaultTp: 111.5,
    bestAction: "long",
    explanation:
      "Cây nến rút chân (Pin Bar) với râu dưới dài > 2x thân nến đóng cửa xanh ngay trên vùng hỗ trợ kèm Volume đột biến là tín hiệu từ chối giảm giá kinh điển. Đặt Stop Loss an toàn dưới râu nến (97.0) và Take Profit trước vùng kháng cự (111.5) cho tỷ lệ R:R ≈ 2.22.",
    initialCandles: [
      { label: "1", o: 112.0, h: 113.2, l: 110.4, c: 110.8, v: 42 },
      { label: "2", o: 110.8, h: 111.5, l: 109.1, c: 109.6, v: 38 },
      { label: "3", o: 109.6, h: 110.9, l: 108.8, c: 110.2, v: 35 },
      { label: "4", o: 110.2, h: 111.8, l: 109.5, c: 111.4, v: 46 },
      { label: "5", o: 111.4, h: 112.6, l: 109.8, c: 110.0, v: 50 },
      { label: "6", o: 110.0, h: 110.5, l: 107.8, c: 108.2, v: 56 },
      { label: "7", o: 108.2, h: 108.9, l: 106.5, c: 107.0, v: 48 },
      { label: "8", o: 107.0, h: 108.4, l: 106.2, c: 107.9, v: 39 },
      { label: "9", o: 107.9, h: 108.2, l: 105.0, c: 105.4, v: 53 },
      { label: "10", o: 105.4, h: 106.1, l: 103.6, c: 104.1, v: 61 },
      { label: "11", o: 104.1, h: 104.8, l: 102.5, c: 103.0, v: 49 },
      { label: "12", o: 103.0, h: 104.2, l: 101.8, c: 103.6, v: 44 },
      { label: "13", o: 103.6, h: 103.9, l: 101.2, c: 101.8, v: 52 },
      { label: "14", o: 101.8, h: 102.4, l: 100.1, c: 100.6, v: 58 },
      { label: "15", o: 100.6, h: 101.3, l: 98.9, c: 99.6, v: 67 },
      { label: "16", o: 99.6, h: 102.0, l: 97.4, c: 101.6, v: 94 },
    ],
    futureCandles: [
      { label: "17", o: 101.6, h: 104.5, l: 101.1, c: 104.0, v: 82 },
      { label: "18", o: 104.0, h: 106.8, l: 103.2, c: 106.3, v: 75 },
      { label: "19", o: 106.3, h: 107.5, l: 105.1, c: 105.8, v: 54 },
      { label: "20", o: 105.8, h: 109.9, l: 105.4, c: 109.4, v: 86 },
      { label: "21", o: 109.4, h: 112.4, l: 108.9, c: 111.8, v: 91 },
    ],
  },
  {
    id: "false-breakout",
    title: "Bẫy phá vỡ giả (False Breakout) tại vùng Kháng cự",
    ticker: "FPT / ETH-USDT",
    timeframe: "D1",
    context:
      "Sau nhịp tăng mạnh, giá chọc thủng đỉnh kháng cự 118.0 ở nến 15 nhưng ngay lập tức xuất hiện nến nhấn chìm giảm (Bearish Engulfing) đóng cửa lọt ngược lại dưới kháng cự với thanh khoản bán lớn.",
    supportZone: [104.0, 106.5],
    resistanceZone: [117.5, 120.0],
    defaultEntry: 116.5,
    defaultSl: 121.5,
    defaultTp: 106.5,
    bestAction: "short",
    explanation:
      "Phá vỡ giả (Bull Trap) bẫy các lệnh mua đuổi ở đỉnh. Khi nến sau nhấn chìm hoàn toàn thân nến breakout và đóng dưới vùng 117.5, động lượng đảo chiều rất mạnh. Ưu tiên bán/đóng vị thế Long, chặn lỗ trên râu nến 121.5.",
    initialCandles: [
      { label: "1", o: 105.0, h: 107.0, l: 104.4, c: 106.5, v: 45 },
      { label: "2", o: 106.5, h: 108.5, l: 106.0, c: 108.1, v: 50 },
      { label: "3", o: 108.1, h: 109.2, l: 107.0, c: 107.5, v: 41 },
      { label: "4", o: 107.5, h: 110.4, l: 107.2, c: 109.9, v: 58 },
      { label: "5", o: 109.9, h: 112.2, l: 109.4, c: 111.6, v: 64 },
      { label: "6", o: 111.6, h: 113.0, l: 110.8, c: 111.2, v: 47 },
      { label: "7", o: 111.2, h: 114.5, l: 110.9, c: 114.0, v: 69 },
      { label: "8", o: 114.0, h: 116.8, l: 113.5, c: 116.1, v: 74 },
      { label: "9", o: 116.1, h: 118.4, l: 115.6, c: 117.3, v: 80 },
      { label: "10", o: 117.3, h: 118.2, l: 115.0, c: 115.6, v: 62 },
      { label: "11", o: 115.6, h: 117.1, l: 114.8, c: 116.8, v: 55 },
      { label: "12", o: 116.8, h: 118.5, l: 116.0, c: 117.6, v: 66 },
      { label: "13", o: 117.6, h: 118.9, l: 116.2, c: 116.9, v: 59 },
      { label: "14", o: 116.9, h: 118.6, l: 116.4, c: 118.1, v: 70 },
      { label: "15", o: 118.1, h: 121.0, l: 117.9, c: 119.5, v: 63 },
      { label: "16", o: 119.5, h: 120.2, l: 115.8, c: 116.2, v: 95 },
    ],
    futureCandles: [
      { label: "17", o: 116.2, h: 117.0, l: 113.2, c: 113.8, v: 88 },
      { label: "18", o: 113.8, h: 114.5, l: 111.0, c: 111.4, v: 79 },
      { label: "19", o: 111.4, h: 112.6, l: 109.5, c: 110.2, v: 65 },
      { label: "20", o: 110.2, h: 110.9, l: 107.1, c: 107.5, v: 83 },
      { label: "21", o: 107.5, h: 108.1, l: 105.6, c: 106.2, v: 90 },
    ],
  },
  {
    id: "choppy-wait",
    title: "Vùng đi ngang nhiễu (Sideways) — Bài học kiên nhẫn đứng ngoài",
    ticker: "MWG / GOLD",
    timeframe: "H1",
    context:
      "Giá dao động giằng co giữa biên 100 – 106 với hàng loạt nến Doji râu hai đầu, khối lượng sụt giảm và không có vùng cản rõ ràng. Đặt lệnh giữa vùng nhiễu rất dễ bị quét cả hai đầu.",
    supportZone: [99.0, 100.5],
    resistanceZone: [106.0, 107.5],
    defaultEntry: 103.0,
    defaultSl: 100.8,
    defaultTp: 106.2,
    bestAction: "wait",
    explanation:
      "Đứng ngoài cũng là một quyết định giao dịch! Giữa biên sideways, khoảng cách tới kháng cự gần nhưng dễ bị quét SL hai đầu, tỷ lệ R:R kém hấp dẫn (< 1.5) và không có lợi thế xác suất.",
    initialCandles: [
      { label: "1", o: 102.0, h: 104.2, l: 101.2, c: 103.4, v: 42 },
      { label: "2", o: 103.4, h: 105.0, l: 102.1, c: 102.4, v: 39 },
      { label: "3", o: 102.4, h: 104.8, l: 101.5, c: 104.1, v: 44 },
      { label: "4", o: 104.1, h: 105.2, l: 102.0, c: 102.6, v: 36 },
      { label: "5", o: 102.6, h: 103.9, l: 100.9, c: 101.4, v: 35 },
      { label: "6", o: 101.4, h: 104.4, l: 101.0, c: 103.8, v: 40 },
      { label: "7", o: 103.8, h: 104.9, l: 101.9, c: 102.2, v: 34 },
      { label: "8", o: 102.2, h: 104.1, l: 101.2, c: 103.5, v: 31 },
      { label: "9", o: 103.5, h: 104.7, l: 101.8, c: 102.7, v: 33 },
      { label: "10", o: 102.7, h: 104.6, l: 101.4, c: 103.9, v: 37 },
      { label: "11", o: 103.9, h: 105.1, l: 102.2, c: 102.5, v: 29 },
      { label: "12", o: 102.5, h: 104.0, l: 101.1, c: 103.3, v: 28 },
      { label: "13", o: 103.3, h: 104.5, l: 101.7, c: 102.8, v: 30 },
      { label: "14", o: 102.8, h: 104.2, l: 101.4, c: 103.4, v: 27 },
      { label: "15", o: 103.4, h: 104.8, l: 101.8, c: 102.9, v: 29 },
      { label: "16", o: 102.9, h: 104.1, l: 101.9, c: 103.1, v: 25 },
    ],
    futureCandles: [
      { label: "17", o: 103.1, h: 105.0, l: 100.6, c: 102.4, v: 41 },
      { label: "18", o: 102.4, h: 104.6, l: 100.9, c: 103.8, v: 35 },
      { label: "19", o: 103.8, h: 104.9, l: 101.3, c: 102.0, v: 38 },
      { label: "20", o: 102.0, h: 104.5, l: 101.5, c: 103.6, v: 32 },
      { label: "21", o: 103.6, h: 104.7, l: 101.7, c: 102.8, v: 30 },
    ],
  },
];

const DEFAULT_ENGLISH_DRILLS: EnglishDrill[] = [
  {
    id: "eng-cognitive",
    topic: "IELTS Listening & Academic Fluency",
    level: "Band 6.5 – 7.5",
    sentence:
      "Consistent daily practice builds long-term retention far better than cramming before an exam.",
    phoneticHint: "/kənˈsɪstənt ˈdeɪli ˈpræktɪs bɪldz lɒŋ-tɜːm rɪˈtenʃən/",
    meaningVi:
      "Việc luyện tập đều đặn hằng ngày giúp ghi nhớ dài hạn tốt hơn nhiều so với học nhồi trước kỳ thi.",
    speakingPrompt:
      "Describe a study habit that helps you remember new knowledge effectively (45–120 seconds).",
    sampleAnswer:
      "One habit that has transformed my learning is spaced repetition. Instead of reviewing notes once and forgetting them, I turn key concepts into short recall prompts and practice speaking aloud for fifteen minutes every evening.",
  },
  {
    id: "eng-finance-news",
    topic: "Business & Analytical Listening",
    level: "Band 7.0+",
    sentence:
      "Investors should evaluate downside risk carefully before allocating capital to volatile assets.",
    phoneticHint: "/ɪnˈvestərz ʃʊd ɪˈvæljueɪt ˈdaʊnsaɪd rɪsk ˈkeərfəli/",
    meaningVi:
      "Nhà đầu tư nên đánh giá kỹ rủi ro giảm giá trước khi phân bổ vốn vào các tài sản biến động mạnh.",
    speakingPrompt:
      "Explain why risk management matters more than predicting short-term price movements.",
    sampleAnswer:
      "Nobody can predict every market swing accurately, so limiting each potential loss to a small percentage of capital keeps you in the game during uncertain periods.",
  },
  {
    id: "eng-tech",
    topic: "Technical Communication",
    level: "Intermediate – B2",
    sentence:
      "Clear documentation allows engineers to understand trade-offs without reading every single file.",
    phoneticHint: "/klɪər ˌdɒkjumenˈteɪʃən əˈlaʊz ˌendʒɪˈnɪərz/",
    meaningVi:
      "Tài liệu rõ ràng giúp kỹ sư hiểu được các đánh đổi thiết kế mà không cần đọc từng tệp mã nguồn.",
    speakingPrompt:
      "Talk about a complex topic or project you recently learned and how you simplified it.",
    sampleAnswer:
      "When I started learning full-stack architecture, breaking the flow into request, validation, and database layers made debugging much faster.",
  },
];

function stripDiacritics(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function normalizeWordList(text: string): string[] {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9'\s-]/g, " ")
    .split(/\s+/)
    .map((w) => w.trim())
    .filter(Boolean);
}

function detectPreferredMode(courseTitle: string, lessonTitle: string, md: string): PracticeMode {
  if (/```practice-finance\b/i.test(md)) return "finance";
  if (/```practice-english\b/i.test(md)) return "english";
  if (/```practice-code\b/i.test(md)) return "code";
  if (/```practice-case\b/i.test(md)) return "recall";

  const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${md.slice(0, 1200)}`);
  if (
    /\b(tai chinh|dau tu|chung khoan|investing|finance|nen|candlestick|co phieu|dong tien|danh muc|trai phieu)\b/.test(
      combined
    )
  ) {
    return "finance";
  }
  if (
    /\b(ielts|tieng anh|english|listening|speaking|vocabulary|grammar|phat am|reading|writing)\b/.test(
      combined
    )
  ) {
    return "english";
  }
  if (
    /\b(\.net|c#|csharp|react|typescript|javascript|sql|api|docker|ef core|backend|frontend|fullstack|git|code)\b/.test(
      combined
    ) ||
    /```(csharp|cs|ts|tsx|js|jsx|sql|python|py|bash|json)\b/i.test(md)
  ) {
    return "code";
  }
  return "recall";
}

function parseCustomBlocks(md: string) {
  const finance: FinanceScenario[] = [];
  const english: EnglishDrill[] = [];
  const codeBlocks: Array<{ lang: string; code: string }> = [];
  const customCases: CasePracticeData[] = [];

  const fenceRegex = /```([a-zA-Z0-9_-]*)\r?\n([\s\S]*?)```/g;
  let match: RegExpExecArray | null;
  while ((match = fenceRegex.exec(md)) !== null) {
    const lang = (match[1] || "").toLowerCase();
    const body = (match[2] || "").trim();
    if (!body) continue;

    if (lang === "practice-finance") {
      try {
        const parsed = JSON.parse(body) as Partial<FinanceScenario>;
        if (parsed.title && Array.isArray(parsed.initialCandles) && parsed.initialCandles.length >= 4) {
          finance.push({
            id: `author-fin-${finance.length}`,
            title: parsed.title,
            ticker: parsed.ticker || "Tài sản thực hành",
            timeframe: parsed.timeframe || "D1",
            context: parsed.context || "Phân tích hành động giá và thiết lập quản trị rủi ro.",
            supportZone: parsed.supportZone || [97, 100],
            resistanceZone: parsed.resistanceZone || [110, 113],
            defaultEntry: Number(parsed.defaultEntry ?? 101),
            defaultSl: Number(parsed.defaultSl ?? 97),
            defaultTp: Number(parsed.defaultTp ?? 111),
            bestAction: parsed.bestAction || "long",
            explanation: parsed.explanation || "Đối chiếu cấu trúc nến và tỷ lệ R:R.",
            initialCandles: parsed.initialCandles,
            futureCandles: Array.isArray(parsed.futureCandles) ? parsed.futureCandles : [],
          });
        }
      } catch {
        // Ignore malformed author JSON cleanly
      }
    } else if (lang === "practice-english") {
      try {
        const parsed = JSON.parse(body) as Partial<EnglishDrill>;
        if (parsed.sentence || parsed.speakingPrompt) {
          english.push({
            id: `author-eng-${english.length}`,
            topic: parsed.topic || "Bài luyện Nghe & Nói theo bài giảng",
            level: parsed.level || "Theo giáo trình",
            sentence: parsed.sentence || DEFAULT_ENGLISH_DRILLS[0].sentence,
            phoneticHint: parsed.phoneticHint || "",
            meaningVi: parsed.meaningVi || "",
            speakingPrompt:
              parsed.speakingPrompt || DEFAULT_ENGLISH_DRILLS[0].speakingPrompt,
            sampleAnswer: parsed.sampleAnswer || "",
          });
        }
      } catch {
        // Ignore malformed author JSON cleanly
      }
    } else if (lang === "practice-code") {
      try {
        const parsed = JSON.parse(body) as Partial<CodeChallenge>;
        if (parsed.prompt || parsed.solutionCode) {
          codeBlocks.push({
            lang: `practice-code::${JSON.stringify(parsed)}`,
            code: parsed.solutionCode || "",
          });
        }
      } catch {
        // Ignore
      }
    } else if (lang === "practice-case") {
      try {
        const parsed = JSON.parse(body) as Partial<CasePracticeData>;
        if (parsed.scenario) {
          customCases.push({
            title: parsed.title || "Tình huống thực hành",
            scenario: parsed.scenario,
            criteria: Array.isArray(parsed.criteria) ? parsed.criteria : [],
          });
        }
      } catch {
        // Ignore
      }
    } else if (!lang.startsWith("practice-")) {
      if (body.length >= 20 && body.split("\n").length >= 2) {
        codeBlocks.push({ lang: lang || "text", code: body });
      }
    }
  }

  return { finance, english, codeBlocks, customCases };
}

function extractKeyTerms(md: string, lessonTitle: string): string[] {
  const terms = new Set<string>();
  const boldRegex = /\*\*([^*\n]{3,45})\*\*/g;
  let m: RegExpExecArray | null;
  while ((m = boldRegex.exec(md)) !== null) {
    const clean = m[1].replace(/[:`[\]]/g, "").trim();
    if (clean.length >= 3 && clean.length <= 42 && !clean.startsWith("LO-")) {
      terms.add(clean);
    }
    if (terms.size >= 8) break;
  }

  if (terms.size < 4) {
    const headingRegex = /^#{1,3}\s+(.+)$/gm;
    while ((m = headingRegex.exec(md)) !== null) {
      const clean = m[1].replace(/[*`#]/g, "").trim();
      if (clean.length >= 4 && clean.length <= 45) terms.add(clean);
      if (terms.size >= 8) break;
    }
  }

  if (terms.size === 0 && lessonTitle) {
    for (const part of lessonTitle.split(/[\s—–-]+/).filter((p) => p.length > 3)) {
      terms.add(part);
      if (terms.size >= 5) break;
    }
  }

  return Array.from(terms).slice(0, 7);
}

function buildCodeChallenges(
  lessonTitle: string,
  parsedCodeBlocks: Array<{ lang: string; code: string }>
): CodeChallenge[] {
  const challenges: CodeChallenge[] = [];

  for (let i = 0; i < parsedCodeBlocks.length; i++) {
    const blk = parsedCodeBlocks[i];
    if (blk.lang.startsWith("practice-code::")) {
      try {
        const parsed = JSON.parse(blk.lang.slice("practice-code::".length)) as Partial<CodeChallenge>;
        challenges.push({
          id: `author-code-${i}`,
          title: parsed.title || "Thử thách lập trình theo bài",
          lang: parsed.lang || "csharp",
          prompt: parsed.prompt || "Hoàn thiện đoạn mã theo yêu cầu bên dưới.",
          starterCode: parsed.starterCode || "// Viết lời giải của bạn tại đây...\n",
          solutionCode: parsed.solutionCode || "",
          expectedKeywords: Array.isArray(parsed.expectedKeywords) ? parsed.expectedKeywords : [],
        });
      } catch {
        // Ignore
      }
      continue;
    }

    const lines = blk.code.split(/\r?\n/);
    const candidates = lines
      .map((line, idx) => ({ line, idx }))
      .filter(({ line }) => line.trim().length > 8 && !line.trim().startsWith("//") && !line.trim().startsWith("#"))
      .slice(0, 2);

    const starterLines = [...lines];
    const blankLines: Array<{ before: string; answer: string; after: string; hint: string }> = [];
    for (const c of candidates) {
      starterLines[c.idx] = "    // TODO: Tự gõ lại dòng xử lý quan trọng tại đây";
      blankLines.push({
        before: c.idx > 0 ? lines[c.idx - 1] : "",
        answer: c.line.trim(),
        after: c.idx + 1 < lines.length ? lines[c.idx + 1] : "",
        hint: `Dòng ${c.idx + 1} trong khối ${blk.lang.toUpperCase()}`,
      });
    }

    challenges.push({
      id: `lesson-code-${i}`,
      title: `Thực hành ${blk.lang.toUpperCase()} rút từ bài "${lessonTitle}"`,
      lang: blk.lang,
      prompt: `Viết lại phần logic bị ẩn trong đoạn mã ${blk.lang.toUpperCase()} hoặc tự viết một biến thể mới giải quyết cùng bài toán.`,
      starterCode: starterLines.join("\n"),
      solutionCode: blk.code,
      expectedKeywords: candidates.map((c) => c.line.trim().split(/\s+/)[0]).filter(Boolean),
      blankLines,
    });
  }

  if (challenges.length === 0) {
    challenges.push(
      {
        id: "default-dotnet",
        title: "C# / ASP.NET Core — Kiểm tra đầu vào & tính tổng chi tiêu hợp lệ",
        lang: "csharp",
        prompt:
          "Viết hàm `CalculateValidTotal(IEnumerable<decimal> amounts)` bỏ qua các khoản <= 0 và ném `ArgumentNullException` nếu danh sách null.",
        starterCode: `public static decimal CalculateValidTotal(IEnumerable<decimal> amounts)\n{\n    // 1. Kiểm tra null\n    // 2. Lọc số dương và cộng tổng\n    return 0m;\n}`,
        solutionCode: `public static decimal CalculateValidTotal(IEnumerable<decimal> amounts)\n{\n    ArgumentNullException.ThrowIfNull(amounts);\n    return amounts.Where(x => x > 0).Sum();\n}`,
        expectedKeywords: ["ArgumentNullException", "Where", "Sum"],
      },
      {
        id: "default-react",
        title: "React + TypeScript — State bộ lọc không mutate mảng gốc",
        lang: "typescript",
        prompt:
          "Hoàn thiện hàm `toggleSelectedTag(prev: string[], tag: string): string[]` trả về mảng mới (nếu đã có thì bỏ, chưa có thì thêm).",
        starterCode: `export function toggleSelectedTag(prev: string[], tag: string): string[] {\n  // Không dùng prev.push() trực tiếp\n  return prev;\n}`,
        solutionCode: `export function toggleSelectedTag(prev: string[], tag: string): string[] {\n  return prev.includes(tag)\n    ? prev.filter((item) => item !== tag)\n    : [...prev, tag];\n}`,
        expectedKeywords: ["includes", "filter", "...prev"],
      }
    );
  }

  return challenges;
}

export function LessonPracticeLab({
  courseTitle,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
}: LessonPracticeLabProps) {
  const { activeUserHeaderId } = useWorkspaceStore();

  const parsedCustom = useMemo(() => parseCustomBlocks(lessonMarkdown), [lessonMarkdown]);
  const defaultMode = useMemo(
    () => detectPreferredMode(courseTitle, lessonTitle, lessonMarkdown),
    [courseTitle, lessonTitle, lessonMarkdown]
  );

  const [mode, setMode] = useState<PracticeMode>(defaultMode);
  useEffect(() => {
    setMode(defaultMode);
  }, [defaultMode, lessonTitle]);

  const [savingNote, setSavingNote] = useState(false);
  const [labMessage, setLabMessage] = useState<string | null>(null);

  const notify = (msg: string) => {
    setLabMessage(msg);
    setTimeout(() => setLabMessage((c) => (c === msg ? null : c)), 4000);
  };

  // ==================== 1. FINANCE CANDLESTICK LAB ====================
  const financeScenarios = useMemo(
    () =>
      parsedCustom.finance.length > 0
        ? [...parsedCustom.finance, ...DEFAULT_FINANCE_SCENARIOS]
        : DEFAULT_FINANCE_SCENARIOS,
    [parsedCustom.finance]
  );

  const [finIdx, setFinIdx] = useState(0);
  const activeScenario = financeScenarios[finIdx] ?? financeScenarios[0];

  const [finAction, setFinAction] = useState<"long" | "short" | "wait">(activeScenario.bestAction);
  const [capitalUsd, setCapitalUsd] = useState(10000);
  const [riskPercent, setRiskPercent] = useState(1.5);
  const [entryPrice, setEntryPrice] = useState(activeScenario.defaultEntry);
  const [stopLoss, setStopLoss] = useState(activeScenario.defaultSl);
  const [takeProfit, setTakeProfit] = useState(activeScenario.defaultTp);
  const [finRevealed, setFinRevealed] = useState(false);
  const [hoveredCandleIdx, setHoveredCandleIdx] = useState<number | null>(null);

  useEffect(() => {
    const sc = financeScenarios[finIdx] ?? financeScenarios[0];
    setFinAction(sc.bestAction);
    setEntryPrice(sc.defaultEntry);
    setStopLoss(sc.defaultSl);
    setTakeProfit(sc.defaultTp);
    setFinRevealed(false);
    setHoveredCandleIdx(null);
  }, [finIdx, financeScenarios]);

  const visibleCandles = useMemo(
    () =>
      finRevealed
        ? [...activeScenario.initialCandles, ...activeScenario.futureCandles]
        : activeScenario.initialCandles,
    [activeScenario, finRevealed]
  );

  const riskMetrics = useMemo(() => {
    const riskAmount = Math.max(0, (capitalUsd * riskPercent) / 100);
    const slDist = Math.abs(entryPrice - stopLoss);
    const tpDist = Math.abs(takeProfit - entryPrice);
    const rr = slDist > 0 ? tpDist / slDist : 0;
    const positionUnits = slDist > 0 ? riskAmount / slDist : 0;
    const validDirection =
      finAction === "wait" ||
      (finAction === "long" && stopLoss < entryPrice && takeProfit > entryPrice) ||
      (finAction === "short" && stopLoss > entryPrice && takeProfit < entryPrice);

    return { riskAmount, slDist, tpDist, rr, positionUnits, validDirection };
  }, [capitalUsd, riskPercent, entryPrice, stopLoss, takeProfit, finAction]);

  const financeOutcome = useMemo(() => {
    if (!finRevealed) return null;
    if (finAction === "wait") {
      return {
        verdict: activeScenario.bestAction === "wait" ? "win" : "neutral",
        headline:
          activeScenario.bestAction === "wait"
            ? "Quyết định chuẩn xác — Bảo toàn vốn thành công"
            : "Bạn đã đứng ngoài bỏ qua cơ hội đẹp",
        pnlUsd: 0,
        rMultiple: 0,
      };
    }

    for (const c of activeScenario.futureCandles) {
      if (finAction === "long") {
        if (c.l <= stopLoss) {
          return {
            verdict: "loss",
            headline: `Chạm cắt lỗ (SL ${stopLoss}) tại nến #${c.label}`,
            pnlUsd: -riskMetrics.riskAmount,
            rMultiple: -1,
          };
        }
        if (c.h >= takeProfit) {
          return {
            verdict: "win",
            headline: `Chốt lời thành công (TP ${takeProfit}) tại nến #${c.label}!`,
            pnlUsd: riskMetrics.riskAmount * riskMetrics.rr,
            rMultiple: riskMetrics.rr,
          };
        }
      } else {
        if (c.h >= stopLoss) {
          return {
            verdict: "loss",
            headline: `Chạm cắt lỗ (SL ${stopLoss}) tại nến #${c.label}`,
            pnlUsd: -riskMetrics.riskAmount,
            rMultiple: -1,
          };
        }
        if (c.l <= takeProfit) {
          return {
            verdict: "win",
            headline: `Chốt lời Short thành công (TP ${takeProfit}) tại nến #${c.label}!`,
            pnlUsd: riskMetrics.riskAmount * riskMetrics.rr,
            rMultiple: riskMetrics.rr,
          };
        }
      }
    }

    const lastClose =
      activeScenario.futureCandles[activeScenario.futureCandles.length - 1]?.c ?? entryPrice;
    const diff = finAction === "long" ? lastClose - entryPrice : entryPrice - lastClose;
    const rMult = riskMetrics.slDist > 0 ? diff / riskMetrics.slDist : 0;
    return {
      verdict: diff >= 0 ? "win" : "loss",
      headline: `Đóng lệnh cuối phiên mô phỏng ở giá ${lastClose.toFixed(1)}`,
      pnlUsd: riskMetrics.riskAmount * rMult,
      rMultiple: rMult,
    };
  }, [finRevealed, finAction, activeScenario, stopLoss, takeProfit, entryPrice, riskMetrics]);

  // SVG chart bounds
  const chartGeom = useMemo(() => {
    const prices = visibleCandles.flatMap((c) => [c.h, c.l]);
    if (finAction !== "wait") {
      prices.push(entryPrice, stopLoss, takeProfit);
    }
    const minP = Math.min(...prices, activeScenario.supportZone[0]) - 1;
    const maxP = Math.max(...prices, activeScenario.resistanceZone[1]) + 1;
    const maxVol = Math.max(...visibleCandles.map((c) => c.v), 10);
    return { minP, maxP, maxVol };
  }, [visibleCandles, finAction, entryPrice, stopLoss, takeProfit, activeScenario]);

  const handleSaveFinanceJournal = async () => {
    setSavingNote(true);
    try {
      const actionLabel =
        finAction === "long" ? "Mua (Long)" : finAction === "short" ? "Bán (Short)" : "Đứng ngoài";
      const summary = [
        `### 📈 Nhật ký thực hành Nến: ${activeScenario.title}`,
        `- Hành động: **${actionLabel}** | Vốn: $${capitalUsd} | Rủi ro: ${riskPercent}% ($${riskMetrics.riskAmount.toFixed(0)})`,
        finAction !== "wait"
          ? `- Entry: ${entryPrice} | SL: ${stopLoss} | TP: ${takeProfit} | Tỷ lệ R:R = 1 : ${riskMetrics.rr.toFixed(2)}`
          : `- Giữ tiền mặt quan sát cấu trúc thị trường.`,
        financeOutcome
          ? `- Kết quả mô phỏng: **${financeOutcome.headline}** (${financeOutcome.rMultiple >= 0 ? "+" : ""}${financeOutcome.rMultiple.toFixed(2)}R)`
          : "",
        `- Bài học: ${activeScenario.explanation}`,
      ]
        .filter(Boolean)
        .join("\n");
      await onSaveToNotebook(summary);
      notify("Đã lưu nhật ký giao dịch vào Sổ tay bài học.");
    } finally {
      setSavingNote(false);
    }
  };

  // ==================== 2. ENGLISH LISTENING & SPEAKING LAB ====================
  const englishDrills = useMemo(
    () =>
      parsedCustom.english.length > 0
        ? [...parsedCustom.english, ...DEFAULT_ENGLISH_DRILLS]
        : DEFAULT_ENGLISH_DRILLS,
    [parsedCustom.english]
  );
  const [engIdx, setEngIdx] = useState(0);
  const activeDrill = englishDrills[engIdx] ?? englishDrills[0];

  const [ttsRate, setTtsRate] = useState<number>(1.0);
  const [ttsAccent, setTtsAccent] = useState<"en-US" | "en-GB">("en-US");
  const [isSpeakingTts, setIsSpeakingTts] = useState(false);
  const [dictationInput, setDictationInput] = useState("");
  const [dictationChecked, setDictationChecked] = useState(false);

  const [isListeningMic, setIsListeningMic] = useState(false);
  const [spokenTranscript, setSpokenTranscript] = useState("");
  const [speakingAiReview, setSpeakingAiReview] = useState("");
  const [speakingAiLoading, setSpeakingAiLoading] = useState(false);
  const [speechSeconds, setSpeechSeconds] = useState(0);
  const [speechTimerActive, setSpeechTimerActive] = useState(false);
  const [recordedAudioUrl, setRecordedAudioUrl] = useState<string | null>(null);
  const recognitionRef = useRef<{ stop: () => void } | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);

  useEffect(() => {
    setDictationInput("");
    setDictationChecked(false);
    setSpokenTranscript("");
    setSpeakingAiReview("");
    setSpeechSeconds(0);
    setSpeechTimerActive(false);
  }, [engIdx, englishDrills]);

  useEffect(() => {
    if (!speechTimerActive) return;
    const id = setInterval(() => setSpeechSeconds((s) => s + 1), 1000);
    return () => clearInterval(id);
  }, [speechTimerActive]);

  useEffect(() => {
    return () => {
      if (typeof window !== "undefined" && "speechSynthesis" in window) {
        window.speechSynthesis.cancel();
      }
      if (recognitionRef.current) {
        try {
          recognitionRef.current.stop();
        } catch {
          // ignore
        }
      }
      if (recordedAudioUrl) {
        URL.revokeObjectURL(recordedAudioUrl);
      }
    };
  }, [recordedAudioUrl]);

  const handlePlayTts = (textToSpeak: string) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window)) {
      notify("Trình duyệt hiện tại không hỗ trợ Web Speech API (TTS).");
      return;
    }
    window.speechSynthesis.cancel();
    const utter = new SpeechSynthesisUtterance(textToSpeak);
    utter.lang = ttsAccent;
    utter.rate = ttsRate;
    utter.onstart = () => setIsSpeakingTts(true);
    utter.onend = () => setIsSpeakingTts(false);
    utter.onerror = () => setIsSpeakingTts(false);
    window.speechSynthesis.speak(utter);
  };

  const dictationEvaluation = useMemo(() => {
    const targetWords = normalizeWordList(activeDrill.sentence);
    const typedWords = normalizeWordList(dictationInput);
    const typedSet = new Set(typedWords);
    let hits = 0;
    const tokens = targetWords.map((w, idx) => {
      const exactPos = typedWords[idx] === w;
      const nearby = exactPos || typedSet.has(w);
      if (nearby) hits++;
      return { word: w, status: exactPos ? "exact" : nearby ? "close" : "missed" };
    });
    const accuracy = targetWords.length > 0 ? Math.round((hits * 100) / targetWords.length) : 0;
    return { tokens, accuracy, hits, total: targetWords.length };
  }, [activeDrill.sentence, dictationInput]);

  const speakingScore = useMemo(() => {
    const targetWords = normalizeWordList(activeDrill.sentence);
    const spokenWords = normalizeWordList(spokenTranscript);
    if (spokenWords.length === 0) return null;
    const spokenSet = new Set(spokenWords);
    const hits = targetWords.filter((w) => spokenSet.has(w)).length;
    const accuracy = targetWords.length > 0 ? Math.round((hits * 100) / targetWords.length) : 0;
    return { hits, total: targetWords.length, accuracy };
  }, [activeDrill.sentence, spokenTranscript]);

  const handleToggleMic = async () => {
    if (isListeningMic) {
      recognitionRef.current?.stop();
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
        mediaRecorderRef.current.stop();
      }
      setIsListeningMic(false);
      setSpeechTimerActive(false);
      return;
    }

    const win = window as unknown as {
      SpeechRecognition?: new () => {
        lang: string;
        interimResults: boolean;
        continuous: boolean;
        onresult: ((ev: { results: ArrayLike<{ 0: { transcript: string } }> }) => void) | null;
        onerror: (() => void) | null;
        onend: (() => void) | null;
        start: () => void;
        stop: () => void;
      };
      webkitSpeechRecognition?: new () => {
        lang: string;
        interimResults: boolean;
        continuous: boolean;
        onresult: ((ev: { results: ArrayLike<{ 0: { transcript: string } }> }) => void) | null;
        onerror: (() => void) | null;
        onend: (() => void) | null;
        start: () => void;
        stop: () => void;
      };
    };

    const SpeechRec = win.SpeechRecognition || win.webkitSpeechRecognition;
    if (SpeechRec) {
      const rec = new SpeechRec();
      rec.lang = ttsAccent;
      rec.interimResults = true;
      rec.continuous = true;
      rec.onresult = (ev) => {
        const lines: string[] = [];
        for (let i = 0; i < ev.results.length; i++) {
          lines.push(ev.results[i][0].transcript);
        }
        setSpokenTranscript(lines.join(" ").trim());
      };
      rec.onerror = () => {
        setIsListeningMic(false);
        setSpeechTimerActive(false);
      };
      rec.onend = () => {
        setIsListeningMic(false);
        setSpeechTimerActive(false);
      };
      rec.start();
      recognitionRef.current = rec;
    } else {
      notify("Thiết bị chưa bật Web Speech Recognition — bạn có thể gõ/dán bản nói vào ô bên dưới để chấm.");
    }

    if (typeof navigator !== "undefined" && navigator.mediaDevices?.getUserMedia) {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const recorder = new MediaRecorder(stream);
        const chunks: BlobPart[] = [];
        recorder.ondataavailable = (e) => {
          if (e.data.size > 0) chunks.push(e.data);
        };
        recorder.onstop = () => {
          const blob = new Blob(chunks, { type: "audio/webm" });
          const nextUrl = URL.createObjectURL(blob);
          setRecordedAudioUrl(nextUrl);
          stream.getTracks().forEach((t) => t.stop());
        };
        recorder.start();
        mediaRecorderRef.current = recorder;
      } catch {
        // Microphone denied or unavailable; allow manual transcript testing
      }
    }

    setIsListeningMic(true);
    setSpeechTimerActive(true);
  };

  const handleAiFeedbackEnglish = async () => {
    const answerText = spokenTranscript.trim() || dictationInput.trim();
    if (!answerText) {
      notify("Hãy nói hoặc nhập câu tiếng Anh của bạn trước khi gọi AI nhận xét.");
      return;
    }
    setSpeakingAiLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json",
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers,
        body: JSON.stringify({
          excerpt: `Prompt: ${activeDrill.speakingPrompt}\nUser Answer: ${answerText}`,
          contextTitle: `IELTS Speaking & Writing Review — ${lessonTitle}`,
          actionType: "explain",
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setSpeakingAiReview(data.explanation || "Đã hoàn tất đánh giá ngữ pháp & từ vựng.");
      } else {
        setSpeakingAiReview(
          "Không gọi được AI lúc này. Tự rà: kiểm tra thì động từ (Tense), mạo từ (a/an/the) và bổ sung ví dụ thực tế để nâng band diễn đạt."
        );
      }
    } catch {
      setSpeakingAiReview("Lỗi kết nối tới dịch vụ AI.");
    } finally {
      setSpeakingAiLoading(false);
    }
  };

  // ==================== 3. CODE PRACTICE LAB ====================
  const codeChallenges = useMemo(
    () => buildCodeChallenges(lessonTitle, parsedCustom.codeBlocks),
    [lessonTitle, parsedCustom.codeBlocks]
  );
  const [codeIdx, setCodeIdx] = useState(0);
  const activeChallenge = codeChallenges[codeIdx] ?? codeChallenges[0];
  const [userCode, setUserCode] = useState(activeChallenge.starterCode);
  const [showSolution, setShowSolution] = useState(false);
  const [codeCheckResult, setCodeCheckResult] = useState<{
    matched: string[];
    missing: string[];
  } | null>(null);

  useEffect(() => {
    const ch = codeChallenges[codeIdx] ?? codeChallenges[0];
    setUserCode(ch.starterCode);
    setShowSolution(false);
    setCodeCheckResult(null);
  }, [codeIdx, codeChallenges]);

  const handleCheckCode = () => {
    const kws = activeChallenge.expectedKeywords;
    const matched = kws.filter((k) => userCode.includes(k));
    const missing = kws.filter((k) => !userCode.includes(k));
    setCodeCheckResult({ matched, missing });
  };

  // ==================== 4. FEYNMAN RECALL & CASE STUDY ====================
  const keyTerms = useMemo(
    () => extractKeyTerms(lessonMarkdown, lessonTitle),
    [lessonMarkdown, lessonTitle]
  );
  const [recallText, setRecallText] = useState("");
  const [recallChecked, setRecallChecked] = useState(false);
  const [recallAiFeedback, setRecallAiFeedback] = useState("");
  const [recallAiLoading, setRecallAiLoading] = useState(false);

  const recallCoverage = useMemo(() => {
    const normInput = stripDiacritics(recallText);
    const covered: string[] = [];
    const missed: string[] = [];
    for (const term of keyTerms) {
      const normTerm = stripDiacritics(term);
      const tokens = normTerm.split(/\s+/).filter((t) => t.length >= 2);
      const hit =
        normInput.includes(normTerm) ||
        (tokens.length > 0 && tokens.filter((t) => normInput.includes(t)).length >= Math.ceil(tokens.length * 0.6));
      if (hit) covered.push(term);
      else missed.push(term);
    }
    const percent = keyTerms.length > 0 ? Math.round((covered.length * 100) / keyTerms.length) : 0;
    return { covered, missed, percent };
  }, [keyTerms, recallText]);

  const handleRecallAiCheck = async () => {
    if (!recallText.trim()) {
      notify("Hãy viết tóm tắt bằng lời của bạn trước khi nhờ AI đối chiếu.");
      return;
    }
    setRecallAiLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders({
        "Content-Type": "application/json",
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });
      const resp = await fetch("/api/v1/ai/explain", {
        method: "POST",
        headers,
        body: JSON.stringify({
          excerpt: `Tự diễn đạt bài "${lessonTitle}": ${recallText.trim().slice(0, 600)}`,
          contextTitle: lessonTitle,
          actionType: "explain",
        }),
      });
      if (resp.ok) {
        const data = await resp.json();
        setRecallAiFeedback(data.explanation || "");
      } else {
        setRecallAiFeedback("Không gọi được AI lúc này. Hãy xem danh sách ý cốt lõi bên trên để bổ sung.");
      }
    } catch {
      setRecallAiFeedback("Lỗi kết nối khi gọi AI nhận xét.");
    } finally {
      setRecallAiLoading(false);
    }
  };

  // Helper SVG price->Y mapping
  const svgW = 720;
  const svgH = 290;
  const padL = 16;
  const padR = 54;
  const padT = 16;
  const padB = 56;
  const plotW = svgW - padL - padR;
  const plotH = svgH - padT - padB;
  const priceSpan = Math.max(1, chartGeom.maxP - chartGeom.minP);
  const priceToY = useCallback(
    (p: number) => padT + ((chartGeom.maxP - p) / priceSpan) * plotH,
    [chartGeom.maxP, priceSpan, plotH]
  );

  return (
    <div
      data-testid="lesson-practice-lab"
      className="flex-1 overflow-y-auto swms-thin-scrollbar px-4 sm:px-8 py-6"
    >
      <div className="max-w-4xl mx-auto space-y-5">
        {/* Practice Header + Sub-Mode Switcher */}
        <div className="flex flex-wrap items-center justify-between gap-3 pb-4 border-b border-border">
          <div>
            <p className="text-xs font-medium text-accent mb-0.5">Phòng thực hành theo ngữ cảnh bài học</p>
            <h2 className="text-lg font-semibold text-text-primary">{lessonTitle}</h2>
          </div>

          <div className="inline-flex flex-wrap items-center gap-1 rounded-lg border border-border bg-bg-secondary p-1">
            {(
              [
                { id: "finance", label: "Nến & Rủi ro", icon: CandlestickChart },
                { id: "english", label: "Nghe & Nói Anh", icon: Headphones },
                { id: "code", label: "Thực hành Code", icon: Code2 },
                { id: "recall", label: "Tự diễn đạt", icon: Brain },
              ] as const
            ).map((m) => {
              const Icon = m.icon;
              return (
                <button
                  key={m.id}
                  type="button"
                  data-testid={`practice-mode-${m.id}`}
                  onClick={() => setMode(m.id)}
                  className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-md text-xs font-medium transition cursor-pointer ${
                    mode === m.id
                      ? "bg-accent text-white shadow-xs"
                      : "text-text-secondary hover:text-text-primary hover:bg-bg-primary"
                  }`}
                >
                  <Icon className="w-3.5 h-3.5" />
                  <span>{m.label}</span>
                </button>
              );
            })}
          </div>
        </div>

        {labMessage && (
          <div className="rounded-lg border border-accent/30 bg-accent-subtle px-3.5 py-2 text-xs font-medium text-accent">
            {labMessage}
          </div>
        )}

        {/* ==================== MODE 1: FINANCE CANDLESTICK LAB ==================== */}
        {mode === "finance" && (
          <div className="space-y-4">
            {/* Scenario Pills */}
            <div className="flex flex-wrap items-center gap-2">
              {financeScenarios.map((sc, idx) => (
                <button
                  key={sc.id}
                  type="button"
                  onClick={() => setFinIdx(idx)}
                  className={`px-3 py-1.5 rounded-md border text-xs font-medium transition cursor-pointer ${
                    idx === finIdx
                      ? "border-accent bg-accent-subtle text-accent"
                      : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
                  }`}
                >
                  Tình huống {idx + 1}: {sc.title}
                </button>
              ))}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
              {/* Left 2 cols: Interactive SVG Candlestick Chart */}
              <div className="lg:col-span-2 rounded-xl border border-border bg-bg-secondary/40 p-4 space-y-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-semibold px-2 py-0.5 rounded bg-bg-tertiary text-text-primary">
                        {activeScenario.ticker}
                      </span>
                      <span className="text-xs text-text-tertiary">Khung {activeScenario.timeframe}</span>
                    </div>
                    <p className="text-xs text-text-secondary mt-1.5 leading-relaxed">
                      {activeScenario.context}
                    </p>
                  </div>
                </div>

                {/* OHLC Bar */}
                <div className="flex flex-wrap items-center gap-3 text-[11px] font-mono text-text-secondary bg-bg-primary border border-border rounded-md px-3 py-1.5">
                  {(() => {
                    const c =
                      hoveredCandleIdx !== null && visibleCandles[hoveredCandleIdx]
                        ? visibleCandles[hoveredCandleIdx]
                        : visibleCandles[visibleCandles.length - 1];
                    const up = c.c >= c.o;
                    return (
                      <>
                        <span className="font-semibold text-text-primary">Nến #{c.label}</span>
                        <span>O: {c.o.toFixed(1)}</span>
                        <span>H: {c.h.toFixed(1)}</span>
                        <span>L: {c.l.toFixed(1)}</span>
                        <span className={up ? "text-emerald-600 dark:text-emerald-400" : "text-rose-600 dark:text-rose-400"}>
                          C: {c.c.toFixed(1)}
                        </span>
                        <span>Vol: {c.v}</span>
                      </>
                    );
                  })()}
                </div>

                {/* SVG Candlestick + Volume Chart */}
                <div className="w-full rounded-lg border border-border bg-bg-primary overflow-hidden">
                  <svg viewBox={`0 0 ${svgW} ${svgH}`} className="w-full h-auto block select-none">
                    {/* Resistance Zone */}
                    <rect
                      x={padL}
                      y={priceToY(activeScenario.resistanceZone[1])}
                      width={plotW}
                      height={Math.max(
                        4,
                        priceToY(activeScenario.resistanceZone[0]) -
                          priceToY(activeScenario.resistanceZone[1])
                      )}
                      fill="rgba(244, 63, 94, 0.10)"
                    />
                    <text
                      x={padL + 6}
                      y={priceToY(activeScenario.resistanceZone[1]) + 11}
                      fontSize="10"
                      fill="#e11d48"
                      fontWeight="600"
                    >
                      Kháng cự ({activeScenario.resistanceZone[0]}–{activeScenario.resistanceZone[1]})
                    </text>

                    {/* Support Zone */}
                    <rect
                      x={padL}
                      y={priceToY(activeScenario.supportZone[1])}
                      width={plotW}
                      height={Math.max(
                        4,
                        priceToY(activeScenario.supportZone[0]) -
                          priceToY(activeScenario.supportZone[1])
                      )}
                      fill="rgba(16, 185, 129, 0.11)"
                    />
                    <text
                      x={padL + 6}
                      y={priceToY(activeScenario.supportZone[0]) - 4}
                      fontSize="10"
                      fill="#059669"
                      fontWeight="600"
                    >
                      Hỗ trợ ({activeScenario.supportZone[0]}–{activeScenario.supportZone[1]})
                    </text>

                    {/* Horizontal Grid + Price Axis */}
                    {[0, 0.25, 0.5, 0.75, 1].map((t) => {
                      const p = chartGeom.minP + t * priceSpan;
                      const y = priceToY(p);
                      return (
                        <g key={t}>
                          <line
                            x1={padL}
                            y1={y}
                            x2={svgW - padR}
                            y2={y}
                            stroke="currentColor"
                            strokeOpacity={0.08}
                            strokeDasharray="3 3"
                          />
                          <text
                            x={svgW - padR + 6}
                            y={y + 3}
                            fontSize="10"
                            fill="currentColor"
                            opacity={0.55}
                          >
                            {p.toFixed(1)}
                          </text>
                        </g>
                      );
                    })}

                    {/* Entry / SL / TP lines */}
                    {finAction !== "wait" && (
                      <g>
                        <line
                          x1={padL}
                          y1={priceToY(entryPrice)}
                          x2={svgW - padR}
                          y2={priceToY(entryPrice)}
                          stroke="#6366f1"
                          strokeWidth="1.3"
                          strokeDasharray="4 3"
                        />
                        <text x={svgW - padR + 4} y={priceToY(entryPrice) + 3} fontSize="9.5" fill="#6366f1" fontWeight="700">
                          Entry
                        </text>

                        <line
                          x1={padL}
                          y1={priceToY(stopLoss)}
                          x2={svgW - padR}
                          y2={priceToY(stopLoss)}
                          stroke="#f43f5e"
                          strokeWidth="1.3"
                          strokeDasharray="4 3"
                        />
                        <text x={svgW - padR + 4} y={priceToY(stopLoss) + 3} fontSize="9.5" fill="#f43f5e" fontWeight="700">
                          SL
                        </text>

                        <line
                          x1={padL}
                          y1={priceToY(takeProfit)}
                          x2={svgW - padR}
                          y2={priceToY(takeProfit)}
                          stroke="#10b981"
                          strokeWidth="1.3"
                          strokeDasharray="4 3"
                        />
                        <text x={svgW - padR + 4} y={priceToY(takeProfit) + 3} fontSize="9.5" fill="#10b981" fontWeight="700">
                          TP
                        </text>
                      </g>
                    )}

                    {/* Future Reveal Divider */}
                    {finRevealed && (
                      <line
                        x1={
                          padL +
                          (activeScenario.initialCandles.length / visibleCandles.length) * plotW
                        }
                        y1={padT}
                        x2={
                          padL +
                          (activeScenario.initialCandles.length / visibleCandles.length) * plotW
                        }
                        y2={svgH - 20}
                        stroke="#6366f1"
                        strokeWidth="1.2"
                        strokeDasharray="3 3"
                        opacity={0.6}
                      />
                    )}

                    {/* Candles & Volume Bars */}
                    {visibleCandles.map((c, idx) => {
                      const slotW = plotW / Math.max(visibleCandles.length, 16);
                      const cx = padL + idx * slotW + slotW / 2;
                      const bodyW = Math.max(6, Math.min(18, slotW * 0.58));
                      const up = c.c >= c.o;
                      const color = up ? "#10b981" : "#f43f5e";
                      const yHigh = priceToY(c.h);
                      const yLow = priceToY(c.l);
                      const yTop = priceToY(Math.max(c.o, c.c));
                      const yBot = priceToY(Math.min(c.o, c.c));
                      const volH = (c.v / chartGeom.maxVol) * 32;
                      const isFuture = idx >= activeScenario.initialCandles.length;

                      return (
                        <g
                          key={c.label}
                          onMouseEnter={() => setHoveredCandleIdx(idx)}
                          onClick={() => setHoveredCandleIdx(idx)}
                          className="cursor-pointer"
                          opacity={isFuture ? 1 : 0.92}
                        >
                          {/* Wick */}
                          <line x1={cx} y1={yHigh} x2={cx} y2={yLow} stroke={color} strokeWidth="1.6" />
                          {/* Body */}
                          <rect
                            x={cx - bodyW / 2}
                            y={yTop}
                            width={bodyW}
                            height={Math.max(2.5, yBot - yTop)}
                            rx="1.5"
                            fill={color}
                          />
                          {/* Volume */}
                          <rect
                            x={cx - bodyW / 2}
                            y={svgH - 22 - volH}
                            width={bodyW}
                            height={volH}
                            fill={color}
                            opacity={0.28}
                          />
                          {idx % 2 === 0 && (
                            <text
                              x={cx}
                              y={svgH - 7}
                              fontSize="9"
                              textAnchor="middle"
                              fill="currentColor"
                              opacity={0.45}
                            >
                              {c.label}
                            </text>
                          )}
                        </g>
                      );
                    })}
                  </svg>
                </div>

                {/* Evaluation Banner after Reveal */}
                {finRevealed && financeOutcome && (
                  <div
                    data-testid="practice-finance-result"
                    className={`rounded-lg border p-3.5 space-y-2 ${
                      financeOutcome.verdict === "win"
                        ? "border-emerald-500/40 bg-emerald-500/10"
                        : financeOutcome.verdict === "loss"
                          ? "border-rose-500/40 bg-rose-500/10"
                          : "border-border bg-bg-primary"
                    }`}
                  >
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <span className="text-sm font-semibold text-text-primary flex items-center gap-1.5">
                        {financeOutcome.verdict === "win" ? (
                          <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                        ) : financeOutcome.verdict === "loss" ? (
                          <XCircle className="w-4 h-4 text-rose-500" />
                        ) : (
                          <Activity className="w-4 h-4 text-accent" />
                        )}
                        {financeOutcome.headline}
                      </span>
                      {finAction !== "wait" && (
                        <span
                          className={`text-xs font-semibold px-2 py-0.5 rounded ${
                            financeOutcome.pnlUsd >= 0
                              ? "bg-emerald-500/20 text-emerald-700 dark:text-emerald-300"
                              : "bg-rose-500/20 text-rose-700 dark:text-rose-300"
                          }`}
                        >
                          {financeOutcome.pnlUsd >= 0 ? "+" : ""}${financeOutcome.pnlUsd.toFixed(0)} (
                          {financeOutcome.rMultiple >= 0 ? "+" : ""}
                          {financeOutcome.rMultiple.toFixed(2)}R)
                        </span>
                      )}
                    </div>
                    <p className="text-xs text-text-secondary leading-relaxed">
                      {activeScenario.explanation}
                    </p>
                  </div>
                )}
              </div>

              {/* Right 1 col: Order & Risk Calculator */}
              <div className="rounded-xl border border-border bg-bg-primary p-4 flex flex-col justify-between space-y-4">
                <div className="space-y-3.5">
                  <h3 className="text-sm font-semibold text-text-primary">
                    Ra quyết định &amp; Tính khối lượng lệnh
                  </h3>

                  <div className="grid grid-cols-3 gap-1.5">
                    {(
                      [
                        { id: "long", label: "Mua (Long)", icon: TrendingUp },
                        { id: "short", label: "Bán (Short)", icon: TrendingDown },
                        { id: "wait", label: "Đứng ngoài", icon: Activity },
                      ] as const
                    ).map((act) => {
                      const Icon = act.icon;
                      return (
                        <button
                          key={act.id}
                          type="button"
                          onClick={() => setFinAction(act.id)}
                          className={`h-9 px-2 rounded-md border text-xs font-medium inline-flex items-center justify-center gap-1 transition cursor-pointer ${
                            finAction === act.id
                              ? act.id === "long"
                                ? "border-emerald-500 bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                                : act.id === "short"
                                  ? "border-rose-500 bg-rose-500/15 text-rose-700 dark:text-rose-300"
                                  : "border-accent bg-accent-subtle text-accent"
                              : "border-border bg-bg-secondary text-text-secondary"
                          }`}
                        >
                          <Icon className="w-3.5 h-3.5" />
                          <span>{act.label}</span>
                        </button>
                      );
                    })}
                  </div>

                  <div className="grid grid-cols-2 gap-2.5">
                    <div>
                      <label className="block text-[11px] font-medium text-text-secondary mb-1">
                        Vốn tài khoản ($)
                      </label>
                      <input
                        type="number"
                        value={capitalUsd}
                        onChange={(e) => setCapitalUsd(Math.max(100, Number(e.target.value) || 0))}
                        className="w-full h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary"
                      />
                    </div>
                    <div>
                      <label className="block text-[11px] font-medium text-text-secondary mb-1">
                        % Rủi ro / lệnh
                      </label>
                      <input
                        type="number"
                        step="0.5"
                        value={riskPercent}
                        onChange={(e) => setRiskPercent(Math.max(0.1, Number(e.target.value) || 0.5))}
                        className="w-full h-8 px-2.5 rounded-md border border-border bg-bg-secondary text-xs text-text-primary"
                      />
                    </div>
                  </div>

                  {finAction !== "wait" && (
                    <div className="grid grid-cols-3 gap-2">
                      <div>
                        <label className="block text-[11px] font-medium text-text-secondary mb-1">
                          Giá vào (Entry)
                        </label>
                        <input
                          type="number"
                          step="0.5"
                          value={entryPrice}
                          onChange={(e) => setEntryPrice(Number(e.target.value) || 0)}
                          className="w-full h-8 px-2 rounded-md border border-border bg-bg-secondary text-xs text-text-primary"
                        />
                      </div>
                      <div>
                        <label className="block text-[11px] font-medium text-rose-600 dark:text-rose-400 mb-1">
                          Cắt lỗ (SL)
                        </label>
                        <input
                          type="number"
                          step="0.5"
                          value={stopLoss}
                          onChange={(e) => setStopLoss(Number(e.target.value) || 0)}
                          className="w-full h-8 px-2 rounded-md border border-border bg-bg-secondary text-xs text-text-primary"
                        />
                      </div>
                      <div>
                        <label className="block text-[11px] font-medium text-emerald-600 dark:text-emerald-400 mb-1">
                          Chốt lời (TP)
                        </label>
                        <input
                          type="number"
                          step="0.5"
                          value={takeProfit}
                          onChange={(e) => setTakeProfit(Number(e.target.value) || 0)}
                          className="w-full h-8 px-2 rounded-md border border-border bg-bg-secondary text-xs text-text-primary"
                        />
                      </div>
                    </div>
                  )}

                  {/* Risk Metrics Readout */}
                  <div className="rounded-lg border border-border bg-bg-secondary/50 p-3 space-y-1.5 text-xs">
                    <div className="flex justify-between">
                      <span className="text-text-tertiary">Số tiền chấp nhận mất tối đa:</span>
                      <span className="font-semibold text-text-primary">
                        ${riskMetrics.riskAmount.toFixed(0)}
                      </span>
                    </div>
                    {finAction !== "wait" && (
                      <>
                        <div className="flex justify-between">
                          <span className="text-text-tertiary">Tỷ lệ Lợi nhuận / Rủi ro (R:R):</span>
                          <span
                            className={`font-semibold ${
                              riskMetrics.rr >= 1.8
                                ? "text-emerald-600 dark:text-emerald-400"
                                : "text-amber-600 dark:text-amber-400"
                            }`}
                          >
                            1 : {riskMetrics.rr.toFixed(2)}
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-text-tertiary">Khối lượng gợi ý:</span>
                          <span className="font-semibold text-text-primary">
                            {riskMetrics.positionUnits.toFixed(1)} đơn vị
                          </span>
                        </div>
                      </>
                    )}
                  </div>

                  {!riskMetrics.validDirection && (
                    <p className="text-[11px] text-amber-600 dark:text-amber-400 flex items-center gap-1.5">
                      <AlertTriangle className="w-3.5 h-3.5 shrink-0" />
                      Kiểm tra lại chiều SL / TP so với hướng lệnh {finAction === "long" ? "Mua" : "Bán"}.
                    </p>
                  )}
                </div>

                <div className="space-y-2 pt-2 border-t border-border-subtle">
                  {!finRevealed ? (
                    <button
                      type="button"
                      data-testid="practice-finance-reveal-btn"
                      onClick={() => setFinRevealed(true)}
                      className="w-full h-9 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition inline-flex items-center justify-center gap-1.5 cursor-pointer"
                    >
                      <Play className="w-3.5 h-3.5" />
                      <span>Chạy tiếp 5 cây nến tiếp theo</span>
                    </button>
                  ) : (
                    <button
                      type="button"
                      onClick={() => setFinRevealed(false)}
                      className="w-full h-9 px-4 rounded-md border border-border bg-bg-secondary text-text-primary text-xs font-medium hover:bg-bg-tertiary transition inline-flex items-center justify-center gap-1.5 cursor-pointer"
                    >
                      <RotateCcw className="w-3.5 h-3.5" />
                      <span>Thử lại kịch bản này</span>
                    </button>
                  )}

                  <button
                    type="button"
                    disabled={savingNote}
                    onClick={() => void handleSaveFinanceJournal()}
                    className="w-full h-8 px-3 rounded-md border border-border bg-bg-primary text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-secondary transition inline-flex items-center justify-center gap-1.5 cursor-pointer"
                  >
                    <NotebookPen className="w-3.5 h-3.5" />
                    <span>Lưu nhật ký giao dịch vào Sổ tay</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* ==================== MODE 2: ENGLISH LISTENING & SPEAKING LAB ==================== */}
        {mode === "english" && (
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-2">
              {englishDrills.map((dr, idx) => (
                <button
                  key={dr.id}
                  type="button"
                  onClick={() => setEngIdx(idx)}
                  className={`px-3 py-1.5 rounded-md border text-xs font-medium transition cursor-pointer ${
                    idx === engIdx
                      ? "border-accent bg-accent-subtle text-accent"
                      : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
                  }`}
                >
                  Bài tập {idx + 1}: {dr.topic}
                </button>
              ))}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
              {/* Left: Listening Dictation */}
              <div className="rounded-xl border border-border bg-bg-primary p-4 space-y-3.5">
                <div className="flex items-center justify-between gap-2">
                  <span className="text-xs font-semibold text-accent inline-flex items-center gap-1.5">
                    <Headphones className="w-4 h-4" />
                    1. Luyện Nghe &amp; Chép chính tả
                  </span>
                  <span className="text-[11px] px-2 py-0.5 rounded bg-bg-tertiary text-text-secondary">
                    {activeDrill.level}
                  </span>
                </div>

                {/* Audio Controls */}
                <div className="rounded-lg border border-border bg-bg-secondary/50 p-3 flex flex-wrap items-center justify-between gap-2">
                  <button
                    type="button"
                    data-testid="practice-english-tts-btn"
                    onClick={() => handlePlayTts(activeDrill.sentence)}
                    className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
                  >
                    <Volume2 className="w-3.5 h-3.5" />
                    <span>{isSpeakingTts ? "Đang phát..." : "Nghe câu mẫu"}</span>
                  </button>

                  <div className="flex items-center gap-1.5 text-xs">
                    {[0.75, 1.0, 1.2].map((r) => (
                      <button
                        key={r}
                        type="button"
                        onClick={() => setTtsRate(r)}
                        className={`h-7 px-2 rounded border cursor-pointer ${
                          ttsRate === r
                            ? "border-accent bg-accent-subtle text-accent font-semibold"
                            : "border-border bg-bg-primary text-text-secondary"
                        }`}
                      >
                        {r}x
                      </button>
                    ))}
                    <select
                      value={ttsAccent}
                      onChange={(e) => setTtsAccent(e.target.value as "en-US" | "en-GB")}
                      className="h-7 px-2 rounded border border-border bg-bg-primary text-xs text-text-primary"
                    >
                      <option value="en-US">Anh - Mỹ (US)</option>
                      <option value="en-GB">Anh - Anh (UK)</option>
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1">
                    Gõ lại đúng những gì bạn nghe được:
                  </label>
                  <textarea
                    rows={3}
                    value={dictationInput}
                    onChange={(e) => {
                      setDictationInput(e.target.value);
                      setDictationChecked(false);
                    }}
                    placeholder="Nhấn 'Nghe câu mẫu' rồi gõ lại câu tiếng Anh tại đây..."
                    className="w-full p-2.5 rounded-md border border-border bg-bg-secondary text-sm text-text-primary focus:outline-none focus:border-accent"
                  />
                </div>

                <div className="flex flex-wrap items-center gap-2">
                  <button
                    type="button"
                    data-testid="practice-english-check-dictation-btn"
                    onClick={() => setDictationChecked(true)}
                    className="h-8 px-3.5 rounded-md bg-bg-tertiary hover:bg-border text-xs font-medium text-text-primary transition cursor-pointer"
                  >
                    Kiểm tra chính tả &amp; Hiện đáp án
                  </button>
                  {onCreateFlashcardQuick && (
                    <button
                      type="button"
                      onClick={() =>
                        void onCreateFlashcardQuick(
                          `Nghe & dịch: "${activeDrill.sentence}"`,
                          `${activeDrill.meaningVi}${activeDrill.phoneticHint ? ` (${activeDrill.phoneticHint})` : ""}`
                        )
                      }
                      className="h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-accent transition cursor-pointer"
                    >
                      + Lưu thành thẻ ghi nhớ
                    </button>
                  )}
                </div>

                {dictationChecked && (
                  <div className="rounded-lg border border-border bg-bg-secondary/40 p-3 space-y-2 text-xs">
                    <div className="flex items-center justify-between">
                      <span className="font-semibold text-text-primary">Đối chiếu từng từ:</span>
                      <span className="font-semibold text-accent">
                        Độ chính xác: {dictationEvaluation.accuracy}% ({dictationEvaluation.hits}/
                        {dictationEvaluation.total} từ)
                      </span>
                    </div>
                    <div className="flex flex-wrap gap-1.5 leading-relaxed">
                      {dictationEvaluation.tokens.map((tok, i) => (
                        <span
                          key={i}
                          className={`px-1.5 py-0.5 rounded font-medium ${
                            tok.status === "exact"
                              ? "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300"
                              : tok.status === "close"
                                ? "bg-amber-500/15 text-amber-700 dark:text-amber-300"
                                : "bg-rose-500/15 text-rose-700 dark:text-rose-300"
                          }`}
                        >
                          {tok.word}
                        </span>
                      ))}
                    </div>
                    {activeDrill.phoneticHint && (
                      <p className="text-text-tertiary font-mono text-[11px]">
                        Phiên âm: {activeDrill.phoneticHint}
                      </p>
                    )}
                    {activeDrill.meaningVi && (
                      <p className="text-text-secondary">Nghĩa: {activeDrill.meaningVi}</p>
                    )}
                  </div>
                )}
              </div>

              {/* Right: Speaking & Shadowing */}
              <div className="rounded-xl border border-border bg-bg-primary p-4 space-y-3.5">
                <div className="flex items-center justify-between gap-2">
                  <span className="text-xs font-semibold text-accent inline-flex items-center gap-1.5">
                    <Mic className="w-4 h-4" />
                    2. Luyện Nói (Shadowing &amp; IELTS Prompt)
                  </span>
                  <span className="text-xs font-mono px-2 py-0.5 rounded bg-bg-secondary border border-border text-text-secondary">
                    {Math.floor(speechSeconds / 60)}:
                    {String(speechSeconds % 60).padStart(2, "0")} / 2:00
                  </span>
                </div>

                <div className="rounded-lg border border-border bg-bg-secondary/50 p-3 space-y-1">
                  <p className="text-[11px] font-medium uppercase tracking-wider text-text-tertiary">
                    Đề bài nói mẫu
                  </p>
                  <p className="text-sm font-medium text-text-primary leading-relaxed">
                    {activeDrill.speakingPrompt}
                  </p>
                </div>

                <div className="flex flex-wrap items-center gap-2">
                  <button
                    type="button"
                    data-testid="practice-english-mic-btn"
                    onClick={() => void handleToggleMic()}
                    className={`inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md text-xs font-medium transition cursor-pointer ${
                      isListeningMic
                        ? "bg-rose-600 text-white"
                        : "bg-accent text-white hover:bg-accent-hover"
                    }`}
                  >
                    {isListeningMic ? (
                      <>
                        <Square className="w-3.5 h-3.5" />
                        <span>Dừng thu âm</span>
                      </>
                    ) : (
                      <>
                        <Mic className="w-3.5 h-3.5" />
                        <span>Bắt đầu nói (Mic)</span>
                      </>
                    )}
                  </button>

                  {activeDrill.sampleAnswer && (
                    <button
                      type="button"
                      onClick={() => handlePlayTts(activeDrill.sampleAnswer)}
                      className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
                    >
                      <Volume2 className="w-3.5 h-3.5" />
                      <span>Nghe câu mẫu Band 7.5+</span>
                    </button>
                  )}

                  {recordedAudioUrl && (
                    <audio controls src={recordedAudioUrl} className="h-8 max-w-[200px]" />
                  )}
                </div>

                <div>
                  <label className="block text-xs font-medium text-text-secondary mb-1">
                    Bản ghi âm thanh thành chữ (hoặc tự gõ câu trả lời tiếng Anh của bạn):
                  </label>
                  <textarea
                    rows={3}
                    value={spokenTranscript}
                    onChange={(e) => setSpokenTranscript(e.target.value)}
                    placeholder="Lời nói của bạn sẽ hiện ở đây khi bật Mic, hoặc gõ thử câu trả lời để AI chấm diễn đạt..."
                    className="w-full p-2.5 rounded-md border border-border bg-bg-secondary text-sm text-text-primary focus:outline-none focus:border-accent"
                  />
                </div>

                {speakingScore && (
                  <p className="text-xs text-text-secondary">
                    Khớp từ vựng mục tiêu (Shadowing):{" "}
                    <span className="font-semibold text-accent">{speakingScore.accuracy}%</span> (
                    {speakingScore.hits}/{speakingScore.total} từ)
                  </p>
                )}

                <div className="flex flex-wrap items-center gap-2">
                  <button
                    type="button"
                    disabled={speakingAiLoading}
                    onClick={() => void handleAiFeedbackEnglish()}
                    className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-md border border-accent/40 bg-accent-subtle text-accent text-xs font-medium hover:bg-accent hover:text-white transition cursor-pointer"
                  >
                    <Sparkles className="w-3.5 h-3.5" />
                    <span>{speakingAiLoading ? "Đang chấm..." : "AI nhận xét ngữ pháp & từ vựng"}</span>
                  </button>
                  <button
                    type="button"
                    onClick={() =>
                      void onSaveToNotebook(
                        `### 🎧 Thực hành Tiếng Anh: ${activeDrill.topic}\n- Câu mẫu: ${activeDrill.sentence}\n- Bài nói/viết của tôi: ${spokenTranscript || dictationInput}\n${speakingAiReview ? `- Nhận xét AI: ${speakingAiReview}` : ""}`
                      )
                    }
                    className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
                  >
                    <NotebookPen className="w-3.5 h-3.5" />
                    <span>Lưu vào Sổ tay</span>
                  </button>
                </div>

                {speakingAiReview && (
                  <div className="rounded-lg border border-border bg-bg-secondary/50 p-3 text-xs text-text-primary whitespace-pre-wrap leading-relaxed">
                    {speakingAiReview}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* ==================== MODE 3: CODE PRACTICE WORKBENCH ==================== */}
        {mode === "code" && (
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-2">
              {codeChallenges.map((ch, idx) => (
                <button
                  key={ch.id}
                  type="button"
                  onClick={() => setCodeIdx(idx)}
                  className={`px-3 py-1.5 rounded-md border text-xs font-medium transition cursor-pointer ${
                    idx === codeIdx
                      ? "border-accent bg-accent-subtle text-accent"
                      : "border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
                  }`}
                >
                  Bài tập {idx + 1}: {ch.title}
                </button>
              ))}
            </div>

            <div className="rounded-xl border border-border bg-bg-primary p-4 space-y-3.5">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <span className="text-[11px] font-mono uppercase px-2 py-0.5 rounded bg-bg-tertiary text-text-secondary">
                    {activeChallenge.lang}
                  </span>
                  <h3 className="text-sm font-semibold text-text-primary mt-1">
                    {activeChallenge.title}
                  </h3>
                  <p className="text-xs text-text-secondary mt-0.5">{activeChallenge.prompt}</p>
                </div>

                <button
                  type="button"
                  onClick={() => setShowSolution((s) => !s)}
                  className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
                >
                  <Eye className="w-3.5 h-3.5" />
                  <span>{showSolution ? "Ẩn đáp án mẫu" : "Xem đáp án mẫu"}</span>
                </button>
              </div>

              <textarea
                rows={10}
                value={userCode}
                onChange={(e) => setUserCode(e.target.value)}
                spellCheck={false}
                className="w-full p-3.5 rounded-lg border border-border bg-slate-950 text-slate-100 font-mono text-xs leading-relaxed focus:outline-none focus:border-accent"
              />

              <div className="flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  onClick={handleCheckCode}
                  className="inline-flex items-center gap-1.5 h-8 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
                >
                  <CheckCircle2 className="w-3.5 h-3.5" />
                  <span>Kiểm tra cấu trúc &amp; từ khóa</span>
                </button>
                <button
                  type="button"
                  onClick={() =>
                    void onSaveToNotebook(
                      `### 💻 Bài thực hành Code: ${activeChallenge.title}\n\`\`\`${activeChallenge.lang}\n${userCode}\n\`\`\``
                    )
                  }
                  className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary cursor-pointer"
                >
                  <NotebookPen className="w-3.5 h-3.5" />
                  <span>Lưu code vào Sổ tay</span>
                </button>
              </div>

              {codeCheckResult && (
                <div className="rounded-lg border border-border bg-bg-secondary/50 p-3 text-xs space-y-1.5">
                  <p className="font-semibold text-text-primary">
                    Đạt {codeCheckResult.matched.length}/
                    {codeCheckResult.matched.length + codeCheckResult.missing.length} thành phần cốt lõi
                  </p>
                  {codeCheckResult.missing.length > 0 ? (
                    <p className="text-amber-600 dark:text-amber-400">
                      Còn thiếu: {codeCheckResult.missing.join(", ")}
                    </p>
                  ) : (
                    <p className="text-emerald-600 dark:text-emerald-400">
                      Đủ toàn bộ từ khóa mục tiêu! Hãy đối chiếu edge-cases với đáp án mẫu.
                    </p>
                  )}
                </div>
              )}

              {showSolution && activeChallenge.solutionCode && (
                <div className="rounded-lg border border-border bg-bg-secondary p-3 space-y-1.5">
                  <p className="text-[11px] font-semibold text-text-tertiary uppercase">Lời giải tham khảo</p>
                  <pre className="p-3 rounded bg-slate-950 text-slate-100 font-mono text-xs overflow-x-auto">
                    <code>{activeChallenge.solutionCode}</code>
                  </pre>
                </div>
              )}
            </div>
          </div>
        )}

        {/* ==================== MODE 4: FEYNMAN ACTIVE RECALL & CASE ==================== */}
        {mode === "recall" && (
          <div className="rounded-xl border border-border bg-bg-primary p-5 space-y-4">
            <div className="space-y-1">
              <span className="text-xs font-semibold text-accent inline-flex items-center gap-1.5">
                <Award className="w-4 h-4" />
                Tự diễn đạt lại từ trang trắng (Phương pháp Feynman)
              </span>
              <p className="text-xs text-text-secondary leading-relaxed">
                {parsedCustom.customCases[0]
                  ? parsedCustom.customCases[0].scenario
                  : `Không nhìn lại bài giảng "${lessonTitle}", hãy giải thích ngắn gọn những ý quan trọng nhất như thể bạn đang giảng lại cho người khác.`}
              </p>
            </div>

            <textarea
              rows={7}
              value={recallText}
              onChange={(e) => {
                setRecallText(e.target.value);
                setRecallChecked(false);
              }}
              placeholder="Gõ phần tự tóm tắt, công thức hoặc lập luận tình huống của bạn tại đây..."
              className="w-full p-3.5 rounded-lg border border-border bg-bg-secondary text-sm text-text-primary leading-relaxed focus:outline-none focus:border-accent"
            />

            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                onClick={() => setRecallChecked(true)}
                className="inline-flex items-center gap-1.5 h-8 px-4 rounded-md bg-accent text-white text-xs font-medium hover:bg-accent-hover transition cursor-pointer"
              >
                <BookOpen className="w-3.5 h-3.5" />
                <span>Đối chiếu ý cốt lõi</span>
              </button>
              <button
                type="button"
                disabled={recallAiLoading}
                onClick={() => void handleRecallAiCheck()}
                className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:bg-bg-tertiary transition cursor-pointer"
              >
                <Sparkles className="w-3.5 h-3.5 text-accent" />
                <span>{recallAiLoading ? "Đang chấm..." : "AI góp ý lỗ hổng"}</span>
              </button>
              <button
                type="button"
                disabled={!recallText.trim()}
                onClick={() =>
                  void onSaveToNotebook(
                    `### 🧠 Tự diễn đạt (${lessonTitle})\n${recallText.trim()}\n${recallAiFeedback ? `\n> Góp ý AI: ${recallAiFeedback}` : ""}`
                  )
                }
                className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary disabled:opacity-50 cursor-pointer"
              >
                <NotebookPen className="w-3.5 h-3.5" />
                <span>Lưu vào Sổ tay</span>
              </button>
            </div>

            {recallChecked && (
              <div className="rounded-lg border border-border bg-bg-secondary/50 p-3.5 space-y-2 text-xs">
                <p className="font-semibold text-text-primary">
                  Độ bao phủ khái niệm: {recallCoverage.percent}% ({recallCoverage.covered.length}/
                  {keyTerms.length})
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {recallCoverage.covered.map((t) => (
                    <span
                      key={t}
                      className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-medium"
                    >
                      ✓ {t}
                    </span>
                  ))}
                  {recallCoverage.missed.map((t) => (
                    <span
                      key={t}
                      className="px-2 py-0.5 rounded bg-amber-500/15 text-amber-700 dark:text-amber-300 font-medium"
                    >
                      Cần bổ sung: {t}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {recallAiFeedback && (
              <div className="rounded-lg border border-border bg-bg-secondary/40 p-3 text-xs text-text-primary whitespace-pre-wrap leading-relaxed">
                {recallAiFeedback}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
