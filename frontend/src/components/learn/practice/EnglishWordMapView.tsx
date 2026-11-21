"use client";

import React, { useState } from "react";
import {
  Volume2,
  BookmarkPlus,
  CheckCircle2,
  Sparkles,
  Link2,
  BookOpen,
  ArrowRight,
  Layers,
  Check,
  Zap,
  HelpCircle
} from "lucide-react";

export interface WordMapNode {
  text: string;
  ipa?: string;
  meaningVi: string;
  example?: string;
}

export interface WordMapData {
  centralWord: string;
  ipa?: string;
  meaningVi: string;
  collocations: WordMapNode[];
  phrasalVerbs: WordMapNode[];
  synonyms: WordMapNode[];
  contextExamples: Array<{ sentence: string; meaningVi: string }>;
}

export interface EnglishWordMapViewProps {
  wordMap: WordMapData;
  onSpeak: (text: string, rate?: number) => void;
  onSaveToFlashcard: (params: { front: string; back: string; ipa?: string; example?: string }) => Promise<void>;
  onComplete: () => Promise<void>;
  isCompleted?: boolean;
  bestScore?: number | null;
}

export default function EnglishWordMapView({
  wordMap,
  onSpeak,
  onSaveToFlashcard,
  onComplete,
  isCompleted = false,
  bestScore = null,
}: EnglishWordMapViewProps) {
  const [selectedNode, setSelectedNode] = useState<WordMapNode | null>(null);
  const [savedKeys, setSavedKeys] = useState<Record<string, boolean>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [isFinishing, setIsFinishing] = useState(false);

  const handleSaveNode = async (node: WordMapNode) => {
    if (savedKeys[node.text] || isSaving) return;
    setIsSaving(true);
    try {
      await onSaveToFlashcard({
        front: node.text,
        back: node.meaningVi,
        ipa: node.ipa,
        example: node.example,
      });
      setSavedKeys(prev => ({ ...prev, [node.text]: true }));
    } finally {
      setIsSaving(false);
    }
  };

  const handleSaveCentral = async () => {
    if (savedKeys[wordMap.centralWord] || isSaving) return;
    setIsSaving(true);
    try {
      await onSaveToFlashcard({
        front: wordMap.centralWord,
        back: wordMap.meaningVi,
        ipa: wordMap.ipa,
        example: wordMap.contextExamples?.[0]?.sentence,
      });
      setSavedKeys(prev => ({ ...prev, [wordMap.centralWord]: true }));
    } finally {
      setIsSaving(false);
    }
  };

  const handleFinish = async () => {
    setIsFinishing(true);
    try {
      await onComplete();
    } finally {
      setIsFinishing(false);
    }
  };

  return (
    <div className="space-y-6 animate-fadeIn">
      {/* Top Banner & Instructions */}
      <div className="flex flex-wrap items-center justify-between gap-3 p-4 rounded-2xl bg-gradient-to-r from-[#5E51E8]/10 via-purple-500/10 to-indigo-500/5 border border-[#5E51E8]/20 text-xs">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-xl bg-[#5E51E8] text-white flex items-center justify-center font-bold shadow-sm">
            <Layers className="w-4 h-4" />
          </div>
          <div>
            <h4 className="font-bold text-sm text-[var(--text-primary,#18181b)] dark:text-zinc-100">
              Sơ Đồ Tư Duy Từ Vựng (Interactive Word Map)
            </h4>
            <p className="text-[var(--text-secondary,#52525b)] dark:text-zinc-400">
              Chạm vào từng nhánh để nghe phát âm IPA chuẩn, xem ví dụ thực tế và lưu vào Flashcard FSRS để ôn tập.
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {isCompleted ? (
            <span className="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 font-bold border border-emerald-500/30">
              <CheckCircle2 className="w-4 h-4" />
              <span>Đã đạt {bestScore ? `(${bestScore}đ)` : ""}</span>
            </span>
          ) : (
            <span className="px-3 py-1.5 rounded-full bg-zinc-200 dark:bg-zinc-800 text-[var(--text-secondary,#52525b)] dark:text-zinc-400 font-medium">
              Chưa hoàn thành
            </span>
          )}
        </div>
      </div>

      {/* Visual Interactive SVG Mind Map Canvas */}
      <div className="rounded-3xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-900/90 shadow-sm p-6 sm:p-8 space-y-8 relative overflow-hidden">
        {/* Subtle decorative background grid */}
        <div className="absolute inset-0 bg-[radial-gradient(#e5e7eb_1px,transparent_1px)] [background-size:16px_16px] opacity-40 dark:opacity-10 pointer-events-none" />

        {/* 1. CENTRAL KEYWORD NODE */}
        <div className="relative z-10 flex flex-col items-center justify-center text-center">
          <div className="p-5 sm:p-6 rounded-3xl bg-gradient-to-br from-[#5E51E8] to-[#4338CA] text-white shadow-xl shadow-[#5E51E8]/20 max-w-md w-full border border-indigo-400/30 space-y-2">
            <span className="text-[10px] uppercase font-bold tracking-widest text-indigo-200 bg-white/10 px-2.5 py-0.5 rounded-full inline-block">
              Từ Khóa Trung Tâm
            </span>
            <div className="flex items-center justify-center gap-3">
              <h2 className="text-2xl sm:text-3xl font-black tracking-tight">{wordMap.centralWord}</h2>
              <button
                type="button"
                onClick={() => onSpeak(wordMap.centralWord, 1.0)}
                title="Nghe phát âm từ khóa trung tâm"
                className="w-9 h-9 rounded-full bg-white/20 hover:bg-white/30 flex items-center justify-center transition cursor-pointer active:scale-95"
              >
                <Volume2 className="w-4 h-4" />
              </button>
            </div>
            {wordMap.ipa && (
              <p className="font-mono text-sm text-indigo-100 tracking-wider">{wordMap.ipa}</p>
            )}
            <p className="text-xs sm:text-sm text-indigo-50 font-medium">{wordMap.meaningVi}</p>

            <div className="pt-2 flex items-center justify-center gap-2">
              <button
                type="button"
                onClick={handleSaveCentral}
                disabled={savedKeys[wordMap.centralWord]}
                className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition shadow-xs ${
                  savedKeys[wordMap.centralWord]
                    ? "bg-emerald-500 text-white cursor-default"
                    : "bg-white text-[#5E51E8] hover:bg-indigo-50"
                }`}
              >
                {savedKeys[wordMap.centralWord] ? <Check className="w-3.5 h-3.5" /> : <BookmarkPlus className="w-3.5 h-3.5" />}
                <span>{savedKeys[wordMap.centralWord] ? "Đã lưu vào FSRS" : "Lưu vào Flashcard FSRS"}</span>
              </button>
            </div>
          </div>

          {/* SVG Connecting lines downward */}
          <div className="w-full flex justify-center py-2">
            <div className="w-0.5 h-6 bg-gradient-to-b from-[#5E51E8] to-indigo-300 dark:to-indigo-700" />
          </div>
        </div>

        {/* 2. FOUR RADIATING BRANCH HUBS */}
        <div className="relative z-10 grid grid-cols-1 md:grid-cols-2 gap-5">
          {/* BRANCH A: Collocations */}
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#f4f4f5)]/60 dark:bg-zinc-800/40 p-4 sm:p-5 space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  1. Cụm từ thường gặp (Collocations)
                </h3>
              </div>
              <span className="text-[11px] font-semibold text-emerald-600 dark:text-emerald-400">
                {wordMap.collocations.length} cụm
              </span>
            </div>

            <div className="space-y-2">
              {wordMap.collocations.map((item, idx) => (
                <div
                  key={idx}
                  onClick={() => setSelectedNode(item)}
                  className={`p-3 rounded-xl border transition-all cursor-pointer ${
                    selectedNode?.text === item.text
                      ? "border-[#5E51E8] bg-[#5E51E8]/5 ring-1 ring-[#5E51E8]/20"
                      : "border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 hover:border-indigo-300"
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                        {item.text}
                      </span>
                      {item.ipa && (
                        <span className="text-[11px] font-mono text-[#5E51E8] dark:text-[#818CF8]">
                          {item.ipa}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          onSpeak(item.text);
                        }}
                        className="w-7 h-7 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-700 flex items-center justify-center text-[var(--text-secondary,#52525b)]"
                      >
                        <Volume2 className="w-3.5 h-3.5" />
                      </button>
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          handleSaveNode(item);
                        }}
                        disabled={savedKeys[item.text]}
                        className={`px-2 py-1 rounded-lg text-[11px] font-semibold transition ${
                          savedKeys[item.text]
                            ? "text-emerald-600 dark:text-emerald-400 bg-emerald-500/10"
                            : "text-[#5E51E8] dark:text-[#818CF8] hover:bg-[#5E51E8]/10"
                        }`}
                      >
                        {savedKeys[item.text] ? "Đã lưu" : "+ Thẻ FSRS"}
                      </button>
                    </div>
                  </div>
                  <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-1">
                    {item.meaningVi}
                  </p>
                  {item.example && (
                    <p className="text-[11px] italic text-zinc-500 dark:text-zinc-400 mt-1.5 border-l-2 border-emerald-500/40 pl-2">
                      &quot;{item.example}&quot;
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* BRANCH B: Phrasal Verbs & Idioms */}
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#f4f4f5)]/60 dark:bg-zinc-800/40 p-4 sm:p-5 space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-purple-500" />
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  2. Cụm Động Từ & Thành Ngữ (Phrasal Verbs)
                </h3>
              </div>
              <span className="text-[11px] font-semibold text-purple-600 dark:text-purple-400">
                {wordMap.phrasalVerbs.length} cụm
              </span>
            </div>

            <div className="space-y-2">
              {wordMap.phrasalVerbs.map((item, idx) => (
                <div
                  key={idx}
                  onClick={() => setSelectedNode(item)}
                  className={`p-3 rounded-xl border transition-all cursor-pointer ${
                    selectedNode?.text === item.text
                      ? "border-purple-500 bg-purple-500/5 ring-1 ring-purple-500/20"
                      : "border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 hover:border-purple-300"
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                        {item.text}
                      </span>
                      {item.ipa && (
                        <span className="text-[11px] font-mono text-purple-600 dark:text-purple-400">
                          {item.ipa}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          onSpeak(item.text);
                        }}
                        className="w-7 h-7 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-700 flex items-center justify-center text-[var(--text-secondary,#52525b)]"
                      >
                        <Volume2 className="w-3.5 h-3.5" />
                      </button>
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          handleSaveNode(item);
                        }}
                        disabled={savedKeys[item.text]}
                        className={`px-2 py-1 rounded-lg text-[11px] font-semibold transition ${
                          savedKeys[item.text]
                            ? "text-emerald-600 dark:text-emerald-400 bg-emerald-500/10"
                            : "text-purple-600 dark:text-purple-400 hover:bg-purple-500/10"
                        }`}
                      >
                        {savedKeys[item.text] ? "Đã lưu" : "+ Thẻ FSRS"}
                      </button>
                    </div>
                  </div>
                  <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-1">
                    {item.meaningVi}
                  </p>
                  {item.example && (
                    <p className="text-[11px] italic text-zinc-500 dark:text-zinc-400 mt-1.5 border-l-2 border-purple-500/40 pl-2">
                      &quot;{item.example}&quot;
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* BRANCH C: Synonyms & Antonyms */}
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#f4f4f5)]/60 dark:bg-zinc-800/40 p-4 sm:p-5 space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-blue-500" />
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  3. Từ Đồng Nghĩa & Liên Quan (Synonyms)
                </h3>
              </div>
              <span className="text-[11px] font-semibold text-blue-600 dark:text-blue-400">
                {wordMap.synonyms.length} từ
              </span>
            </div>

            <div className="space-y-2">
              {wordMap.synonyms.map((item, idx) => (
                <div
                  key={idx}
                  onClick={() => setSelectedNode(item)}
                  className={`p-3 rounded-xl border transition-all cursor-pointer ${
                    selectedNode?.text === item.text
                      ? "border-blue-500 bg-blue-500/5 ring-1 ring-blue-500/20"
                      : "border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 hover:border-blue-300"
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-bold text-[var(--text-primary,#18181b)] dark:text-zinc-100">
                        {item.text}
                      </span>
                      {item.ipa && (
                        <span className="text-[11px] font-mono text-blue-600 dark:text-blue-400">
                          {item.ipa}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          onSpeak(item.text);
                        }}
                        className="w-7 h-7 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-700 flex items-center justify-center text-[var(--text-secondary,#52525b)]"
                      >
                        <Volume2 className="w-3.5 h-3.5" />
                      </button>
                      <button
                        type="button"
                        onClick={e => {
                          e.stopPropagation();
                          handleSaveNode(item);
                        }}
                        disabled={savedKeys[item.text]}
                        className={`px-2 py-1 rounded-lg text-[11px] font-semibold transition ${
                          savedKeys[item.text]
                            ? "text-emerald-600 dark:text-emerald-400 bg-emerald-500/10"
                            : "text-blue-600 dark:text-blue-400 hover:bg-blue-500/10"
                        }`}
                      >
                        {savedKeys[item.text] ? "Đã lưu" : "+ Thẻ FSRS"}
                      </button>
                    </div>
                  </div>
                  <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 mt-1">
                    {item.meaningVi}
                  </p>
                  {item.example && (
                    <p className="text-[11px] italic text-zinc-500 dark:text-zinc-400 mt-1.5 border-l-2 border-blue-500/40 pl-2">
                      &quot;{item.example}&quot;
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* BRANCH D: Context Sentences */}
          <div className="rounded-2xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 bg-[var(--surface-elevated,#f4f4f5)]/60 dark:bg-zinc-800/40 p-4 sm:p-5 space-y-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="w-2.5 h-2.5 rounded-full bg-amber-500" />
                <h3 className="text-xs font-bold uppercase tracking-wider text-[var(--text-secondary,#52525b)] dark:text-zinc-300">
                  4. Ví Dụ Ngữ Cảnh Thực Tế (Contextual Usage)
                </h3>
              </div>
              <span className="text-[11px] font-semibold text-amber-600 dark:text-amber-400">
                {wordMap.contextExamples.length} câu
              </span>
            </div>

            <div className="space-y-2.5">
              {wordMap.contextExamples.map((ex, idx) => (
                <div
                  key={idx}
                  className="p-3.5 rounded-xl border border-[var(--border-calm,#e4e4e7)] dark:border-zinc-700/60 bg-[var(--surface-card,#ffffff)] dark:bg-zinc-800 space-y-2"
                >
                  <div className="flex items-start justify-between gap-2">
                    <p className="text-xs sm:text-sm font-medium text-[var(--text-primary,#18181b)] dark:text-zinc-100 leading-relaxed">
                      &quot;{ex.sentence}&quot;
                    </p>
                    <button
                      type="button"
                      onClick={() => onSpeak(ex.sentence)}
                      className="w-7 h-7 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-700 flex items-center justify-center text-[var(--text-secondary,#52525b)] shrink-0"
                    >
                      <Volume2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                  <p className="text-xs text-[var(--text-secondary,#52525b)] dark:text-zinc-400 italic">
                    {ex.meaningVi}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* 3. COMPLETION FOOTER */}
        <div className="pt-4 border-t border-[var(--border-calm,#e4e4e7)] dark:border-zinc-800 flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-xs text-[var(--text-secondary,#71717a)] dark:text-zinc-400">
            <Zap className="w-4 h-4 text-amber-500 shrink-0" />
            <span>Mẹo: Thẻ đã lưu sẽ tự động xuất hiện trong Workstation Ôn tập FSRS (/app/review).</span>
          </div>

          <button
            type="button"
            onClick={handleFinish}
            disabled={isFinishing}
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm min-h-[44px] bg-[#5E51E8] text-white hover:bg-[#4F46E5] shadow-sm transition active:scale-95 disabled:opacity-50"
          >
            <Check className="w-4 h-4" />
            <span>{isCompleted ? "Ghi nhận ôn tập lại sơ đồ" : "Hoàn thành nghiên cứu Sơ đồ"}</span>
          </button>
        </div>
      </div>
    </div>
  );
}
