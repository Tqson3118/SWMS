"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  AlertTriangle,
  Check,
  CheckSquare,
  ChevronRight,
  Copy,
  ExternalLink,
  FileText,
  Flame,
  Headphones,
  HelpCircle,
  Info,
  Lightbulb,
  Link2,
  Square,
  Volume2
} from "lucide-react";
import {
  SEMANTIC_HIGHLIGHT_KEYS,
  type SemanticHighlightConfig
} from "@/stores/useWorkspaceStore";

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
}

interface AudioAttachment {
  src: string;
  title: string;
  fileName: string;
}

interface InlineToken {
  kind: "text" | "code" | "link" | "image";
  text: string;
  href?: string;
  alt?: string;
  bold?: boolean;
  italic?: boolean;
  strike?: boolean;
  isExternal?: boolean;
  audioMeta?: AudioAttachment;
  mdTargetPath?: string;
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
  | { type: "blockquote"; callout?: "NOTE" | "TIP" | "IMPORTANT" | "WARNING" | "CAUTION"; lines: string[] }
  | { type: "table"; table: TableData }
  | { type: "list"; ordered: boolean; items: ListItemData[] }
  | { type: "details"; summary: string; body: string }
  | { type: "hr" };

export function cleanStudyInlineArtifacts(raw: string): string {
  if (!raw) return raw;
  return raw
    // Hide Learning Objective metadata prefixes like `[LO-W01-01] (Nhớ/Hiểu):` while keeping the bolded verb intact
    .replace(/\[LO-[A-Z0-9-]+\]\s*(?:\([^)]+\)\s*:?\s*)?/gi, "")
    // Hide evidence claim references like [C01-01], [C00-01a, C01-01], [C03-08] (preserve Markdown links `[label](href)`)
    .replace(/\s*\[C\d{1,3}-\d{1,3}[a-z]?(?:\s*,\s*C\d{1,3}-\d{1,3}[a-z]?)*\](?!\()/gi, "")
    // Hide evidence strength / audit badges like 🟢 [Mạnh], 🟡 [Khá], 🔴 [Tranh luận]
    .replace(/\s*(?:🟢|🟡|🔴|⚪)\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo|Strong|Moderate|Weak|Controversial|Verified)[^\]]*\](?!\()/gi, "")
    .replace(/\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo)\](?!\()/gi, "")
    // Hide developer internal work tasks / AI executor rows
    .replace(/^.*\(AI executor\).*$/gim, "")
    .replace(/^.*HANDOFF.*$/gim, "")
    // Clean orphaned bold markers / whitespace before punctuation
    .replace(/\*\*\s*\*\*/g, "")
    .replace(/\s+([.,;:!?])/g, "$1")
    .replace(/\(\s*\)/g, "")
    .replace(/ {2,}/g, " ")
    .trim();
}

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

export function stripYamlFrontmatter(raw: string): string {
  const normalized = raw.replace(/^\uFEFF/, "");
  const match = normalized.match(/^---\r?\n[\s\S]*?\r?\n---(?:\r?\n|$)/);
  if (match) {
    return normalized.slice(match[0].length).trimStart();
  }
  return normalized;
}

export function slugifyHeadingText(text: string, idx: number): string {
  const base = text
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-")
    .slice(0, 48);
  return base ? `sec-${base}-${idx}` : `sec-heading-${idx}`;
}

export function resolveRelativeCoursePath(
  sourceRelativePath: string | null | undefined,
  href: string
): string {
  const cleanHref = href.split("#")[0].split("?")[0].replace(/\\/g, "/").trim();
  if (!cleanHref) return "";
  if (cleanHref.startsWith("/")) {
    return cleanHref.replace(/^\/+/, "");
  }
  const normalizedSource = (sourceRelativePath || "").replace(/\\/g, "/");
  const lastSlash = normalizedSource.lastIndexOf("/");
  const baseDir = lastSlash >= 0 ? normalizedSource.slice(0, lastSlash) : "";
  const parts = baseDir ? baseDir.split("/").filter(Boolean) : [];
  for (const seg of cleanHref.split("/")) {
    if (!seg || seg === ".") continue;
    if (seg === "..") {
      if (parts.length > 0) parts.pop();
    } else {
      parts.push(seg);
    }
  }
  return parts.join("/");
}

function buildDocumentAssetUrl(sourceFolder: string, resolvedPath: string): string {
  const encodedSegments = resolvedPath
    .split("/")
    .filter(Boolean)
    .map((s) => encodeURIComponent(s))
    .join("/");
  return `/api/v1/document-assets/${encodeURIComponent(sourceFolder)}/${encodedSegments}`;
}

function isAudioPath(href: string): boolean {
  const clean = href.split("#")[0].split("?")[0].trim().toLowerCase();
  return /\.(wav|mp3|ogg|m4a)$/.test(clean);
}

function isMarkdownPath(href: string): boolean {
  const clean = href.split("#")[0].split("?")[0].trim().toLowerCase();
  return /\.md$/.test(clean);
}

export function sanitizeSafeUrl(url: string | undefined | null): string {
  if (!url) return "#";
  const raw = url.trim();
  if (!raw) return "#";

  let normalized = raw.replace(/[\u0000-\u001F\u007F\s]+/g, "");
  try {
    const decoded = decodeURIComponent(normalized);
    normalized = decoded.replace(/[\u0000-\u001F\u007F\s]+/g, "");
  } catch {
    // Keep normalized string if malformed URI escape
  }

  const lower = normalized.toLowerCase();
  if (
    lower.startsWith("javascript:") ||
    lower.startsWith("data:") ||
    lower.startsWith("vbscript:")
  ) {
    return "#";
  }

  return raw;
}

function isExternalUrl(href: string): boolean {
  return /^https?:\/\//i.test(href.trim()) || /^mailto:/i.test(href.trim());
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
        ...style
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
        ...style
      });
    }

    lastIdx = pattern.lastIndex;
  }

  if (lastIdx < segment.length) {
    tokens.push({
      kind: "text",
      text: segment.slice(lastIdx),
      ...style
    });
  }

  return tokens;
}

function isDeadOrInternalMarkdown(path: string): boolean {
  const norm = path.toLowerCase().replace(/\\/g, "/").split("/").pop() || "";
  return /^(handoff.*\.md|implementation_plan.*\.md|roadmap.*\.md|agents.*\.md|review_report.*\.md)$/i.test(norm);
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

  // Step 2: Protect images and links (excluding dead/internal md links which should become plain text)
  const linkItems: Array<InlineToken> = [];
  const linkRegex = /(!\[([^\]]*)\]\(([^)\s]+(?:\s+"[^"]*")?)\)|\[([^\]]+)\]\(([^)]+)\))/g;

  const withAllPlaceholders = withCodePlaceholders.replace(
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
        return label;
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

    const segments = token.text.split(/(\uE000(?:CODE|LINK)_\d+\uE000)/g);
    for (const seg of segments) {
      if (!seg) continue;
      const codeMatch = seg.match(/^\uE000CODE_(\d+)\uE000$/);
      if (codeMatch) {
        const cIdx = parseInt(codeMatch[1], 10);
        const item = codeItems[cIdx];
        if (item) {
          finalTokens.push({
            kind: "code",
            text: item.code,
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
          finalTokens.push({
            ...item,
            bold: token.bold || item.bold,
            italic: token.italic || item.italic,
          });
          continue;
        }
      }

      finalTokens.push({
        ...token,
        text: seg,
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
      let callout: "NOTE" | "TIP" | "IMPORTANT" | "WARNING" | "CAUTION" | undefined;
      if (quoteLines.length > 0) {
        const calloutMatch = quoteLines[0].match(/^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*(.*)$/i);
        if (calloutMatch) {
          callout = calloutMatch[1].toUpperCase() as typeof callout;
          if (calloutMatch[2]) {
            quoteLines[0] = calloutMatch[2];
          } else {
            quoteLines.shift();
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

function CodeBlockCard({ lang, code }: { lang: string; code: string }) {
  const [copied, setCopied] = useState(false);

  const normalizedLang = (lang || "").toLowerCase();
  if (normalizedLang.startsWith("practice-")) {
    let parsedTitle = "Bài tập thực hành tương tác";
    try {
      const parsed = JSON.parse(code) as { title?: string; topic?: string };
      if (parsed.title || parsed.topic) {
        parsedTitle = parsed.title || parsed.topic || parsedTitle;
      }
    } catch {
      // Ignore parse error for preview card
    }
    const kindLabel =
      normalizedLang === "practice-finance"
        ? "Biểu đồ Nến & Quản trị Rủi ro"
        : normalizedLang === "practice-english"
          ? "Luyện Nghe & Nói Tiếng Anh"
          : normalizedLang === "practice-code"
            ? "Bàn thực hành Code"
            : "Tình huống thực hành";

    return (
      <div className="my-4 rounded-xl border border-indigo-200 dark:border-indigo-800/70 bg-indigo-50/50 dark:bg-indigo-950/30 p-4 font-sans">
        <div className="text-[11px] font-semibold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
          {kindLabel}
        </div>
        <div className="text-sm font-semibold text-slate-900 dark:text-slate-100 mt-0.5">
          {parsedTitle}
        </div>
        <p className="text-xs text-slate-600 dark:text-slate-400 mt-1">
          Chuyển sang tab <span className="font-semibold">Thực hành</span> ở phía trên để làm bài trực tiếp.
        </p>
      </div>
    );
  }

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(true);
      setTimeout(() => setCopied(false), 1800);
    } catch {
      // Clipboard blocked in headless context; ignore gracefully
    }
  };

  return (
    <div className="my-4 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-950 text-slate-100 overflow-hidden shadow-xs font-sans max-w-full">
      <div className="flex items-center justify-between px-3.5 py-2 bg-slate-900 border-b border-slate-800 text-[11px]">
        <span className="font-mono font-bold uppercase tracking-wider text-indigo-300">
          {lang || "code"}
        </span>
        <button
          type="button"
          onClick={handleCopy}
          className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold transition cursor-pointer"
        >
          {copied ? (
            <>
              <Check className="w-3 h-3 text-emerald-400" />
              <span className="text-emerald-300">Đã sao chép</span>
            </>
          ) : (
            <>
              <Copy className="w-3 h-3" />
              <span>Sao chép</span>
            </>
          )}
        </button>
      </div>
      <pre className="p-3.5 overflow-x-auto max-w-full text-[13px] leading-relaxed font-mono text-slate-100">
        <code>{code}</code>
      </pre>
    </div>
  );
}

function normalizeTitleForComparison(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .replace(/[*_`~#]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, " ")
    .trim();
}

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
  onHeadingsExtracted
}: LessonMarkdownViewProps) {
  const cleanText = (text: string) =>
    cleanReadingMode ? cleanStudyInlineArtifacts(text) : text;

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
      pieceText: string,
      key: string
    ): React.ReactNode => {
      if (token.kind === "image") {
        return (
          <img
            key={key}
            src={sanitizeSafeUrl(token.href)}
            alt={token.alt || ""}
            className="my-3 max-w-full rounded-xl border border-slate-200 dark:border-slate-800 shadow-xs"
            loading="lazy"
          />
        );
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
                onOpenRelativeMarkdown(token.mdTargetPath!, token.text);
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

  const renderDetailsAccordion = (
    summary: string,
    body: string,
    keyId: string,
    blockId: string
  ) => {
    const cleanSummary = cleanText(summary) || "Xem gợi ý";
    const subBlocks = parseMarkdownBlocks(body);
    return (
      <details
        key={keyId}
        data-testid="lesson-details-accordion"
        className="group my-2.5 rounded-xl border border-indigo-200/80 dark:border-indigo-800/70 bg-indigo-50/40 dark:bg-indigo-950/25 overflow-hidden font-sans"
      >
        <summary className="flex items-center gap-2 px-3.5 py-2 text-xs sm:text-[13px] font-bold text-indigo-900 dark:text-indigo-200 cursor-pointer select-none list-none [&::-webkit-details-marker]:hidden hover:bg-indigo-100/50 dark:hover:bg-indigo-900/35 transition">
          <ChevronRight className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400 shrink-0 transition-transform duration-150 group-open:rotate-90" />
          <span className="flex-1 min-w-0">
            {renderInlineTokens(
              tokenizeInlineMarkdown(cleanSummary, sourceFolder, sourceRelativePath),
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
                return <CodeBlockCard key={sbKey} lang={sb.lang} code={sb.code} />;
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
                          tokenizeInlineMarkdown(cleanText(it.text), sourceFolder, sourceRelativePath),
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
                  : sb.type === "details"
                  ? `${sb.summary} — ${sb.body}`
                  : "";
              const display = cleanText(rawParagraph);
              if (!display) return null;
              return (
                <p key={sbKey} className="break-words">
                  {renderInlineTokens(
                    tokenizeInlineMarkdown(display, sourceFolder, sourceRelativePath),
                    sbKey
                  )}
                </p>
              );
            })
          )}
        </div>
      </details>
    );
  };

  return (
    <div data-testid="lesson-markdown-view" className="space-y-4 text-slate-800 dark:text-slate-200 min-w-0 max-w-full">
      {indexedBlocks.map(({ block, bIdx }) => {
        const blockId = makeDeterministicBlockUuid(bIdx);

        if (block.type === "hr") {
          return <hr key={blockId} className="my-6 border-slate-200 dark:border-slate-800" />;
        }

        if (block.type === "code") {
          return <CodeBlockCard key={blockId} lang={block.lang} code={block.code} />;
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

          return (
            <HeadingTag
              key={blockId}
              id={block.id}
              onMouseUp={() => onSelectBlockText(blockId, plain)}
              className={`${sizeClass} text-slate-900 dark:text-white font-sans scroll-mt-20 break-words`}
            >
              {renderInlineTokens(tokens, blockId)}
            </HeadingTag>
          );
        }

        if (block.type === "table") {
          const { headers, alignments, rows } = block.table;
          return (
            <div
              key={blockId}
              className="my-4 w-full max-w-full overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-800 shadow-2xs font-sans"
            >
              <table className="w-full border-collapse text-left text-xs sm:text-sm">
                <thead className="bg-slate-50 dark:bg-slate-800/80 border-b border-slate-200 dark:border-slate-700">
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
                          className={`px-3.5 py-2.5 font-bold text-slate-900 dark:text-slate-100 whitespace-nowrap ${alignCls}`}
                        >
                          {renderInlineTokens(tokens, `${blockId}-th-${hIdx}`)}
                        </th>
                      );
                    })}
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 dark:divide-slate-800/80">
                  {rows.map((row, rIdx) => (
                    <tr
                      key={rIdx}
                      className="hover:bg-slate-50/70 dark:hover:bg-slate-800/30 transition-colors"
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
                            className={`px-3.5 py-2.5 text-slate-700 dark:text-slate-300 align-top leading-relaxed ${alignCls}`}
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
          );
        }

        if (block.type === "blockquote") {
          const calloutStyles: Record<
            string,
            { container: string; titleColor: string; label: string; icon: React.ReactNode }
          > = {
            NOTE: {
              container: "border-l-sky-500 bg-sky-50/80 dark:bg-sky-950/30 border-sky-200 dark:border-sky-900/60",
              titleColor: "text-sky-800 dark:text-sky-300",
              label: "Ghi chú",
              icon: <Info className="w-4 h-4 text-sky-600 dark:text-sky-400" />
            },
            TIP: {
              container: "border-l-emerald-500 bg-emerald-50/80 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-900/60",
              titleColor: "text-emerald-800 dark:text-emerald-300",
              label: "Mẹo thực hành",
              icon: <Lightbulb className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
            },
            IMPORTANT: {
              container: "border-l-indigo-500 bg-indigo-50/80 dark:bg-indigo-950/30 border-indigo-200 dark:border-indigo-900/60",
              titleColor: "text-indigo-800 dark:text-indigo-300",
              label: "Quan trọng",
              icon: <Info className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            },
            WARNING: {
              container: "border-l-amber-500 bg-amber-50/80 dark:bg-amber-950/30 border-amber-200 dark:border-amber-900/60",
              titleColor: "text-amber-800 dark:text-amber-300",
              label: "Lưu ý",
              icon: <AlertTriangle className="w-4 h-4 text-amber-600 dark:text-amber-400" />
            },
            CAUTION: {
              container: "border-l-rose-500 bg-rose-50/80 dark:bg-rose-950/30 border-rose-200 dark:border-rose-900/60",
              titleColor: "text-rose-800 dark:text-rose-300",
              label: "Cảnh báo",
              icon: <Flame className="w-4 h-4 text-rose-600 dark:text-rose-400" />
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

        return (
          <div
            key={blockId}
            id={block.extraId || `block-${blockId}`}
            onMouseUp={() => onSelectBlockText(blockId, plain)}
            className="group relative leading-[1.8]"
          >
            <p className="break-words">
              {renderInlineTokens(tokens, blockId, matchedAnns)}
            </p>
            {audioLinks.map((au, idx) => (
              <InlineAudioPracticePlayer key={`au-p-${idx}`} {...au} />
            ))}
            {renderCitationBadges(matchedAnns)}
          </div>
        );
      })}
    </div>
  );
}
