"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  AlertTriangle,
  BookOpen,
  Check,
  CheckCircle2,
  CheckSquare,
  ChevronRight,
  Copy,
  ExternalLink,
  FileText,
  Flame,
  Hash,
  Headphones,
  HelpCircle,
  Info,
  Lightbulb,
  Link2,
  Mic,
  MicOff,
  NotebookPen,
  Play,
  RotateCcw,
  Sparkles,
  Square,
  Volume2,
  VolumeX,
  X,
} from "lucide-react";
import {
  SEMANTIC_HIGHLIGHT_KEYS,
  type SemanticHighlightConfig,
} from "@/stores/useWorkspaceStore";
import {
  cleanStudyInlineArtifacts,
  stripYamlFrontmatter,
  stripDiacritics,
  slugifyHeadingText,
  resolveRelativeCoursePath,
  sanitizeSafeUrl,
  isDeadOrInternalMarkdown,
  isAudioPath,
  isMarkdownPath,
  isExternalUrl,
  buildDocumentAssetUrl,
  inferPrecedingWord,
  normalizeTitleForComparison,
} from "./markdown/markdownCleaner";
import { CodeBlockCard } from "./markdown/CodeBlockCard";
import { BlockMath, InlineMath } from "@/lib/math-markdown";
import {
  InlineSpeechPracticeBar,
  SpeechButton,
  type SpeechPracticeItem,
} from "./markdown/InlineSpeechPracticeBar";

export {
  cleanStudyInlineArtifacts,
  stripYamlFrontmatter,
  stripDiacritics,
  slugifyHeadingText,
  resolveRelativeCoursePath,
  sanitizeSafeUrl,
  CodeBlockCard,
  InlineSpeechPracticeBar,
  SpeechButton,
  type SpeechPracticeItem,
};

export interface MarkdownAnnotationItem {
  id: string;
  blockId?: string | null;
  exactQuote: string;
  prefixQuote?: string;
  suffixQuote?: string;
  colorKey: "yellow" | "green" | "blue" | "purple" | "rose";
  semanticLabel?: string;
  status: string;
  linkedFlashcardsCount?: number;
}

export interface LessonHeadingItem {
  id: string;
  level: number;
  text: string;
}

interface LessonMarkdownViewProps {
  markdown: string;
  lessonTitle?: string | null;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  cleanReadingMode?: boolean;
  hideInlineCitationBadges?: boolean;
  anchoredAnnotations: MarkdownAnnotationItem[];
  onSelectBlockText: (blockId: string, fullBlockText: string) => void;
  onHighlightClick: (exactQuote: string) => void;
  onInsertCitation: (ann: MarkdownAnnotationItem) => void;
  onOpenRelativeMarkdown?: (resolvedRelativePath: string, label: string) => void;
  onHeadingsExtracted?: (headings: LessonHeadingItem[]) => void;
  onOpenPractice?: () => void;
  onQuickSaveToNotebook?: (text: string) => Promise<void>;
  autoGenerateFsrsSection?: React.ReactNode;
}

interface AudioAttachment {
  src: string;
  title: string;
  fileName: string;
}

interface InlineToken {
  kind: "text" | "code" | "link" | "image" | "math";
  text: string;
  href?: string;
  alt?: string;
  bold?: boolean;
  italic?: boolean;
  strike?: boolean;
  isExternal?: boolean;
  audioMeta?: AudioAttachment;
  mdTargetPath?: string;
  mathDisplay?: boolean;
}

interface TableData {
  headers: string[];
  alignments: Array<"left" | "center" | "right">;
  rows: string[][];
}

interface DetailsSection {
  summary: string;
  body: string;
}

interface ListItemData {
  text: string;
  indent: number;
  checked?: boolean | null;
  details?: DetailsSection[];
}

type ParsedBlock =
  | { type: "heading"; level: 1 | 2 | 3 | 4; id: string; text: string; extraId?: string }
  | { type: "paragraph"; text: string; extraId?: string }
  | { type: "code"; lang: string; code: string }
  | {
      type: "blockquote";
      callout?:
        | "NOTE"
        | "TIP"
        | "IMPORTANT"
        | "WARNING"
        | "CAUTION"
        | "EXAMPLE"
        | "SUMMARY"
        | "FORMULA"
        | "CONCEPT";
      lines: string[];
    }
  | { type: "table"; table: TableData }
  | { type: "list"; ordered: boolean; items: ListItemData[] }
  | { type: "details"; summary: string; body: string }
  | { type: "math"; tex: string; displayMode?: boolean }
  | { type: "hr" };

function shouldHideTechnicalNoteBlock(block: ParsedBlock): boolean {
  if (block.type !== "blockquote") return false;
  const joined = block.lines.join(" ").toLowerCase();
  if (
    joined.includes("ghi chú hiển thị markdown") ||
    (joined.includes("<details>") && joined.includes("trình xem"))
  ) {
    return true;
  }
  return false;
}

function renderHighlightIcon(iconName: string, className = "w-3 h-3 inline-block") {
  switch (iconName) {
    case "Lightbulb":
      return <Lightbulb className={className} />;
    case "Code2":
      return <span className="font-mono text-[10px] font-bold">&lt;/&gt;</span>;
    case "HelpCircle":
      return <HelpCircle className={className} />;
    case "Link2":
      return <Link2 className={className} />;
    case "AlertTriangle":
      return <AlertTriangle className={className} />;
    default:
      return <Lightbulb className={className} />;
  }
}

function makeDeterministicBlockUuid(index: number): string {
  const hex = Math.max(1, index + 1).toString(16).padStart(12, "0");
  return `10000000-0000-4000-8000-${hex}`;
}

function parseStyledTextRun(
  segment: string,
  style: { bold?: boolean; italic?: boolean; strike?: boolean }
): InlineToken[] {
  if (!segment) return [];
  const tokens: InlineToken[] = [];
  // Match ***both***, **bold**, __bold__, ~~strike~~, *italic*, _italic_ (word-boundary safe), or bare https:// URLs
  const pattern =
    /(\*\*\*([^\n*]+?)\*\*\*|\*\*([^\n*]+?)\*\*|__([^\n_]+?)__|~~([^\n~]+?)~~|\*([^\s*][^\n*]*?[^\s*]|[^\s*])\*|(?<![a-zA-Z0-9_])_([^\s_][^\n_]*?[^\s_]|[^\s_])_(?![a-zA-Z0-9_])|https?:\/\/[^\s<>()]+[^\s<>().,;:!?"')\]])/g;

  let lastIdx = 0;
  let match: RegExpExecArray | null;
  while ((match = pattern.exec(segment)) !== null) {
    if (match.index > lastIdx) {
      tokens.push({
        kind: "text",
        text: segment.slice(lastIdx, match.index),
        ...style,
      });
    }

    const full = match[0];
    if (match[2] !== undefined) {
      tokens.push(...parseStyledTextRun(match[2], { ...style, bold: true, italic: true }));
    } else if (match[3] !== undefined) {
      tokens.push(...parseStyledTextRun(match[3], { ...style, bold: true }));
    } else if (match[4] !== undefined) {
      tokens.push(...parseStyledTextRun(match[4], { ...style, bold: true }));
    } else if (match[5] !== undefined) {
      tokens.push(...parseStyledTextRun(match[5], { ...style, strike: true }));
    } else if (match[6] !== undefined) {
      tokens.push(...parseStyledTextRun(match[6], { ...style, italic: true }));
    } else if (match[7] !== undefined) {
      tokens.push(...parseStyledTextRun(match[7], { ...style, italic: true }));
    } else if (/^https?:\/\//i.test(full)) {
      tokens.push({
        kind: "link",
        text: full,
        href: full,
        isExternal: true,
        ...style,
      });
    }

    lastIdx = pattern.lastIndex;
  }

  if (lastIdx < segment.length) {
    tokens.push({
      kind: "text",
      text: segment.slice(lastIdx),
      ...style,
    });
  }

  return tokens;
}

function tokenizeInlineMarkdown(
  rawText: string,
  sourceFolder?: string | null,
  sourceRelativePath?: string | null
): InlineToken[] {
  if (!rawText) return [];

  // Step 1: Protect inline code with unique Unicode private-use placeholders
  // so nested formatting like **Đọc `implementation_plan_new.md`** doesn't break bold tokens
  const codeItems: Array<{ code: string }> = [];
  const withCodePlaceholders = rawText.replace(/(`+)([\s\S]*?)\1/g, (_m, _ticks, inner) => {
    const idx = codeItems.length;
    codeItems.push({ code: inner.trim() });
    return `\uE000CODE_${idx}\uE000`;
  });

  // Step 1b: Protect block math $$...$$ and inline math $...$
  const mathItems: Array<{ tex: string; displayMode: boolean }> = [];
  let withMathPlaceholders = withCodePlaceholders.replace(/\$\$([\s\S]+?)\$\$/g, (_m, tex) => {
    const idx = mathItems.length;
    mathItems.push({ tex: tex.trim(), displayMode: true });
    return `\uE000MATH_${idx}\uE000`;
  });
  withMathPlaceholders = withMathPlaceholders.replace(
    /(?<!\\)\$(?!\s)([^$\r\n]+?)(?<![\s\\])\$/g,
    (full, tex) => {
      // Guard against false positive currency like $10 or $100
      if (/^\d+(?:,\d+)*(?:\.\d+)?$/.test(tex.trim())) {
        return full;
      }
      const idx = mathItems.length;
      mathItems.push({ tex: tex.trim(), displayMode: false });
      return `\uE000MATH_${idx}\uE000`;
    }
  );

  // Step 2: Protect images and links (excluding dead/internal md links which should become plain text)
  const linkItems: Array<InlineToken> = [];
  const linkRegex = /(!\[([^\]]*)\]\(([^)\s]+(?:\s+"[^"]*")?)\)|\[([^\]]+)\]\(([^)]+)\))/g;

  const withAllPlaceholders = withMathPlaceholders.replace(
    linkRegex,
    (full, _m1, imgAlt, imgUrl, linkText, linkHref) => {
      if (full.startsWith("![")) {
        const alt = imgAlt ?? "";
        const rawHref = (imgUrl ?? "").replace(/\s+"[^"]*"$/, "").trim();
        let resolvedUrl = rawHref;
        if (!isExternalUrl(rawHref) && !rawHref.startsWith("/api/") && sourceFolder) {
          const rel = resolveRelativeCoursePath(sourceRelativePath, rawHref);
          if (rel) resolvedUrl = buildDocumentAssetUrl(sourceFolder, rel);
        }
        const idx = linkItems.length;
        linkItems.push({
          kind: "image",
          text: alt,
          alt,
          href: resolvedUrl,
        });
        return `\uE000LINK_${idx}\uE000`;
      }

      const label = linkText ?? "";
      const rawHref = (linkHref ?? "").trim();
      const external = isExternalUrl(rawHref);

      // Dead or internal markdown links: render as regular text rather than dead link
      if (!external && isDeadOrInternalMarkdown(rawHref)) {
        return label.replace(/\uE000CODE_(\d+)\uE000/g, (_m: string, idxStr: string) => {
          const cIdx = parseInt(idxStr, 10);
          return codeItems[cIdx]?.code ?? "";
        });
      }

      if (!external && isAudioPath(rawHref) && sourceFolder) {
        const rel = resolveRelativeCoursePath(sourceRelativePath, rawHref);
        const assetUrl = rel ? buildDocumentAssetUrl(sourceFolder, rel) : rawHref;
        const fileName = rel.split("/").pop() || rawHref;
        const idx = linkItems.length;
        linkItems.push({
          kind: "link",
          text: label,
          href: assetUrl,
          isExternal: false,
          audioMeta: {
            src: assetUrl,
            title: label,
            fileName,
          },
        });
        return `\uE000LINK_${idx}\uE000`;
      }

      if (!external && isMarkdownPath(rawHref)) {
        const rel = resolveRelativeCoursePath(sourceRelativePath, rawHref);
        const idx = linkItems.length;
        linkItems.push({
          kind: "link",
          text: label,
          href: rawHref,
          isExternal: false,
          mdTargetPath: rel || rawHref,
        });
        return `\uE000LINK_${idx}\uE000`;
      }

      let resolvedHref = rawHref;
      if (!external && !rawHref.startsWith("#") && !rawHref.startsWith("/") && sourceFolder) {
        const rel = resolveRelativeCoursePath(sourceRelativePath, rawHref);
        if (rel) resolvedHref = buildDocumentAssetUrl(sourceFolder, rel);
      }
      const idx = linkItems.length;
      linkItems.push({
        kind: "link",
        text: label,
        href: resolvedHref,
        isExternal: external,
      });
      return `\uE000LINK_${idx}\uE000`;
    }
  );

  // Step 3: Run parseStyledTextRun on the placeholder string
  const styledTokens = parseStyledTextRun(withAllPlaceholders, {});

  // Step 4: Expand placeholders back into actual code / link tokens, preserving any enclosing styles (bold/italic)
  const finalTokens: InlineToken[] = [];

  for (const token of styledTokens) {
    if (token.kind !== "text") {
      finalTokens.push(token);
      continue;
    }

    const segments = token.text.split(/(\uE000(?:CODE|LINK|MATH)_\d+\uE000)/g);
    for (const seg of segments) {
      if (!seg) continue;
      const codeMatch = seg.match(/^\uE000CODE_(\d+)\uE000$/);
      if (codeMatch) {
        const cIdx = parseInt(codeMatch[1], 10);
        const item = codeItems[cIdx];
        if (item) {
          const codeVal = item.code.trim();

          // 1. If internal dev markdown (e.g. implementation_plan*.md, HANDOFF*.md, AGENTS.md, REVIEW_REPORT.md)
          // render as plain text rather than code
          if (isDeadOrInternalMarkdown(codeVal)) {
            finalTokens.push({
              kind: "text",
              text: codeVal,
              bold: token.bold,
              italic: token.italic,
            });
            continue;
          }

          // 2. If valid relative course markdown path (e.g. Phase-3/month1-study-guide.md, notes/lecture1.md)
          // convert to clickable reference link
          if (isMarkdownPath(codeVal)) {
            const rel = resolveRelativeCoursePath(sourceRelativePath, codeVal);
            finalTokens.push({
              kind: "link",
              text: codeVal,
              href: codeVal,
              isExternal: false,
              mdTargetPath: rel || codeVal,
              bold: token.bold,
              italic: token.italic,
            });
            continue;
          }

          finalTokens.push({
            kind: "code",
            text: item.code,
            bold: token.bold,
            italic: token.italic,
          });
          continue;
        }
      }

      const mathMatch = seg.match(/^\uE000MATH_(\d+)\uE000$/);
      if (mathMatch) {
        const mIdx = parseInt(mathMatch[1], 10);
        const item = mathItems[mIdx];
        if (item) {
          finalTokens.push({
            kind: "math",
            text: item.tex,
            mathDisplay: item.displayMode,
            bold: token.bold,
            italic: token.italic,
          });
          continue;
        }
      }

      const linkMatch = seg.match(/^\uE000LINK_(\d+)\uE000$/);
      if (linkMatch) {
        const lIdx = parseInt(linkMatch[1], 10);
        const item = linkItems[lIdx];
        if (item) {
          // Resolve any code placeholders inside link text
          const resolvedLinkText = item.text.replace(/\uE000CODE_(\d+)\uE000/g, (_m: string, idxStr: string) => {
            const cIdx = parseInt(idxStr, 10);
            return codeItems[cIdx]?.code ?? "";
          });
          finalTokens.push({
            ...item,
            text: resolvedLinkText,
            bold: token.bold || item.bold,
            italic: token.italic || item.italic,
          });
          continue;
        }
      }

      const resolvedSeg = seg.replace(/\uE000CODE_(\d+)\uE000/g, (_m: string, idxStr: string) => {
        const cIdx = parseInt(idxStr, 10);
        return codeItems[cIdx]?.code ?? "";
      });

      finalTokens.push({
        ...token,
        text: resolvedSeg,
      });
    }
  }

  return finalTokens;
}

function isTableSeparatorLine(line: string): boolean {
  const trimmed = line.trim();
  if (!trimmed.includes("|") && !trimmed.includes("-")) return false;
  const cells = trimmed.replace(/^\|/, "").replace(/\|$/, "").split("|");
  return cells.length >= 2 && cells.every((c) => /^:?-{2,}:?$/.test(c.trim()));
}

function splitTableRow(line: string): string[] {
  return line
    .trim()
    .replace(/^\|/, "")
    .replace(/\|$/, "")
    .split("|")
    .map((cell) => cell.trim());
}

function parseTableAlignments(line: string): Array<"left" | "center" | "right"> {
  return splitTableRow(line).map((cell) => {
    const left = cell.startsWith(":");
    const right = cell.endsWith(":");
    if (left && right) return "center";
    if (right) return "right";
    return "left";
  });
}

function consumeDetailsBlock(
  lines: string[],
  startIndex: number
): { summary: string; body: string; nextIndex: number } | null {
  const firstTrimmed = lines[startIndex]?.trim() || "";
  const openMatch = firstTrimmed.match(/^<details(?:\s+[^>]*)?>([\s\S]*)$/i);
  if (!openMatch) return null;

  const collected: string[] = [];
  const afterOpen = openMatch[1];
  const sameLineClose = afterOpen.match(/^([\s\S]*?)<\/details>/i);
  let nextIndex = startIndex + 1;

  if (sameLineClose) {
    if (sameLineClose[1].trim()) collected.push(sameLineClose[1]);
  } else {
    if (afterOpen.trim()) collected.push(afterOpen);
    while (nextIndex < lines.length) {
      const curLine = lines[nextIndex];
      const closeIdx = curLine.search(/<\/details>/i);
      if (closeIdx >= 0) {
        const beforeClose = curLine.slice(0, closeIdx);
        if (beforeClose.trim()) collected.push(beforeClose);
        nextIndex++;
        break;
      }
      collected.push(curLine);
      nextIndex++;
    }
  }

  let summary = "Xem chi tiết";
  const bodyLines: string[] = [];
  let inSummary = false;
  const summaryParts: string[] = [];

  for (const rawLine of collected) {
    const t = rawLine.trim();
    if (!inSummary) {
      const singleSum = t.match(/^<summary(?:\s+[^>]*)?>([\s\S]*?)<\/summary>(.*)$/i);
      if (singleSum) {
        summary = singleSum[1].trim() || summary;
        if (singleSum[2]?.trim()) bodyLines.push(singleSum[2].trim());
        continue;
      }
      const openSum = t.match(/^<summary(?:\s+[^>]*)?>(.*)$/i);
      if (openSum) {
        inSummary = true;
        if (openSum[1].trim()) summaryParts.push(openSum[1].trim());
        continue;
      }
      bodyLines.push(rawLine);
    } else {
      const closeIdx = t.search(/<\/summary>/i);
      if (closeIdx >= 0) {
        const part = t.slice(0, closeIdx).trim();
        if (part) summaryParts.push(part);
        const rest = t.slice(closeIdx + "</summary>".length).trim();
        summary = summaryParts.join(" ").trim() || summary;
        inSummary = false;
        if (rest) bodyLines.push(rest);
      } else if (t) {
        summaryParts.push(t);
      }
    }
  }

  const nonEmptyIndents = bodyLines
    .filter((l) => l.trim().length > 0)
    .map((l) => l.match(/^(\s*)/)?.[1].length ?? 0);
  const minIndent = nonEmptyIndents.length > 0 ? Math.min(...nonEmptyIndents) : 0;
  const normalizedBody = bodyLines
    .map((l) => (minIndent > 0 && l.length >= minIndent ? l.slice(minIndent) : l.trimEnd()))
    .join("\n")
    .trim();

  return {
    summary,
    body: normalizedBody,
    nextIndex
  };
}

function parseMarkdownBlocks(rawMarkdown: string): ParsedBlock[] {
  const cleanMd = stripYamlFrontmatter(rawMarkdown);
  const lines = cleanMd.split(/\r?\n/);
  const blocks: ParsedBlock[] = [];
  let i = 0;
  let headingCounter = 0;
  let pendingAnchorId: string | undefined;

  while (i < lines.length) {
    const line = lines[i];
    const trimmed = line.trim();

    if (!trimmed) {
      i++;
      continue;
    }

    // Standalone HTML anchor tag (<a id="..."></a>)
    const anchorMatch = trimmed.match(/^<a\s+(?:id|name)=["']([^"']+)["'][^>]*>\s*<\/a>$/i);
    if (anchorMatch) {
      pendingAnchorId = anchorMatch[1];
      i++;
      continue;
    }

    // Native HTML <details><summary>...</summary>...</details> accordion
    if (/^<details(?:\s+[^>]*)?>/i.test(trimmed)) {
      const details = consumeDetailsBlock(lines, i);
      if (details) {
        blocks.push({
          type: "details",
          summary: details.summary,
          body: details.body
        });
        i = details.nextIndex;
        continue;
      }
    }

    // Standalone block math ($$...$$ or multi-line $$\n...\n$$)
    if (trimmed.startsWith("$$")) {
      if (trimmed.length > 2 && trimmed.endsWith("$$")) {
        const tex = trimmed.slice(2, -2).trim();
        blocks.push({
          type: "math",
          tex,
          displayMode: true,
        });
        i++;
        continue;
      }
      const mathLines: string[] = [];
      const firstLineContent = trimmed.slice(2).trim();
      if (firstLineContent) mathLines.push(firstLineContent);
      i++;
      while (i < lines.length) {
        const closeTrim = lines[i].trim();
        if (closeTrim.endsWith("$$")) {
          const lastLine = closeTrim.slice(0, -2).trim();
          if (lastLine) mathLines.push(lastLine);
          i++;
          break;
        }
        mathLines.push(lines[i]);
        i++;
      }
      blocks.push({
        type: "math",
        tex: mathLines.join("\n").trim(),
        displayMode: true,
      });
      continue;
    }

    // Fenced code block (``` or ~~~)
    const fenceMatch = trimmed.match(/^(`{3,}|~{3,})(.*)$/);
    if (fenceMatch) {
      const fence = fenceMatch[1];
      const fenceChar = fence[0];
      const fenceLen = fence.length;
      const lang = fenceMatch[2].trim();
      const codeLines: string[] = [];
      i++;
      while (i < lines.length) {
        const closeTrim = lines[i].trim();
        if (
          closeTrim.startsWith(fenceChar.repeat(fenceLen)) &&
          closeTrim.replace(new RegExp(`\\${fenceChar}`, "g"), "").trim() === ""
        ) {
          i++;
          break;
        }
        codeLines.push(lines[i]);
        i++;
      }
      blocks.push({
        type: "code",
        lang: lang || "text",
        code: codeLines.join("\n")
      });
      continue;
    }

    // Horizontal rule
    if (/^(-{3,}|\*{3,}|_{3,})$/.test(trimmed)) {
      blocks.push({ type: "hr" });
      i++;
      continue;
    }

    // Heading (# .. ####)
    const headingMatch = trimmed.match(/^(#{1,4})\s+(.+)$/);
    if (headingMatch) {
      const level = headingMatch[1].length as 1 | 2 | 3 | 4;
      const text = headingMatch[2].replace(/\s+#+\s*$/, "").trim();
      const id = pendingAnchorId || slugifyHeadingText(text, headingCounter);
      headingCounter++;
      blocks.push({
        type: "heading",
        level,
        id,
        text,
        extraId: pendingAnchorId
      });
      pendingAnchorId = undefined;
      i++;
      continue;
    }

    // GFM Table
    if (
      line.includes("|") &&
      i + 1 < lines.length &&
      isTableSeparatorLine(lines[i + 1])
    ) {
      const headers = splitTableRow(line);
      const alignments = parseTableAlignments(lines[i + 1]);
      i += 2;
      const rows: string[][] = [];
      while (i < lines.length) {
        const rowLine = lines[i];
        if (!rowLine.trim() || !rowLine.includes("|")) break;
        const cells = splitTableRow(rowLine);
        while (cells.length < headers.length) cells.push("");
        rows.push(cells.slice(0, headers.length));
        i++;
      }
      blocks.push({
        type: "table",
        table: { headers, alignments, rows }
      });
      continue;
    }

    // Blockquote / GitHub Callout
    if (trimmed.startsWith(">")) {
      const quoteLines: string[] = [];
      while (i < lines.length && lines[i].trim().startsWith(">")) {
        quoteLines.push(lines[i].trim().replace(/^>\s?/, ""));
        i++;
      }
      let callout:
        | "NOTE"
        | "TIP"
        | "IMPORTANT"
        | "WARNING"
        | "CAUTION"
        | "EXAMPLE"
        | "SUMMARY"
        | "FORMULA"
        | "CONCEPT"
        | undefined;
      if (quoteLines.length > 0) {
        const calloutMatch = quoteLines[0].match(
          /^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION|EXAMPLE|SUMMARY|FORMULA|CONCEPT)\]\s*(.*)$/i
        );
        if (calloutMatch) {
          callout = calloutMatch[1].toUpperCase() as typeof callout;
          if (calloutMatch[2]) {
            quoteLines[0] = calloutMatch[2];
          } else {
            quoteLines.shift();
          }
        } else {
          // Automatic context detection from curriculum keywords
          const norm = stripDiacritics(quoteLines[0].toLowerCase().trim());
          if (
            /^(?:[⚠️🚨❌]|(?:\*\*|__)?(?:canh bao|bay|sai lam|warning|danger|caution|nguy hiem))/i.test(
              norm
            )
          ) {
            callout = "WARNING";
          } else if (
            /^(?:[💡✨]|(?:\*\*|__)?(?:meo|tip|ghi nho|kinh nghiem|khuyen dung|pro-tip))/i.test(
              norm
            )
          ) {
            callout = "TIP";
          } else if (
            /^(?:[🧪🔍]|(?:\*\*|__)?(?:vi du|thuc hanh|minh hoa|example|case study))/i.test(
              norm
            )
          ) {
            callout = "EXAMPLE";
          } else if (
            /^(?:[📐📊]|(?:\*\*|__)?(?:cong thuc|tinh toan|formula|toan hoc))/i.test(
              norm
            )
          ) {
            callout = "FORMULA";
          } else if (
            /^(?:[📋📑]|(?:\*\*|__)?(?:tom tat|tong ket|summary|takeaways|bai hoc rut ra))/i.test(
              norm
            )
          ) {
            callout = "SUMMARY";
          } else if (
            /^(?:[⚡🎯🔔]|(?:\*\*|__)?(?:quan trong|important|cot loi|bat buoc))/i.test(
              norm
            )
          ) {
            callout = "IMPORTANT";
          } else if (
            /^(?:[📌📝ℹ️]|(?:\*\*|__)?(?:luu y|ghi chu|note|chu y))/i.test(
              norm
            )
          ) {
            callout = "NOTE";
          } else if (
            /^(?:[🔑🧠🏷️]|(?:\*\*|__)?(?:khai niem|dinh nghia|tu khoa|concept|definition|keyword))/i.test(
              norm
            )
          ) {
            callout = "CONCEPT";
          }
        }
      }
      blocks.push({
        type: "blockquote",
        callout,
        lines: quoteLines
      });
      continue;
    }

    // Unordered or Ordered List
    const unorderedMatch = line.match(/^(\s*)([-*+])\s+(.*)$/);
    const orderedMatch = line.match(/^(\s*)(\d+)\.\s+(.*)$/);
    if (unorderedMatch || orderedMatch) {
      const ordered = Boolean(orderedMatch);
      const items: ListItemData[] = [];
      while (i < lines.length) {
        const cur = lines[i];
        const m = ordered
          ? cur.match(/^(\s*)(\d+)\.\s+(.*)$/)
          : cur.match(/^(\s*)([-*+])\s+(.*)$/);
        if (!m) break;
        const indent = Math.floor(m[1].length / 2);
        let content = m[3];
        let checked: boolean | null = null;
        if (!ordered) {
          const taskMatch = content.match(/^\[([ xX])\]\s+(.*)$/);
          if (taskMatch) {
            checked = taskMatch[1].toLowerCase() === "x";
            content = taskMatch[2];
          }
        }
        i++;

        const itemDetails: DetailsSection[] = [];
        // Append indented continuation lines & nested <details>
        while (i < lines.length) {
          const nextLine = lines[i];
          const nextTrim = nextLine.trim();

          if (/^<details(?:\s+[^>]*)?>/i.test(nextTrim)) {
            const det = consumeDetailsBlock(lines, i);
            if (det) {
              itemDetails.push({ summary: det.summary, body: det.body });
              i = det.nextIndex;
              continue;
            }
          }

          if (nextTrim === "") {
            // Look ahead past blank line(s) for nested indented <details> belonging to this item
            let peek = i;
            while (peek < lines.length && lines[peek].trim() === "") peek++;
            if (peek < lines.length && /^\s+<details(?:\s+[^>]*)?>/i.test(lines[peek])) {
              i = peek;
              continue;
            }
            break;
          }

          if (
            /^(\s*)([-*+]|\d+\.)\s+/.test(nextLine) ||
            nextTrim.startsWith("#") ||
            nextTrim.startsWith("```") ||
            nextTrim.startsWith(">") ||
            /^(-{3,}|\*{3,}|_{3,})$/.test(nextTrim)
          ) {
            break;
          }

          content += " " + nextTrim;
          i++;
        }

        items.push({
          text: content,
          indent,
          checked,
          details: itemDetails.length > 0 ? itemDetails : undefined
        });

        // Allow sparse lists (blank lines between consecutive items of the same list kind)
        if (i < lines.length && lines[i].trim() === "") {
          let peek = i;
          while (peek < lines.length && lines[peek].trim() === "") peek++;
          if (
            peek < lines.length &&
            (ordered
              ? /^(\s*)(\d+)\.\s+(.*)$/.test(lines[peek])
              : /^(\s*)([-*+])\s+(.*)$/.test(lines[peek]))
          ) {
            i = peek;
          }
        }
      }
      blocks.push({ type: "list", ordered, items });
      continue;
    }

    // Paragraph (collect contiguous lines until blank or structural block)
    const paraLines: string[] = [trimmed];
    i++;
    while (i < lines.length) {
      const next = lines[i];
      const nextTrim = next.trim();
      if (
        !nextTrim ||
        nextTrim.startsWith("#") ||
        nextTrim.startsWith("```") ||
        nextTrim.startsWith("~~~") ||
        nextTrim.startsWith(">") ||
        /^(-{3,}|\*{3,}|_{3,})$/.test(nextTrim) ||
        /^(\s*)([-*+]|\d+\.)\s+/.test(next) ||
        /^<a\s+(?:id|name)=/i.test(nextTrim) ||
        /^<details(?:\s+[^>]*)?>/i.test(nextTrim) ||
        (next.includes("|") && i + 1 < lines.length && isTableSeparatorLine(lines[i + 1]))
      ) {
        break;
      }
      paraLines.push(nextTrim);
      i++;
    }
    blocks.push({
      type: "paragraph",
      text: paraLines.join(" "),
      extraId: pendingAnchorId
    });
    pendingAnchorId = undefined;
  }

  return blocks;
}

function InlineAudioPracticePlayer({ src, title, fileName }: AudioAttachment) {
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [speed, setSpeed] = useState<number>(1);
  const [loadError, setLoadError] = useState(false);

  const handleSpeedChange = (nextSpeed: number) => {
    setSpeed(nextSpeed);
    if (audioRef.current) {
      audioRef.current.playbackRate = nextSpeed;
    }
  };

  return (
    <div
      data-testid="lesson-audio-player"
      className="my-3 rounded-2xl border border-indigo-200 dark:border-indigo-800/80 bg-gradient-to-br from-indigo-50/90 to-sky-50/70 dark:from-indigo-950/60 dark:to-slate-900 p-3.5 sm:p-4 space-y-2.5 shadow-xs font-sans"
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2 min-w-0">
          <div className="w-8 h-8 rounded-xl bg-indigo-600 text-white flex items-center justify-center shrink-0 shadow-xs">
            <Headphones className="w-4 h-4" />
          </div>
          <div className="min-w-0">
            <div className="text-xs font-extrabold text-indigo-950 dark:text-indigo-100 truncate">
              {title || "Audio luyện nghe"}
            </div>
            <div className="text-[11px] font-mono text-slate-500 dark:text-slate-400 truncate flex items-center gap-1">
              <Volume2 className="w-3 h-3 text-indigo-500 shrink-0" />
              <span>{fileName}</span>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-1 bg-white/80 dark:bg-slate-950/80 border border-indigo-200/80 dark:border-indigo-800 rounded-xl p-1">
          <span className="text-[10px] font-bold text-slate-500 dark:text-slate-400 px-1.5">
            Tốc độ:
          </span>
          {[0.75, 1, 1.25].map((rate) => (
            <button
              key={rate}
              type="button"
              data-testid={`audio-speed-${rate}`}
              onClick={() => handleSpeedChange(rate)}
              className={`px-2 py-0.5 rounded-lg text-[11px] font-extrabold cursor-pointer transition ${
                speed === rate
                  ? "bg-indigo-600 text-white shadow-2xs"
                  : "text-slate-600 dark:text-slate-300 hover:bg-indigo-50 dark:hover:bg-slate-800"
              }`}
            >
              {rate}x
            </button>
          ))}
        </div>
      </div>

      {loadError ? (
        <div className="text-xs text-rose-700 dark:text-rose-300 bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-800 rounded-xl px-3 py-2">
          Không thể phát tệp âm thanh ({fileName}). Vui lòng kiểm tra tệp tài nguyên trên máy chủ.
        </div>
      ) : (
        <audio
          ref={audioRef}
          controls
          preload="metadata"
          src={sanitizeSafeUrl(src)}
          onLoadedMetadata={() => {
            if (audioRef.current) {
              audioRef.current.playbackRate = speed;
            }
          }}
          onError={() => setLoadError(true)}
          className="w-full h-10 rounded-lg"
        />
      )}
    </div>
  );
}

function DetailsInteractiveAccordion({
  summary,
  body,
  keyId,
  blockId,
  sourceFolder,
  sourceRelativePath,
  cleanReadingMode,
  renderInlineTokens,
  onOpenPractice,
  onQuickSaveToNotebook,
}: {
  summary: string;
  body: string;
  keyId: string;
  blockId: string;
  sourceFolder?: string | null;
  sourceRelativePath?: string | null;
  cleanReadingMode: boolean;
  renderInlineTokens: (tokens: InlineToken[], id: string) => React.ReactNode;
  onOpenPractice?: () => void;
  onQuickSaveToNotebook?: (text: string) => Promise<void>;
}) {
  const cleanSummary = cleanReadingMode ? cleanStudyInlineArtifacts(summary) : summary;
  const [userAnswer, setUserAnswer] = useState("");
  const [isSaved, setIsSaved] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const isQuestionOrExercise = useMemo(() => {
    const rawSum = (cleanSummary || "").toLowerCase();
    const norm = normalizeTitleForComparison(cleanSummary || "");

    // Loại trừ các summary đáp án của phần Quiz trắc nghiệm cuối bài
    const isQuizSummary =
      /quiz/i.test(rawSum) ||
      /(dap an.*giai thich|giai thich chi tiet|dap an quiz|trac nghiem)/i.test(norm);
    if (isQuizSummary) return false;

    // Nhận diện câu hỏi, bài tập, mini-review, lời giải / gợi ý thực hành
    return /(cau hoi|mini-review|mini review|tinh huong|case|bai tap|kiem tra|dot 1|dot 2|self-check|self check|reflection|tu kiem tra|luyen tap|xem loi giai|loi giai|goi y|dap an|solution|hint|answer)/i.test(
      norm
    );
  }, [cleanSummary]);

  const handleSaveToNotebook = async () => {
    if (!userAnswer.trim() || !onQuickSaveToNotebook || isSaving) return;
    setIsSaving(true);
    try {
      await onQuickSaveToNotebook(`### ✍️ Câu trả lời: ${cleanSummary}\n${userAnswer.trim()}`);
      setIsSaved(true);
      setTimeout(() => setIsSaved(false), 3500);
    } finally {
      setIsSaving(false);
    }
  };

  const subBlocks = useMemo(() => parseMarkdownBlocks(body), [body]);

  return (
    <div key={keyId} className="my-3 space-y-2">
      {isQuestionOrExercise && (
        <div className="p-3.5 rounded-xl bg-white dark:bg-slate-900 border border-indigo-200/80 dark:border-indigo-800/60 shadow-xs space-y-2 font-sans">
          <div className="flex items-center justify-between gap-2">
            <span className="text-[11px] font-semibold text-indigo-700 dark:text-indigo-300 flex items-center gap-1.5">
              <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
              <span>✍️ Hộp tự thực hành / viết nháp trước khi xem lời giải</span>
            </span>
            {isSaved && (
              <span className="text-[11px] text-emerald-600 dark:text-emerald-400 font-medium flex items-center gap-1">
                <Check className="w-3 h-3" />
                <span>Đã lưu vào Sổ tay</span>
              </span>
            )}
          </div>
          <textarea
            rows={2}
            value={userAnswer}
            onChange={(e) => setUserAnswer(e.target.value)}
            placeholder="✍️ Tự viết code, ý tưởng hoặc câu trả lời của bạn trước khi bấm mở xem lời giải..."
            className="w-full p-2.5 rounded-md border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 text-xs text-slate-800 dark:text-slate-100 placeholder:text-slate-400 focus:outline-none focus:border-indigo-500 leading-relaxed resize-y"
          />
          <div className="flex items-center justify-between gap-2 flex-wrap">
            <span className="text-[10px] text-slate-500 dark:text-slate-400">
              Tự thử sức viết code hoặc phác thảo lời giải trước khi đối chiếu với gợi ý bên dưới.
            </span>
            {onQuickSaveToNotebook && (
              <button
                type="button"
                disabled={!userAnswer.trim() || isSaving}
                onClick={handleSaveToNotebook}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md bg-indigo-600 hover:bg-indigo-700 disabled:opacity-40 text-white text-[11px] font-medium transition cursor-pointer"
              >
                <NotebookPen className="w-3 h-3" />
                <span>{isSaving ? "Đang lưu..." : "Lưu vào Sổ tay"}</span>
              </button>
            )}
          </div>
        </div>
      )}

      <details
        data-testid="lesson-details-accordion"
        className="group rounded-xl border border-indigo-200/80 dark:border-indigo-800/70 bg-indigo-50/40 dark:bg-indigo-950/25 overflow-hidden font-sans"
      >
        <summary className="flex items-center gap-2 px-3.5 py-2 text-xs sm:text-[13px] font-bold text-indigo-900 dark:text-indigo-200 cursor-pointer select-none list-none [&::-webkit-details-marker]:hidden hover:bg-indigo-100/50 dark:hover:bg-indigo-900/35 transition">
          <ChevronRight className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400 shrink-0 transition-transform duration-150 group-open:rotate-90" />
          <span className="flex-1 min-w-0">
            {renderInlineTokens(
              tokenizeInlineMarkdown(cleanSummary || "Xem gợi ý", sourceFolder, sourceRelativePath),
              `${blockId}-sum`
            )}
          </span>
        </summary>
        <div className="px-3.5 pb-3 pt-2 text-sm text-slate-700 dark:text-slate-200 border-t border-indigo-100/80 dark:border-indigo-900/50 space-y-2 leading-relaxed">
          {subBlocks.length === 0 ? (
            <p className="text-xs text-slate-500">Không có nội dung chi tiết.</p>
          ) : (
            subBlocks.map((sb, sbIdx) => {
              const sbKey = `${keyId}-b-${sbIdx}`;
              if (sb.type === "code") {
                return (
                  <CodeBlockCard
                    key={sbKey}
                    lang={sb.lang}
                    code={sb.code}
                    onOpenPractice={onOpenPractice}
                  />
                );
              }
              if (sb.type === "math") {
                return (
                  <div key={sbKey} className="my-2 overflow-x-auto text-center py-1">
                    <BlockMath math={sb.tex} />
                  </div>
                );
              }
              if (sb.type === "list") {
                const SubListTag = sb.ordered ? "ol" : "ul";
                return (
                  <SubListTag
                    key={sbKey}
                    className={`pl-5 space-y-1 ${sb.ordered ? "list-decimal" : "list-disc"}`}
                  >
                    {sb.items.map((it, itIdx) => (
                      <li key={itIdx}>
                        {renderInlineTokens(
                          tokenizeInlineMarkdown(
                            cleanReadingMode ? cleanStudyInlineArtifacts(it.text) : it.text,
                            sourceFolder,
                            sourceRelativePath
                          ),
                          `${sbKey}-${itIdx}`
                        )}
                      </li>
                    ))}
                  </SubListTag>
                );
              }
              const rawParagraph =
                sb.type === "paragraph"
                  ? sb.text
                  : sb.type === "blockquote"
                  ? sb.lines.join(" ")
                  : sb.type === "heading"
                  ? sb.text
                  : "";
              return (
                <p key={sbKey}>
                  {renderInlineTokens(
                    tokenizeInlineMarkdown(
                      cleanReadingMode ? cleanStudyInlineArtifacts(rawParagraph) : rawParagraph,
                      sourceFolder,
                      sourceRelativePath
                    ),
                    sbKey
                  )}
                </p>
              );
            })
          )}
        </div>
      </details>
    </div>
  );
}

const EXCLUDED_PATH_WORDS = new Set([
  "docs",
  "notes",
  "assessments",
  "quality",
  "flashcards",
  "module",
  "phase",
  "http",
  "https",
  "api",
  "src",
  "app",
]);

export function LessonMarkdownView({
  markdown,
  lessonTitle,
  sourceFolder,
  sourceRelativePath,
  cleanReadingMode = true,
  hideInlineCitationBadges = false,
  anchoredAnnotations,
  onSelectBlockText,
  onHighlightClick,
  onInsertCitation,
  onOpenRelativeMarkdown,
  onHeadingsExtracted,
  onOpenPractice,
  onQuickSaveToNotebook,
  autoGenerateFsrsSection,
}: LessonMarkdownViewProps) {
  const [copiedHeadingId, setCopiedHeadingId] = useState<string | null>(null);
  const copyTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [lightboxImage, setLightboxImage] = useState<{ src: string; alt: string } | null>(null);

  useEffect(() => {
    return () => {
      if (copyTimerRef.current) clearTimeout(copyTimerRef.current);
    };
  }, []);

  useEffect(() => {
    if (!lightboxImage) return;
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setLightboxImage(null);
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => {
      document.body.style.overflow = prevOverflow;
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [lightboxImage]);

  const handleCopyHeadingAnchor = async (headingId: string) => {
    const el = document.getElementById(headingId);
    if (el) {
      el.scrollIntoView({ behavior: "smooth", block: "start" });
    }
    const url = `${window.location.origin}${window.location.pathname}${window.location.search}#${headingId}`;
    try {
      await navigator.clipboard.writeText(url);
    } catch {
      // Clipboard blocked in restricted context; ignore gracefully
    }
    setCopiedHeadingId(headingId);
    if (copyTimerRef.current) clearTimeout(copyTimerRef.current);
    copyTimerRef.current = setTimeout(() => {
      setCopiedHeadingId((curr) => (curr === headingId ? null : curr));
    }, 1500);
  };

  const isEnglishContext = useMemo(() => {
    if (sourceFolder?.trim().toUpperCase() === "IELTS") return true;
    const combined = `${sourceFolder || ""} ${lessonTitle || ""} ${sourceRelativePath || ""}`.toLowerCase();
    return (
      combined.includes("ielts") ||
      combined.includes("english") ||
      combined.includes("ipa") ||
      combined.includes("phát âm") ||
      combined.includes("phat am") ||
      combined.includes("từ vựng") ||
      combined.includes("tu vung") ||
      combined.includes("listening") ||
      combined.includes("speaking") ||
      combined.includes("vocabulary") ||
      combined.includes("phase-0") ||
      combined.includes("phase-1")
    );
  }, [sourceFolder, lessonTitle, sourceRelativePath]);

  const cleanText = (text: string) => {
    const cleaned = cleanReadingMode ? cleanStudyInlineArtifacts(text) : text;
    return cleaned ? cleaned.replace(/[\uE000-\uF8FF\u001A]/g, "") : "";
  };

  const blocks = useMemo(() => parseMarkdownBlocks(markdown || ""), [markdown]);

  const indexedBlocks = useMemo(() => {
    let mapped = blocks.map((block, bIdx) => ({ block, bIdx }));
    // Robust H1 deduplication (Bug B5): Check if any of the first 3 blocks is an H1 matching lesson title
    if (lessonTitle && mapped.length > 0) {
      const titleNorm = normalizeTitleForComparison(lessonTitle);
      const h1Idx = mapped.findIndex(
        (m, idx) => idx <= 2 && m.block.type === "heading" && m.block.level === 1
      );
      if (h1Idx !== -1) {
        const h1Block = mapped[h1Idx].block;
        if (h1Block.type === "heading") {
          const h1Norm = normalizeTitleForComparison(h1Block.text);
          if (
            h1Norm &&
            titleNorm &&
            (h1Norm === titleNorm || h1Norm.includes(titleNorm) || titleNorm.includes(h1Norm) || h1Idx === 0)
          ) {
            mapped.splice(h1Idx, 1);
          }
        }
      }
    }
    if (cleanReadingMode) {
      mapped = mapped.filter(({ block }) => !shouldHideTechnicalNoteBlock(block));
    }
    return mapped;
  }, [blocks, lessonTitle, cleanReadingMode]);

  const headings = useMemo<LessonHeadingItem[]>(() => {
    return indexedBlocks
      .map((entry) => entry.block)
      .filter((b): b is Extract<ParsedBlock, { type: "heading" }> => b.type === "heading")
      .map((h) => ({
        id: h.id,
        level: h.level,
        text: cleanReadingMode ? cleanStudyInlineArtifacts(h.text) : h.text
      }));
  }, [indexedBlocks, cleanReadingMode]);

  const lastHeadingSigRef = useRef<string>("");
  useEffect(() => {
    if (!onHeadingsExtracted) return;
    const sig = headings.map((h) => `${h.id}:${h.level}:${h.text}`).join("|");
    if (sig !== lastHeadingSigRef.current) {
      lastHeadingSigRef.current = sig;
      onHeadingsExtracted(headings);
    }
  }, [headings, onHeadingsExtracted]);

  const renderInlineTokens = (
    tokens: InlineToken[],
    blockId: string,
    matchedOut?: MarkdownAnnotationItem[]
  ): React.ReactNode => {
    const plainText = tokens.map((t) => (t.kind === "image" ? "" : t.text)).join("");

    // Locate anchored annotations that match this block
    const blockAnns = anchoredAnnotations.filter((a) => {
      if (a.blockId && a.blockId === blockId) return true;
      if (!a.exactQuote) return false;
      const idx = plainText.indexOf(a.exactQuote);
      if (idx < 0) return false;
      // If annotation has prefix/suffix quote and belongs to a different block UUID, verify context if possible
      if (a.prefixQuote && a.prefixQuote.length >= 6) {
        const localPrefix = plainText.slice(Math.max(0, idx - a.prefixQuote.length), idx);
        if (localPrefix.length >= 4 && !a.prefixQuote.includes(localPrefix) && !localPrefix.includes(a.prefixQuote.slice(-4))) {
          return !a.blockId;
        }
      }
      return true;
    });

    const ranges: Array<{
      ann: MarkdownAnnotationItem;
      start: number;
      end: number;
    }> = [];
    for (const ann of blockAnns) {
      if (!ann.exactQuote) continue;
      const idx = plainText.indexOf(ann.exactQuote);
      if (idx >= 0) {
        ranges.push({ ann, start: idx, end: idx + ann.exactQuote.length });
        if (matchedOut && !matchedOut.some((m) => m.id === ann.id)) {
          matchedOut.push(ann);
        }
      }
    }
    ranges.sort((a, b) => a.start - b.start);

    // Filter overlapping ranges
    const nonOverlapping: typeof ranges = [];
    let cursor = 0;
    for (const r of ranges) {
      if (r.start >= cursor && r.end > r.start) {
        nonOverlapping.push(r);
        cursor = r.end;
      }
    }

    const renderSingleTokenPiece = (
      token: InlineToken,
      rawPieceText: string,
      key: string
    ): React.ReactNode => {
      const pieceText = (rawPieceText || "").replace(/[\uE000-\uF8FF\u001A]/g, "");
      if (token.kind === "image") {
        const safeSrc = sanitizeSafeUrl(token.href);
        const altText = token.alt || "";
        return (
          <button
            key={key}
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              if (safeSrc) {
                setLightboxImage({ src: safeSrc, alt: altText });
              }
            }}
            aria-label={altText ? `Phóng to ảnh: ${altText}` : "Phóng to ảnh"}
            className="group/img inline-block my-2 max-w-full align-middle cursor-zoom-in text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 rounded-xl"
          >
            <img
              src={safeSrc}
              alt={altText}
              className="max-w-full rounded-xl border border-slate-200 dark:border-slate-800 shadow-xs transition group-hover/img:brightness-95"
              loading="lazy"
            />
          </button>
        );
      }

      if (token.kind === "math") {
        if (token.mathDisplay) {
          return <BlockMath key={key} math={pieceText} />;
        }
        return <InlineMath key={key} math={pieceText} />;
      }

      if (token.kind === "code") {
        return (
          <code
            key={key}
            className="px-1.5 py-0.5 mx-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-indigo-700 dark:text-indigo-300 font-mono text-[0.86em] break-words"
          >
            {pieceText}
          </code>
        );
      }

      if (token.kind === "link") {
        if (token.mdTargetPath && onOpenRelativeMarkdown) {
          return (
            <button
              key={key}
              type="button"
              data-testid="lesson-relative-md-link"
              onClick={(e) => {
                e.stopPropagation();
                onOpenRelativeMarkdown(token.mdTargetPath!, (token.text || "").replace(/[\uE000-\uF8FF\u001A]/g, ""));
              }}
              className="inline-flex items-center gap-1 font-semibold text-indigo-600 dark:text-indigo-400 hover:underline underline-offset-2 cursor-pointer text-left"
            >
              <FileText className="w-3.5 h-3.5 shrink-0 inline" />
              <span>{pieceText}</span>
            </button>
          );
        }

        if (token.audioMeta) {
          return (
            <a
              key={key}
              href={sanitizeSafeUrl(token.href)}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1 font-semibold text-indigo-600 dark:text-indigo-400 hover:underline underline-offset-2"
            >
              <Headphones className="w-3.5 h-3.5 shrink-0 inline" />
              <span>{pieceText}</span>
            </a>
          );
        }

        if (token.href?.startsWith("#")) {
          return (
            <a
              key={key}
              href={sanitizeSafeUrl(token.href)}
              className="font-semibold text-indigo-600 dark:text-indigo-400 hover:underline underline-offset-2"
            >
              {pieceText}
            </a>
          );
        }

        return (
          <a
            key={key}
            href={sanitizeSafeUrl(token.href)}
            target={token.isExternal ? "_blank" : undefined}
            rel={token.isExternal ? "noopener noreferrer" : undefined}
            className="inline-flex items-baseline gap-1 font-semibold text-indigo-600 dark:text-indigo-400 hover:underline underline-offset-2 break-all"
          >
            <span>{pieceText}</span>
            {token.isExternal && (
              <ExternalLink className="w-3 h-3 shrink-0 inline self-center" />
            )}
          </a>
        );
      }

      let content: React.ReactNode = pieceText;
      // If pieceText contains IPA phonetic pattern like /.../ (e.g. /kənˈsɪstənt/)
      if (
        token.kind === "text" &&
        /\/([ˈˌːæɑɔəɛɜɪʊʌθðʃʒʧʤŋa-zA-Z\s.-]{2,35})\//.test(pieceText) &&
        /[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/.test(pieceText)
      ) {
        const parts = pieceText.split(/(\/[ˈˌːæɑɔəɛɜɪʊʌθðʃʒʧʤŋa-zA-Z\s.-]{2,35}\/)/g);
        content = parts.map((part, pIdx) => {
          if (/^\/([ˈˌːæɑɔəɛɜɪʊʌθðʃʒʧʤŋa-zA-Z\s.-]{2,35})\/$/.test(part) && /[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/.test(part)) {
            const wordToSpeak = inferPrecedingWord(plainText, part);
            return (
              <span key={`ipa-${pIdx}`} className="inline-flex items-center align-middle mx-0.5">
                <span className="font-mono text-indigo-600 dark:text-indigo-400 font-medium px-1.5 py-0.5 bg-indigo-50/80 dark:bg-indigo-950/60 rounded-md text-[0.88em]">
                  {part}
                </span>
                <SpeechButton textToSpeak={wordToSpeak} />
              </span>
            );
          }
          return part;
        });
      }

      if (token.bold) {
        content = <strong className="font-bold text-slate-900 dark:text-white">{content}</strong>;
      }
      if (token.italic) {
        content = <em className="italic">{content}</em>;
      }
      if (token.strike) {
        content = <del className="line-through opacity-75">{content}</del>;
      }
      return <React.Fragment key={key}>{content}</React.Fragment>;
    };

    if (nonOverlapping.length === 0) {
      return tokens.map((tok, tIdx) => renderSingleTokenPiece(tok, tok.text, `tok-${tIdx}`));
    }

    // Split tokens across annotation [start, end) ranges so highlights wrap smoothly across bold/italic spans
    const output: React.ReactNode[] = [];
    let globalOffset = 0;
    let rangeIdx = 0;
    let activeMarkNodes: React.ReactNode[] = [];
    let currentRange = nonOverlapping[rangeIdx] ?? null;

    const flushActiveMark = (ann: MarkdownAnnotationItem, keySuffix: string) => {
      const colorCfg: SemanticHighlightConfig =
        SEMANTIC_HIGHLIGHT_KEYS.find((c) => c.key === ann.colorKey) ??
        SEMANTIC_HIGHLIGHT_KEYS[0];
      output.push(
        <mark
          key={`mark-${ann.id}-${keySuffix}`}
          id={`reader-highlight-${ann.id}`}
          data-annotation-id={ann.id}
          className={`${colorCfg.cssClass} inline px-1 py-0.5 rounded-xs font-medium cursor-pointer transition`}
          title={colorCfg.label}
          onClick={(e) => {
            e.stopPropagation();
            onHighlightClick(ann.exactQuote);
          }}
        >
          <span className="inline-block mr-1 select-none align-text-bottom">
            {renderHighlightIcon(colorCfg.icon, "w-3 h-3 inline-block")}
          </span>
          {activeMarkNodes}
        </mark>
      );
      activeMarkNodes = [];
    };

    tokens.forEach((tok, tIdx) => {
      if (tok.kind === "image") {
        output.push(renderSingleTokenPiece(tok, tok.text, `img-${tIdx}`));
        return;
      }
      const tokLen = tok.text.length;
      const tokStart = globalOffset;
      const tokEnd = tokStart + tokLen;
      let posInTok = 0;

      while (posInTok < tokLen) {
        const absPos = tokStart + posInTok;
        if (!currentRange) {
          output.push(
            renderSingleTokenPiece(tok, tok.text.slice(posInTok), `t-${tIdx}-${posInTok}`)
          );
          posInTok = tokLen;
          continue;
        }

        if (absPos < currentRange.start) {
          const sliceEnd = Math.min(tokLen, currentRange.start - tokStart);
          output.push(
            renderSingleTokenPiece(tok, tok.text.slice(posInTok, sliceEnd), `t-${tIdx}-${posInTok}`)
          );
          posInTok = sliceEnd;
        } else if (absPos < currentRange.end) {
          const sliceEnd = Math.min(tokLen, currentRange.end - tokStart);
          activeMarkNodes.push(
            renderSingleTokenPiece(tok, tok.text.slice(posInTok, sliceEnd), `m-${tIdx}-${posInTok}`)
          );
          posInTok = sliceEnd;
          if (tokStart + posInTok >= currentRange.end) {
            flushActiveMark(currentRange.ann, `${tIdx}-${posInTok}`);
            rangeIdx++;
            currentRange = nonOverlapping[rangeIdx] ?? null;
          }
        } else {
          if (activeMarkNodes.length > 0) {
            flushActiveMark(currentRange.ann, `${tIdx}-end`);
          }
          rangeIdx++;
          currentRange = nonOverlapping[rangeIdx] ?? null;
        }
      }

      globalOffset = tokEnd;
    });

    if (activeMarkNodes.length > 0 && currentRange) {
      flushActiveMark(currentRange.ann, "tail");
    }

    return <>{output}</>;
  };

  const renderCitationBadges = (matchedAnns: MarkdownAnnotationItem[]) => {
    if (hideInlineCitationBadges || matchedAnns.length === 0) return null;

    // Deduplicate annotations targeting the same quote within one paragraph so repeated pills don't clutter
    const dedupMap = new Map<string, { primary: MarkdownAnnotationItem; totalCards: number }>();
    for (const ann of matchedAnns) {
      const quoteKey = (ann.exactQuote || ann.id).trim().toLowerCase();
      const existing = dedupMap.get(quoteKey);
      if (!existing) {
        dedupMap.set(quoteKey, {
          primary: ann,
          totalCards: ann.linkedFlashcardsCount || 1
        });
      } else {
        existing.totalCards = Math.max(existing.totalCards, ann.linkedFlashcardsCount || 1);
      }
    }
    const entries = Array.from(dedupMap.values());

    return (
      <div className="mt-2 flex flex-wrap items-center gap-2 font-sans text-[11px]">
        {entries.map(({ primary: ann, totalCards }) => (
          <div key={ann.id} className="inline-flex items-center gap-1.5">
            <span className="px-2 py-0.5 rounded-md bg-slate-100 dark:bg-slate-800 font-semibold text-slate-700 dark:text-slate-300 inline-flex items-center gap-1">
              <span>⚡ {totalCards} thẻ FSRS</span>
            </span>
            <button
              type="button"
              onClick={() => onInsertCitation(ann)}
              className="px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/70 text-indigo-700 dark:text-indigo-300 font-semibold hover:bg-indigo-600 hover:text-white transition cursor-pointer inline-flex items-center gap-1"
            >
              <FileText className="w-3 h-3" />
              <span>+ Trích dẫn sang ghi chú</span>
            </button>
          </div>
        ))}
      </div>
    );
  };

  const extractSpeechItemsFromText = (text: string): SpeechPracticeItem[] => {
    if (!text) return [];
    if (
      !/minimal pairs|đọc to|bài tập trọng tâm|tự test|phát âm|pronunciation|\/[^/\n]*[əɪʊʌæɑɔɜːθðʃʒŋˈˌ][^/\n]*\//i.test(
        text
      )
    ) {
      return [];
    }
    const items: SpeechPracticeItem[] = [];
    const hasEnglishSpeechClue = isEnglishContext || /minimal pairs|phát âm|phat am/i.test(text);

    if (hasEnglishSpeechClue) {
      const pairRegex = /\b([a-zA-Z]+)\/([a-zA-Z]+)\b/g;
      let pm: RegExpExecArray | null;
      while ((pm = pairRegex.exec(text)) !== null) {
        const w1 = pm[1].trim();
        const w2 = pm[2].trim();
        if (
          !EXCLUDED_PATH_WORDS.has(w1.toLowerCase()) &&
          !EXCLUDED_PATH_WORDS.has(w2.toLowerCase())
        ) {
          items.push({ wordOrSentence: `${w1} vs ${w2}` });
          if (items.length >= 8) break;
        }
      }
    }

    if (isEnglishContext || /(?:đọc to|doc to)\s+câu/i.test(text)) {
      const quoteRegex = /"([a-zA-Z0-9\s,;:'"–—-]+)"/g;
      let qm: RegExpExecArray | null;
      while ((qm = quoteRegex.exec(text)) !== null) {
        const qText = qm[1].trim();
        if (
          qText.length >= 3 &&
          qText.length <= 80 &&
          !EXCLUDED_PATH_WORDS.has(qText.toLowerCase()) &&
          !items.some((it) => it.wordOrSentence === qText)
        ) {
          items.push({ wordOrSentence: qText });
        }
        if (items.length >= 8) break;
      }
    }

    if (hasEnglishSpeechClue) {
      const listMatch = text.match(/(?:đọc|doc)\s+\d+\s+từ(?:[^\n:]*):([^\n—–.]+)/i);
      if (listMatch) {
        const wordList = listMatch[1]
          .split(/[,;·]+/)
          .map((w) => w.trim())
          .filter(
            (w) =>
              w.length >= 2 &&
              !w.includes(" ") &&
              !EXCLUDED_PATH_WORDS.has(w.toLowerCase())
          );
        for (const w of wordList) {
          if (!items.some((it) => it.wordOrSentence === w)) {
            items.push({ wordOrSentence: w });
          }
          if (items.length >= 12) break;
        }
      }
    }

    const ipaRegex = /\/([^/\n]*[əɪʊʌæɑɔɜːθðʃʒŋˈˌ][^/\n]*)\//g;
    let im: RegExpExecArray | null;
    while ((im = ipaRegex.exec(text)) !== null) {
      const ipaStr = `/${im[1].trim()}/`;
      const pre = text.slice(Math.max(0, im.index - 30), im.index).trim();
      const lastWord = pre.split(/\s+/).pop()?.replace(/[*_`]/g, "") || "";
      if (lastWord && !EXCLUDED_PATH_WORDS.has(lastWord.toLowerCase())) {
        if (!items.some((it) => it.wordOrSentence === lastWord)) {
          items.push({ wordOrSentence: lastWord, ipa: ipaStr });
        }
      }
      if (items.length >= 10) break;
    }

    return items.slice(0, 10);
  };

  const renderDetailsAccordion = (
    summary: string,
    body: string,
    keyId: string,
    blockId: string
  ) => {
    return (
      <DetailsInteractiveAccordion
        key={keyId}
        summary={summary}
        body={body}
        keyId={keyId}
        blockId={blockId}
        sourceFolder={sourceFolder}
        sourceRelativePath={sourceRelativePath}
        cleanReadingMode={cleanReadingMode}
        renderInlineTokens={renderInlineTokens}
        onOpenPractice={onOpenPractice}
        onQuickSaveToNotebook={onQuickSaveToNotebook}
      />
    );
  };

  const renderSingleBlock = (block: ParsedBlock, bIdx: number): React.ReactNode => {
    const blockId = makeDeterministicBlockUuid(bIdx);

    if (block.type === "hr") {
      return <hr key={blockId} className="my-6 border-slate-200 dark:border-slate-800" />;
    }

    if (block.type === "code") {
      return <CodeBlockCard key={blockId} lang={block.lang} code={block.code} onOpenPractice={onOpenPractice} />;
    }

    if (block.type === "math") {
      return (
        <div key={blockId} className="my-4 overflow-x-auto text-center py-2">
          <BlockMath math={block.tex} />
        </div>
      );
    }

    if (block.type === "details") {
      return renderDetailsAccordion(block.summary, block.body, blockId, blockId);
    }

        if (block.type === "heading") {
          const displayHeading = cleanText(block.text);
          if (!displayHeading) return null;
          const tokens = tokenizeInlineMarkdown(displayHeading, sourceFolder, sourceRelativePath);
          const plain = tokens.map((t) => t.text).join("");
          const HeadingTag = (`h${block.level}` as unknown) as keyof React.JSX.IntrinsicElements;
          const sizeClass =
            block.level === 1
              ? "text-xl sm:text-2xl font-extrabold pt-3 pb-1 border-b border-slate-100 dark:border-slate-800"
              : block.level === 2
              ? "text-lg sm:text-xl font-bold pt-4"
              : block.level === 3
              ? "text-base sm:text-lg font-bold pt-3"
              : "text-sm sm:text-base font-bold pt-2";

          const isAnchorCopied = copiedHeadingId === block.id;

          return (
            <HeadingTag
              key={blockId}
              id={block.id}
              onMouseUp={() => onSelectBlockText(blockId, plain)}
              className={`group ${sizeClass} text-slate-900 dark:text-white font-sans scroll-mt-20 break-words`}
            >
              {renderInlineTokens(tokens, blockId)}
              {(block.level === 2 || block.level === 3) && (
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleCopyHeadingAnchor(block.id);
                  }}
                  aria-label={`Sao chép liên kết mục ${plain}`}
                  title="Sao chép liên kết mục này"
                  className={`inline-flex items-center justify-center ml-2 align-middle p-1 rounded-md text-slate-400 hover:text-indigo-600 dark:text-slate-500 dark:hover:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/60 transition cursor-pointer ${
                    isAnchorCopied
                      ? "opacity-100 text-emerald-600 dark:text-emerald-400"
                      : "opacity-50 sm:opacity-0 sm:group-hover:opacity-100 focus:opacity-100 focus-visible:opacity-100"
                  }`}
                >
                  {isAnchorCopied ? (
                    <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                  ) : (
                    <Hash className="w-3.5 h-3.5" />
                  )}
                </button>
              )}
            </HeadingTag>
          );
        }

        if (block.type === "table") {
          const { headers, alignments, rows } = block.table;
          const filteredRows = rows.filter((row) =>
            !row.every((cell) => cleanText(cell).replace(/[|\-\s]+/g, "") === "")
          );
          const hasNonEmptyHeader = headers.some(
            (h) => cleanText(h).replace(/[|\-\s]+/g, "") !== ""
          );
          if (filteredRows.length === 0 && !hasNonEmptyHeader) {
            return null;
          }
          const speechItemsFromTable: SpeechPracticeItem[] = [];
          const ipaColIdx = headers.findIndex((h) => /ipa|phiên âm|phat am/i.test(h));
          const wordColIdx = headers.findIndex((h) => /từ|word|chữ|kí hiệu|ky hieu|ví dụ|sentence|câu/i.test(h));
          const meanColIdx = headers.findIndex((h) => /nghĩa|meaning|ví dụ|example|dịch/i.test(h));
          const hasEnglishTableClue =
            isEnglishContext ||
            ipaColIdx >= 0 ||
            headers.some((h) => /ipa|phiên âm|phat am/i.test(h));

          if (hasEnglishTableClue) {
            for (const row of rows) {
              let word = "";
              let ipa = "";
              let meaning = "";

              for (let c = 0; c < row.length; c++) {
                const cellText = row[c] || "";
                const ipaMatch = cellText.match(/\/([^\/\n]{1,40})\//);
                if (ipaMatch && /[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/.test(ipaMatch[1])) {
                  ipa = `/${ipaMatch[1]}/`;
                  const boldMatch = cellText.match(/\*\*([^*]+)\*\*/);
                  if (boldMatch) word = boldMatch[1].trim();
                }
              }

              if (!word && wordColIdx >= 0 && row[wordColIdx]) {
                word = row[wordColIdx].replace(/[*_`]/g, "").trim();
              } else if (!word && row[0]) {
                word = row[0].replace(/[*_`]/g, "").trim();
              }

              if (!ipa && ipaColIdx >= 0 && row[ipaColIdx]) {
                const rawIpa = row[ipaColIdx].replace(/[*_`]/g, "").trim();
                if (/[əɪʊʌæɑɔɜːθðʃʒŋˈˌ]/.test(rawIpa)) {
                  ipa = rawIpa;
                }
              }

              if (meanColIdx >= 0 && row[meanColIdx]) {
                meaning = row[meanColIdx].replace(/[*_`]/g, "").trim();
              }

              if (
                word &&
                !EXCLUDED_PATH_WORDS.has(word.toLowerCase()) &&
                (ipa || (isEnglishContext && word.split(/\s+/).length <= 6))
              ) {
                if (/^[A-Za-z0-9\s'–—\-./,()]+$/.test(word) && word.length >= 1 && word.length <= 60) {
                  speechItemsFromTable.push({ wordOrSentence: word, ipa, meaning });
                }
              }
            }
          }

          return (
            <div key={blockId} className="my-4 space-y-2">
              <div className="w-full max-w-full overflow-x-auto rounded-[12px] border border-slate-200 dark:border-slate-800 shadow-xs font-sans">
                <table className="w-full border-collapse text-left text-xs sm:text-sm">
                  <thead className="bg-slate-100/90 dark:bg-slate-800/90 border-b border-slate-200 dark:border-slate-700">
                    <tr>
                      {headers.map((header, hIdx) => {
                        const tokens = tokenizeInlineMarkdown(cleanText(header), sourceFolder, sourceRelativePath);
                        const align = alignments[hIdx] || "left";
                        const alignCls =
                          align === "center"
                            ? "text-center"
                            : align === "right"
                            ? "text-right"
                            : "text-left";
                        return (
                          <th
                            key={hIdx}
                            scope="col"
                            className={`px-4 py-3 font-bold text-slate-900 dark:text-slate-100 whitespace-nowrap ${alignCls}`}
                          >
                            {renderInlineTokens(tokens, `${blockId}-th-${hIdx}`)}
                          </th>
                        );
                      })}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 dark:divide-slate-800/70">
                    {filteredRows.map((row, rIdx) => (
                      <tr
                        key={rIdx}
                        className="odd:bg-white even:bg-slate-50/70 dark:odd:bg-slate-950 dark:even:bg-slate-900/60 hover:bg-indigo-50/40 dark:hover:bg-indigo-950/30 transition-colors"
                      >
                        {row.map((cell, cIdx) => {
                          const tokens = tokenizeInlineMarkdown(cleanText(cell), sourceFolder, sourceRelativePath);
                          const plain = tokens.map((t) => t.text).join("");
                          const align = alignments[cIdx] || "left";
                          const alignCls =
                            align === "center"
                              ? "text-center"
                              : align === "right"
                              ? "text-right"
                              : "text-left";
                          return (
                            <td
                              key={cIdx}
                              onMouseUp={() => onSelectBlockText(blockId, plain)}
                              className={`px-4 py-3 text-slate-700 dark:text-slate-300 align-top leading-relaxed ${alignCls}`}
                            >
                              {renderInlineTokens(tokens, blockId)}
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {speechItemsFromTable.length > 0 && (
                <InlineSpeechPracticeBar items={speechItemsFromTable} />
              )}
            </div>
          );
        }

        if (block.type === "blockquote") {
          const calloutStyles: Record<
            string,
            { container: string; titleColor: string; label: string; icon: React.ReactNode }
          > = {
            NOTE: {
              container: "border-l-sky-500 bg-sky-50/80 dark:bg-sky-950/30 border-sky-200 dark:border-sky-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-sky-800 dark:text-sky-300",
              label: "Ghi chú & Lưu ý",
              icon: <Info className="w-4 h-4 text-sky-600 dark:text-sky-400" />
            },
            TIP: {
              container: "border-l-amber-500 bg-amber-50/80 dark:bg-amber-950/30 border-amber-200 dark:border-amber-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-amber-800 dark:text-amber-300",
              label: "Mẹo thực hành & Ghi nhớ",
              icon: <Lightbulb className="w-4 h-4 text-amber-600 dark:text-amber-400" />
            },
            IMPORTANT: {
              container: "border-l-indigo-500 bg-indigo-50/80 dark:bg-indigo-950/30 border-indigo-200 dark:border-indigo-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-indigo-800 dark:text-indigo-300",
              label: "Điểm quan trọng",
              icon: <Info className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            },
            WARNING: {
              container: "border-l-rose-500 bg-rose-50/80 dark:bg-rose-950/30 border-rose-200 dark:border-rose-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-rose-800 dark:text-rose-300",
              label: "Cảnh báo & Sai lầm cần tránh",
              icon: <AlertTriangle className="w-4 h-4 text-rose-600 dark:text-rose-400" />
            },
            CAUTION: {
              container: "border-l-rose-500 bg-rose-50/80 dark:bg-rose-950/30 border-rose-200 dark:border-rose-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-rose-800 dark:text-rose-300",
              label: "Lưu ý nguy hiểm",
              icon: <Flame className="w-4 h-4 text-rose-600 dark:text-rose-400" />
            },
            EXAMPLE: {
              container: "border-l-emerald-500 bg-emerald-50/80 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-emerald-800 dark:text-emerald-300",
              label: "Ví dụ minh họa & Thực tế",
              icon: <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
            },
            FORMULA: {
              container: "border-l-purple-500 bg-purple-50/80 dark:bg-purple-950/30 border-purple-200 dark:border-purple-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-purple-800 dark:text-purple-300",
              label: "Công thức & Cách tính",
              icon: <Sparkles className="w-4 h-4 text-purple-600 dark:text-purple-400" />
            },
            SUMMARY: {
              container: "border-l-indigo-500 bg-indigo-50/80 dark:bg-indigo-950/30 border-indigo-200 dark:border-indigo-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-indigo-800 dark:text-indigo-300",
              label: "Tóm tắt & Điểm cốt lõi",
              icon: <BookOpen className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            },
            CONCEPT: {
              container: "border-l-teal-500 bg-teal-50/80 dark:bg-teal-950/30 border-teal-200 dark:border-teal-900/60 text-slate-800 dark:text-slate-200",
              titleColor: "text-teal-800 dark:text-teal-300",
              label: "Từ khóa & Khái niệm then chốt",
              icon: <Sparkles className="w-4 h-4 text-teal-600 dark:text-teal-400" />
            }
          };

          const cfg = block.callout ? calloutStyles[block.callout] : null;
          const fullQuoteText = cleanText(block.lines.join(" "));
          if (!fullQuoteText) return null;
          const tokens = tokenizeInlineMarkdown(fullQuoteText, sourceFolder, sourceRelativePath);
          const plain = tokens.map((t) => t.text).join("");
          const audioLinks = tokens
            .filter((t) => Boolean(t.audioMeta))
            .map((t) => t.audioMeta!);
          const speechItems = extractSpeechItemsFromText(fullQuoteText);

          return (
            <div key={blockId} className="space-y-2">
              <blockquote
                onMouseUp={() => onSelectBlockText(blockId, plain)}
                className={`my-3 rounded-xl border border-l-4 px-4 py-3 text-sm leading-relaxed ${
                  cfg
                    ? cfg.container
                    : "border-l-indigo-500 border-slate-200 dark:border-slate-800 bg-slate-50/90 dark:bg-slate-950/50 text-slate-700 dark:text-slate-300"
                }`}
              >
                {cfg && (
                  <div className={`flex items-center gap-1.5 text-xs font-extrabold uppercase tracking-wider mb-1 font-sans ${cfg.titleColor}`}>
                    {cfg.icon}
                    <span>{cfg.label}</span>
                  </div>
                )}
                <div>{renderInlineTokens(tokens, blockId)}</div>
              </blockquote>
              {audioLinks.map((au, idx) => (
                <InlineAudioPracticePlayer key={`au-bq-${idx}`} {...au} />
              ))}
              {speechItems.length > 0 && (
                <InlineSpeechPracticeBar items={speechItems} />
              )}
            </div>
          );
        }

        if (block.type === "list") {
          const ListTag = block.ordered ? "ol" : "ul";
          const hasTasks = block.items.some((it) => typeof it.checked === "boolean");
          const audioAttachments: AudioAttachment[] = [];

          return (
            <div key={blockId} className="space-y-2">
              <ListTag
                className={`my-2 space-y-1.5 pl-5 ${
                  hasTasks
                    ? "list-none pl-1"
                    : block.ordered
                    ? "list-decimal"
                    : "list-disc"
                }`}
              >
                {block.items.map((item, idx) => {
                  const displayItem = cleanText(item.text);
                  const tokens = tokenizeInlineMarkdown(
                    displayItem,
                    sourceFolder,
                    sourceRelativePath
                  );
                  const plain = tokens.map((t) => t.text).join("");
                  for (const t of tokens) {
                    if (t.audioMeta) audioAttachments.push(t.audioMeta);
                  }
                  const indentPx = Math.min(3, item.indent) * 16;

                  return (
                    <li
                      key={idx}
                      style={indentPx > 0 ? { marginLeft: `${indentPx}px` } : undefined}
                      onMouseUp={() => onSelectBlockText(blockId, plain)}
                      className="leading-[1.75] text-slate-800 dark:text-slate-200 break-words"
                    >
                      {typeof item.checked === "boolean" ? (
                        <span className="inline-flex items-start gap-2">
                          {item.checked ? (
                            <CheckSquare className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0 mt-1" />
                          ) : (
                            <Square className="w-4 h-4 text-slate-400 shrink-0 mt-1" />
                          )}
                          <span className={item.checked ? "text-slate-700 dark:text-slate-300" : ""}>
                            {renderInlineTokens(tokens, blockId)}
                          </span>
                        </span>
                      ) : (
                        renderInlineTokens(tokens, blockId)
                      )}
                      {item.details?.map((det, dIdx) =>
                        renderDetailsAccordion(
                          det.summary,
                          det.body,
                          `${blockId}-li-${idx}-det-${dIdx}`,
                          `${blockId}-li-${idx}`
                        )
                      )}
                    </li>
                  );
                })}
              </ListTag>
              {audioAttachments.map((au, idx) => (
                <InlineAudioPracticePlayer key={`au-list-${idx}`} {...au} />
              ))}
            </div>
          );
        }

        // Paragraph block
        const displayParagraph = cleanText(block.text);
        if (!displayParagraph) return null;
        const tokens = tokenizeInlineMarkdown(displayParagraph, sourceFolder, sourceRelativePath);
        const plain = tokens.map((t) => (t.kind === "image" ? "" : t.text)).join("");
        const audioLinks = tokens
          .filter((t) => Boolean(t.audioMeta))
          .map((t) => t.audioMeta!);
        const matchedAnns: MarkdownAnnotationItem[] = [];
        const speechItems = extractSpeechItemsFromText(displayParagraph);

        return (
          <div
            key={blockId}
            id={block.extraId || `block-${blockId}`}
            onMouseUp={() => onSelectBlockText(blockId, plain)}
            className="group relative leading-[1.8] space-y-2"
          >
            <p className="break-words">
              {renderInlineTokens(tokens, blockId, matchedAnns)}
            </p>
            {audioLinks.map((au, idx) => (
              <InlineAudioPracticePlayer key={`au-p-${idx}`} {...au} />
            ))}
            {renderCitationBadges(matchedAnns)}
            {speechItems.length > 0 && (
              <InlineSpeechPracticeBar items={speechItems} />
            )}
          </div>
        );
      };

  return (
    <div data-testid="lesson-markdown-view" className="space-y-4 text-slate-800 dark:text-slate-200 min-w-0 max-w-full">
      {(() => {
        const result: React.ReactNode[] = [];
        let k = 0;
        while (k < indexedBlocks.length) {
          const entry = indexedBlocks[k];
          const { block, bIdx } = entry;

          if (
            block.type === "heading" &&
            block.level === 2 &&
            /quiz\s*(kiểm\s*tra|ôn\s*tập|kiem\s*tra|on\s*tap)?/i.test(
              normalizeTitleForComparison(block.text) || block.text
            )
          ) {
            let nextH2 = k + 1;
            while (nextH2 < indexedBlocks.length) {
              const nextB = indexedBlocks[nextH2].block;
              if (nextB.type === "heading" && nextB.level <= 2) {
                break;
              }
              nextH2++;
            }
            const quizSlice = indexedBlocks.slice(k, nextH2);
            result.push(
              <details
                key={`quiz-accordion-${bIdx}`}
                data-testid="lesson-quiz-static-accordion"
                className="group my-5 rounded-xl border border-indigo-200/80 dark:border-indigo-800/70 bg-indigo-50/30 dark:bg-indigo-950/20 overflow-hidden font-sans"
              >
                <summary className="flex items-center gap-2 px-4 py-3 text-xs sm:text-sm font-bold text-indigo-900 dark:text-indigo-200 cursor-pointer select-none list-none [&::-webkit-details-marker]:hidden hover:bg-indigo-100/50 dark:hover:bg-indigo-900/35 transition">
                  <ChevronRight className="w-4 h-4 text-indigo-600 dark:text-indigo-400 shrink-0 transition-transform duration-150 group-open:rotate-90" />
                  <span className="flex-1 min-w-0">📝 Xem bản văn bản câu hỏi ôn tập cuối bài</span>
                  <span className="text-[11px] font-normal text-indigo-600 dark:text-indigo-400 hidden sm:inline">
                    (Đã thu gọn để nhường chỗ cho Quick-Check tương tác)
                  </span>
                </summary>
                <div className="px-4 py-3 border-t border-indigo-100 dark:border-indigo-900/40 space-y-4">
                  {quizSlice.map((qb) => renderSingleBlock(qb.block, qb.bIdx))}
                </div>
              </details>
            );
            k = nextH2;
            continue;
          }

          result.push(renderSingleBlock(block, bIdx));
          k++;
        }
        return result;
      })()}

      {autoGenerateFsrsSection}

      {/* Khối gợi ý thực hành cuối bài */}
      <div className="mt-8 rounded-xl border border-indigo-200/70 dark:border-indigo-900/60 bg-gradient-to-r from-indigo-50/70 via-sky-50/50 to-indigo-50/70 dark:from-indigo-950/40 dark:via-slate-900 dark:to-indigo-950/40 p-4 flex flex-wrap items-center justify-between gap-3 text-xs">
        <div className="flex items-center gap-2.5 min-w-0">
          <div className="w-8 h-8 rounded-lg bg-indigo-600 text-white flex items-center justify-center shrink-0 shadow-xs">
            <Sparkles className="w-4 h-4" />
          </div>
          <div>
            <p className="font-semibold text-slate-900 dark:text-slate-100">
              Sẵn sàng kiểm tra kiến thức bài học?
            </p>
            <p className="text-slate-600 dark:text-slate-400 text-[11px] mt-0.5">
              Bấm <strong>Mở Phòng Thực Hành</strong> để chuyển sang không gian luyện tập toàn màn hình với đầy đủ bài tập tương tác, sửa lỗi code và luyện nói.
            </p>
          </div>
        </div>
        {onOpenPractice && (
          <button
            type="button"
            data-testid="lesson-end-open-practice-btn"
            onClick={onOpenPractice}
            className="inline-flex items-center gap-1.5 min-h-[40px] px-4 py-2 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:hover:bg-[#A5B4FC] dark:text-[#0F0F12] font-semibold text-xs shadow-xs transition-colors cursor-pointer shrink-0"
          >
            <span>Mở Phòng Thực Hành →</span>
          </button>
        )}
      </div>

      {lightboxImage && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label="Xem ảnh phóng to"
          onClick={() => setLightboxImage(null)}
          className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-slate-950/85 backdrop-blur-xs p-4"
        >
          <button
            type="button"
            onClick={() => setLightboxImage(null)}
            aria-label="Đóng ảnh phóng to"
            className="absolute top-4 right-4 p-2 rounded-full bg-slate-900/80 hover:bg-slate-800 text-white border border-slate-700/80 shadow-md transition cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
          <div
            onClick={(e) => e.stopPropagation()}
            className="flex flex-col items-center max-w-full max-h-full"
          >
            <img
              src={lightboxImage.src}
              alt={lightboxImage.alt}
              className="max-w-[92vw] max-h-[85vh] object-contain rounded-xl shadow-2xl border border-white/10"
            />
            {lightboxImage.alt && (
              <p className="mt-3 text-xs sm:text-sm text-slate-200 text-center max-w-[85vw] px-3 py-1.5 rounded-lg bg-slate-900/75">
                {lightboxImage.alt}
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
