"use client";

import React, { useState } from "react";
import { Copy, Check } from "lucide-react";

export interface CodeBlockCardProps {
  lang: string;
  code: string;
  onOpenPractice?: () => void;
}

export function CodeBlockCard({
  lang,
  code,
  onOpenPractice,
}: CodeBlockCardProps) {
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
        <div className="mt-3 flex items-center justify-between gap-3 flex-wrap">
          <p className="text-xs text-slate-600 dark:text-slate-400">
            Bài tập thực hành tương tác chuyên sâu cho phần kiến thức này.
          </p>
          {onOpenPractice && (
            <button
              type="button"
              onClick={onOpenPractice}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-semibold text-xs shadow-xs transition-colors cursor-pointer"
            >
              <span>Bắt đầu bài tập ngay ↓</span>
            </button>
          )}
        </div>
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
    <div className="my-4 rounded-[12px] border border-slate-800 bg-slate-950 text-slate-100 overflow-hidden shadow-xs font-sans max-w-full">
      <div className="flex items-center justify-between px-3.5 py-2 bg-slate-900 border-b border-slate-800 text-[11px]">
        <span className="font-mono font-bold uppercase tracking-wider text-indigo-300">
          {lang || "code"}
        </span>
        <button
          type="button"
          onClick={handleCopy}
          className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold transition cursor-pointer"
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
      <pre className="p-4 overflow-x-auto max-w-full text-[13px] leading-relaxed font-mono text-slate-100">
        <code>{code}</code>
      </pre>
    </div>
  );
}
