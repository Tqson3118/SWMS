import React from "react";
import katex from "katex";

export interface MathRenderOptions {
  displayMode?: boolean;
}

/**
 * Safely render KaTeX TeX string to HTML string with error fallback.
 */
export function renderKatexToString(expr: string, displayMode = false): string {
  if (!expr || !expr.trim()) return "";
  try {
    return katex.renderToString(expr.trim(), {
      displayMode,
      throwOnError: false,
      strict: false,
    });
  } catch {
    return `<span class="katex-error">${escapeHtml(expr)}</span>`;
  }
}

function escapeHtml(str: string): string {
  return str
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

export function InlineMath({ math }: { math: string }) {
  const html = React.useMemo(() => renderKatexToString(math, false), [math]);
  return <span className="inline-math inline" dangerouslySetInnerHTML={{ __html: html }} />;
}

export function BlockMath({ math }: { math: string }) {
  const html = React.useMemo(() => renderKatexToString(math, true), [math]);
  return (
    <span
      className="katex-display block my-2 overflow-x-auto text-center py-1 max-w-full"
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );
}

/**
 * Robust Rich Inline Markdown renderer for Quick-Check, Quiz, and Flashcards.
 * Parses:
 * - Code fences: ```lang\ncode\n```
 * - Inline code: `code`
 * - Block math: $$math$$
 * - Inline math: $math$ (safe against isolated currency symbols like $50 or $100)
 * - Bold: **text** or __text__
 * - Italic: *text* or _text_
 * Prevents leaking raw backticks, asterisks, or unparsed dollar symbols.
 */
export function RichInlineMarkdown({ text }: { text: string | null | undefined }) {
  if (!text) return null;
  return <>{renderRichInlineNodes(text)}</>;
}

export function renderRichInlineNodes(rawText: string): React.ReactNode[] {
  if (!rawText) return [];

  // Step 1: Protect fenced code blocks ```lang\n...```
  const codeFences: string[] = [];
  let processed = rawText.replace(/```([a-zA-Z0-9_-]*)\r?\n([\s\S]*?)```/g, (_m, lang, code) => {
    const idx = codeFences.length;
    codeFences.push(JSON.stringify({ lang: lang || "text", code }));
    return `\uE001FENCE_${idx}\uE001`;
  });

  // Step 2: Protect inline code `...`
  const inlineCodes: string[] = [];
  processed = processed.replace(/`([^`\n]+)`/g, (_m, code) => {
    const idx = inlineCodes.length;
    inlineCodes.push(code);
    return `\uE001CODE_${idx}\uE001`;
  });

  // Step 3: Protect block math $$...$$
  const blockMaths: string[] = [];
  processed = processed.replace(/\$\$([\s\S]+?)\$\$/g, (_m, math) => {
    const idx = blockMaths.length;
    blockMaths.push(math.trim());
    return `\uE001BMATH_${idx}\uE001`;
  });

  // Step 4: Protect inline math $...$
  // Guard against false positive currency like $10 or $100 by ensuring opening $ is not followed by space/comma
  // and contains either math notation (\, ^, _, {, }, =, +, -, *, /, <, >) or valid formula chars
  const inlineMaths: string[] = [];
  processed = processed.replace(
    /(?<!\\)\$(?!\s)([^$\r\n]+?)(?<![\s\\])\$/g,
    (full, math) => {
      // If it looks purely like a price or money range e.g. "100" or "50,000", do not treat as math
      if (/^\d+(?:,\d+)*(?:\.\d+)?$/.test(math.trim())) {
        return full;
      }
      const idx = inlineMaths.length;
      inlineMaths.push(math.trim());
      return `\uE001IMATH_${idx}\uE001`;
    }
  );

  // Step 5: Split by placeholder tokens \uE001(FENCE|CODE|BMATH|IMATH)_\d+\uE001
  const segments = processed.split(/(\uE001(?:FENCE|CODE|BMATH|IMATH)_\d+\uE001)/g);
  const result: React.ReactNode[] = [];

  segments.forEach((seg, sIdx) => {
    if (!seg) return;

    const fenceMatch = seg.match(/^\uE001FENCE_(\d+)\uE001$/);
    if (fenceMatch) {
      const idx = parseInt(fenceMatch[1], 10);
      try {
        const item = JSON.parse(codeFences[idx]);
        result.push(
          <pre
            key={`fence-${sIdx}`}
            className="my-2 p-2.5 rounded-lg bg-slate-900 text-slate-100 text-xs font-mono overflow-x-auto border border-slate-800"
          >
            <code>{item.code}</code>
          </pre>
        );
      } catch {
        result.push(<code key={`fence-${sIdx}`}>{codeFences[idx]}</code>);
      }
      return;
    }

    const codeMatch = seg.match(/^\uE001CODE_(\d+)\uE001$/);
    if (codeMatch) {
      const idx = parseInt(codeMatch[1], 10);
      result.push(
        <code
          key={`code-${sIdx}`}
          className="px-1.5 py-0.5 mx-0.5 rounded-md bg-slate-100 dark:bg-slate-800 text-indigo-700 dark:text-indigo-300 font-mono text-[0.88em] border border-slate-200 dark:border-slate-700 break-words"
        >
          {inlineCodes[idx]}
        </code>
      );
      return;
    }

    const bMathMatch = seg.match(/^\uE001BMATH_(\d+)\uE001$/);
    if (bMathMatch) {
      const idx = parseInt(bMathMatch[1], 10);
      result.push(<BlockMath key={`bmath-${sIdx}`} math={blockMaths[idx]} />);
      return;
    }

    const iMathMatch = seg.match(/^\uE001IMATH_(\d+)\uE001$/);
    if (iMathMatch) {
      const idx = parseInt(iMathMatch[1], 10);
      result.push(<InlineMath key={`imath-${sIdx}`} math={inlineMaths[idx]} />);
      return;
    }

    // Regular styled inline text: bold and italic
    result.push(
      <React.Fragment key={`text-${sIdx}`}>
        {renderBoldItalicText(seg, `t-${sIdx}`)}
      </React.Fragment>
    );
  });

  return result;
}

function renderBoldItalicText(str: string, baseKey: string): React.ReactNode {
  // Parse **bold** first
  const boldParts = str.split(/(\*\*[^*]+?\*\*|__[^_]+?__)/g);
  return boldParts.map((bPart, bIdx) => {
    if (!bPart) return null;
    const isBold =
      (bPart.startsWith("**") && bPart.endsWith("**")) ||
      (bPart.startsWith("__") && bPart.endsWith("__"));
    const inner = isBold ? bPart.slice(2, -2) : bPart;

    // Parse *italic* within each piece
    const italicParts = inner.split(/(\*[^*]+?\*|_[^_]+?_)/g);
    const renderedItalics = italicParts.map((iPart, iIdx) => {
      if (!iPart) return null;
      const isItalic =
        (iPart.startsWith("*") && iPart.endsWith("*")) ||
        (iPart.startsWith("_") && iPart.endsWith("_"));
      const iInner = isItalic ? iPart.slice(1, -1) : iPart;
      if (isItalic) {
        return (
          <em key={`${baseKey}-b${bIdx}-i${iIdx}`} className="italic">
            {iInner}
          </em>
        );
      }
      return iInner;
    });

    if (isBold) {
      return (
        <strong
          key={`${baseKey}-b${bIdx}`}
          className="font-bold text-slate-900 dark:text-white"
        >
          {renderedItalics}
        </strong>
      );
    }
    return renderedItalics;
  });
}
