"use client";

import React, { useCallback, useEffect, useRef, useState } from "react";
import { BookOpen, Clock, Loader2, Search, X } from "lucide-react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";

export interface SearchResultItem {
  lessonId: string;
  chapterId: string;
  chapterTitle: string;
  lessonTitle: string;
  snippet: string | null;
  matchCount: number;
}

export interface CourseSearchResponse {
  query: string;
  totalMatches: number;
  results: SearchResultItem[];
}

export interface CourseInSearchModalProps {
  courseId: string;
  isOpen: boolean;
  onClose: () => void;
  onSelectLesson: (lessonId: string) => void;
}

function normalizeChar(ch: string): string {
  return ch
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function removeAccents(str: string): string {
  return normalizeChar(str);
}

function buildNormalizedTextWithMap(text: string): {
  normText: string;
  charIndexMap: Array<{ start: number; end: number }>;
} {
  let normText = "";
  const charIndexMap: Array<{ start: number; end: number }> = [];

  let i = 0;
  while (i < text.length) {
    const code = text.charCodeAt(i);
    const isSurrogatePair =
      code >= 0xd800 &&
      code <= 0xdbff &&
      i + 1 < text.length &&
      text.charCodeAt(i + 1) >= 0xdc00 &&
      text.charCodeAt(i + 1) <= 0xdfff;
    const stepEnd = isSurrogatePair ? i + 2 : i + 1;
    const rawUnit = text.slice(i, stepEnd);

    // Combining diacritical mark attaches to the preceding base character
    if (/^[\u0300-\u036f]+$/.test(rawUnit)) {
      if (charIndexMap.length > 0) {
        charIndexMap[charIndexMap.length - 1].end = stepEnd;
      }
      i = stepEnd;
      continue;
    }

    const mapped = normalizeChar(rawUnit);
    for (let j = 0; j < mapped.length; j++) {
      normText += mapped[j];
      charIndexMap.push({ start: i, end: stepEnd });
    }
    i = stepEnd;
  }

  return { normText, charIndexMap };
}

function HighlightedText({ text, query }: { text: string; query: string }) {
  if (!query.trim() || !text) {
    return <span>{text}</span>;
  }

  const { normText, charIndexMap } = buildNormalizedTextWithMap(text);
  const normQuery = removeAccents(query.trim());
  const words = normQuery.split(/\s+/).filter(Boolean);

  if (words.length === 0 || charIndexMap.length === 0) return <span>{text}</span>;

  // Map normalized match indices back to original text indices
  const ranges: Array<{ start: number; end: number }> = [];

  for (const word of words) {
    let startIdx = 0;
    while ((startIdx = normText.indexOf(word, startIdx)) !== -1) {
      const endNormIdx = startIdx + word.length - 1;
      const startOrig = charIndexMap[startIdx]?.start;
      const endOrig = charIndexMap[endNormIdx]?.end;
      if (startOrig !== undefined && endOrig !== undefined && endOrig > startOrig) {
        ranges.push({ start: startOrig, end: endOrig });
      }
      startIdx += word.length;
    }
  }

  if (ranges.length === 0) return <span>{text}</span>;

  // Merge overlapping ranges
  ranges.sort((a, b) => a.start - b.start);
  const merged: Array<{ start: number; end: number }> = [];
  let current = ranges[0];

  for (let i = 1; i < ranges.length; i++) {
    if (ranges[i].start <= current.end) {
      current.end = Math.max(current.end, ranges[i].end);
    } else {
      merged.push(current);
      current = ranges[i];
    }
  }
  merged.push(current);

  const parts: React.ReactNode[] = [];
  let lastIdx = 0;

  merged.forEach((range, idx) => {
    if (range.start > lastIdx) {
      parts.push(text.slice(lastIdx, range.start));
    }
    parts.push(
      <mark
        key={idx}
        className="bg-yellow-200 dark:bg-yellow-800/60 text-text-primary px-0.5 rounded font-medium"
      >
        {text.slice(range.start, range.end)}
      </mark>
    );
    lastIdx = range.end;
  });

  if (lastIdx < text.length) {
    parts.push(text.slice(lastIdx));
  }

  return <span>{parts}</span>;
}

const MAX_RECENT_QUERIES = 5;

function getRecentStorageKey(courseId: string): string {
  return `studxy_course_search_recent_${courseId}`;
}

function readRecentQueries(courseId: string): string[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = window.sessionStorage.getItem(getRecentStorageKey(courseId));
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];
    return parsed
      .filter((item): item is string => typeof item === "string" && item.trim().length > 0)
      .slice(0, MAX_RECENT_QUERIES);
  } catch {
    return [];
  }
}

function persistRecentQuery(courseId: string, term: string): string[] {
  const trimmed = term.trim();
  if (!trimmed || typeof window === "undefined") return readRecentQueries(courseId);
  const existing = readRecentQueries(courseId);
  const next = [
    trimmed,
    ...existing.filter((q) => q.toLowerCase() !== trimmed.toLowerCase()),
  ].slice(0, MAX_RECENT_QUERIES);
  try {
    window.sessionStorage.setItem(getRecentStorageKey(courseId), JSON.stringify(next));
  } catch {
    // Ignore storage quota/access errors
  }
  return next;
}

export function CourseInSearchModal({
  courseId,
  isOpen,
  onClose,
  onSelectLesson,
}: CourseInSearchModalProps) {
  const { activeUserHeaderId } = useWorkspaceStore();
  const [query, setQuery] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [totalMatches, setTotalMatches] = useState(0);
  const [results, setResults] = useState<SearchResultItem[]>([]);
  const [hasSearched, setHasSearched] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [activeIndex, setActiveIndex] = useState(0);
  const [recentQueries, setRecentQueries] = useState<string[]>([]);

  const inputRef = useRef<HTMLInputElement>(null);
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const optionRefs = useRef<Array<HTMLButtonElement | null>>([]);
  const keyboardNavRef = useRef(false);

  useEffect(() => {
    if (isOpen) {
      setRecentQueries(readRecentQueries(courseId));
      setActiveIndex(0);
      const focusTimer = setTimeout(() => {
        inputRef.current?.focus();
        inputRef.current?.select();
      }, 50);
      return () => clearTimeout(focusTimer);
    } else {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
        debounceTimerRef.current = null;
      }
      abortControllerRef.current?.abort();
      abortControllerRef.current = null;
      setQuery("");
      setResults([]);
      setTotalMatches(0);
      setHasSearched(false);
      setErrorMsg(null);
      setIsLoading(false);
      setActiveIndex(0);
    }
  }, [isOpen, courseId]);

  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) clearTimeout(debounceTimerRef.current);
      abortControllerRef.current?.abort();
    };
  }, []);

  useEffect(() => {
    if (!isOpen || results.length === 0 || !keyboardNavRef.current) return;
    keyboardNavRef.current = false;
    optionRefs.current[activeIndex]?.scrollIntoView({ block: "nearest" });
  }, [activeIndex, isOpen, results.length]);

  const handleSelectResult = useCallback(
    (res: SearchResultItem) => {
      const trimmed = query.trim();
      if (trimmed) {
        setRecentQueries(persistRecentQuery(courseId, trimmed));
      }
      onSelectLesson(res.lessonId);
      onClose();
    },
    [courseId, onClose, onSelectLesson, query]
  );

  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.preventDefault();
        onClose();
        return;
      }

      if (results.length === 0) return;

      if (e.key === "ArrowDown") {
        e.preventDefault();
        keyboardNavRef.current = true;
        setActiveIndex((prev) => (prev + 1) % results.length);
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        keyboardNavRef.current = true;
        setActiveIndex((prev) => (prev - 1 + results.length) % results.length);
      } else if (e.key === "Home") {
        e.preventDefault();
        keyboardNavRef.current = true;
        setActiveIndex(0);
      } else if (e.key === "End") {
        e.preventDefault();
        keyboardNavRef.current = true;
        setActiveIndex(results.length - 1);
      } else if (e.key === "Enter") {
        const activeEl = document.activeElement;
        if (activeEl instanceof HTMLButtonElement && activeEl !== inputRef.current) {
          return;
        }
        const selected = results[activeIndex];
        if (selected) {
          e.preventDefault();
          handleSelectResult(selected);
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, results, activeIndex, onClose, handleSelectResult]);

  const executeSearch = async (searchTerm: string) => {
    const trimmed = searchTerm.trim();
    abortControllerRef.current?.abort();
    abortControllerRef.current = null;

    if (!trimmed) {
      setResults([]);
      setTotalMatches(0);
      setHasSearched(false);
      setErrorMsg(null);
      setIsLoading(false);
      setActiveIndex(0);
      return;
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setErrorMsg(null);

    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);

      const headers = getClientRequestHeaders({
        ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      });

      const resp = await fetch(
        `/api/v1/courses/${encodeURIComponent(courseId)}/search?q=${encodeURIComponent(trimmed)}`,
        {
          headers,
          credentials: "include",
          signal: controller.signal,
        }
      );

      if (!resp.ok) {
        throw new Error(`Tìm kiếm thất bại (${resp.status})`);
      }

      const data: CourseSearchResponse = await resp.json();
      if (controller.signal.aborted) return;

      const nextResults = data.results || [];
      setResults(nextResults);
      setTotalMatches(data.totalMatches || nextResults.length);
      setHasSearched(true);
      setActiveIndex(0);
      setRecentQueries(persistRecentQuery(courseId, trimmed));
    } catch (err) {
      if ((err instanceof DOMException && err.name === "AbortError") || controller.signal.aborted) {
        return;
      }
      setErrorMsg(err instanceof Error ? err.message : "Đã xảy ra lỗi khi tìm kiếm.");
      setResults([]);
      setTotalMatches(0);
      setActiveIndex(0);
    } finally {
      if (abortControllerRef.current === controller) {
        setIsLoading(false);
      }
    }
  };

  const handleQueryChange = (val: string) => {
    setQuery(val);
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
      debounceTimerRef.current = null;
    }
    if (!val.trim()) {
      void executeSearch("");
      return;
    }
    debounceTimerRef.current = setTimeout(() => {
      void executeSearch(val);
    }, 250);
  };

  const handleSelectRecentQuery = (term: string) => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
      debounceTimerRef.current = null;
    }
    setQuery(term);
    void executeSearch(term);
    inputRef.current?.focus();
  };

  const activeOptionId =
    results.length > 0 && activeIndex >= 0 && activeIndex < results.length
      ? `course-search-option-${activeIndex}`
      : undefined;

  if (!isOpen) return null;

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="course-search-modal-title"
      className="fixed inset-0 z-50 flex items-start justify-center pt-12 sm:pt-24 px-3 sm:px-4 bg-black/50 backdrop-blur-xs animate-in fade-in duration-150"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div className="w-full max-w-2xl bg-bg-primary rounded-xl border border-border shadow-2xl overflow-hidden flex flex-col max-h-[82vh]">
        {/* Header / Search Input */}
        <div className="p-3 sm:p-4 border-b border-border flex items-center gap-2.5 sm:gap-3 bg-bg-secondary/40">
          <Search className="w-5 h-5 text-text-tertiary shrink-0 ml-0.5 sm:ml-1" />
          <input
            ref={inputRef}
            id="course-search-modal-title"
            type="text"
            role="combobox"
            aria-expanded={results.length > 0}
            aria-controls="course-search-results-listbox"
            aria-activedescendant={activeOptionId}
            aria-autocomplete="list"
            value={query}
            onChange={(e) => handleQueryChange(e.target.value)}
            placeholder="Tìm kiếm nội dung trong toàn bộ khóa học... (từ khóa, bài học, chương)"
            className="flex-1 min-w-0 bg-transparent text-sm sm:text-base text-text-primary placeholder:text-text-tertiary focus:outline-none"
          />
          {isLoading ? (
            <Loader2 className="w-4 h-4 text-accent animate-spin shrink-0" />
          ) : query ? (
            <button
              type="button"
              onClick={() => {
                handleQueryChange("");
                inputRef.current?.focus();
              }}
              className="p-1 rounded text-text-tertiary hover:text-text-primary hover:bg-bg-tertiary transition cursor-pointer shrink-0"
              title="Xóa nội dung tìm"
              aria-label="Xóa nội dung tìm"
            >
              <X className="w-4 h-4" />
            </button>
          ) : null}
          <button
            type="button"
            onClick={onClose}
            className="px-2 py-1 rounded text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-tertiary transition cursor-pointer border border-border shrink-0"
          >
            Esc
          </button>
        </div>

        {/* Results Metadata Bar */}
        {hasSearched && !isLoading && (
          <div className="px-4 py-2 border-b border-border bg-bg-secondary/20 text-xs text-text-secondary flex items-center justify-between gap-2">
            <span className="truncate">
              {totalMatches > 0
                ? `Tìm thấy ${totalMatches} kết quả khớp từ khóa "${query}"`
                : `Không tìm thấy kết quả nào cho "${query}"`}
            </span>
            <span className="text-[11px] text-text-tertiary shrink-0 hidden sm:inline">
              Nhấn vào bài học để chuyển ngay
            </span>
          </div>
        )}

        {/* Results List */}
        <div className="flex-1 overflow-y-auto swms-thin-scrollbar p-3 space-y-2">
          {errorMsg && (
            <div className="p-4 text-center text-sm text-red-500 bg-red-500/10 rounded-lg">
              {errorMsg}
            </div>
          )}

          {!hasSearched && !isLoading && (
            <div className="py-6 sm:py-10 px-3 sm:px-4 space-y-5">
              {recentQueries.length > 0 && (
                <div className="space-y-2">
                  <div className="flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wider text-text-tertiary">
                    <Clock className="w-3.5 h-3.5" />
                    <span>Tìm gần đây</span>
                  </div>
                  <div className="flex flex-wrap items-center gap-1.5">
                    {recentQueries.map((recent) => (
                      <button
                        key={recent}
                        type="button"
                        onClick={() => handleSelectRecentQuery(recent)}
                        className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full border border-border bg-bg-secondary/60 hover:bg-accent-subtle hover:border-accent/40 hover:text-accent text-xs font-medium text-text-primary transition cursor-pointer max-w-full"
                      >
                        <Search className="w-3 h-3 text-text-tertiary shrink-0" />
                        <span className="truncate max-w-[200px]">{recent}</span>
                      </button>
                    ))}
                  </div>
                </div>
              )}

              <div className="text-center space-y-2 pt-2">
                <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-accent-subtle text-accent mb-1">
                  <Search className="w-6 h-6" />
                </div>
                <p className="text-sm font-medium text-text-primary">
                  Tìm kiếm toàn văn trong khóa học
                </p>
                <p className="text-xs text-text-secondary max-w-sm mx-auto">
                  Nhập tên chương, tiêu đề bài giảng hoặc đoạn nội dung bất kỳ để tra cứu nhanh toàn bộ giáo trình.
                </p>
                <div className="pt-2 text-[11px] text-text-tertiary flex flex-wrap items-center justify-center gap-2">
                  <span>Phím tắt mở:</span>
                  <kbd className="px-1.5 py-0.5 rounded bg-bg-secondary border border-border font-mono text-[10px]">
                    Ctrl+Shift+F
                  </kbd>
                  <span>/</span>
                  <kbd className="px-1.5 py-0.5 rounded bg-bg-secondary border border-border font-mono text-[10px]">
                    ⌘+Shift+F
                  </kbd>
                </div>
              </div>
            </div>
          )}

          {hasSearched && results.length === 0 && !isLoading && !errorMsg && (
            <div className="py-12 px-4 text-center space-y-2">
              <p className="text-sm font-medium text-text-secondary">
                Không tìm thấy bài học nào phù hợp với từ khóa này.
              </p>
              <p className="text-xs text-text-tertiary">
                Thử tìm bằng từ khóa ngắn gọn hơn hoặc không dấu.
              </p>
            </div>
          )}

          {results.length > 0 && (
            <div
              id="course-search-results-listbox"
              role="listbox"
              aria-label="Kết quả tìm kiếm trong khóa học"
              className="space-y-2"
            >
              {results.map((res, idx) => {
                const isActive = idx === activeIndex;
                return (
                  <button
                    key={`${res.chapterId}-${res.lessonId}`}
                    ref={(el) => {
                      optionRefs.current[idx] = el;
                    }}
                    id={`course-search-option-${idx}`}
                    role="option"
                    aria-selected={isActive}
                    type="button"
                    onClick={() => handleSelectResult(res)}
                    className={`w-full text-left p-3 rounded-lg border transition cursor-pointer group space-y-1.5 ${
                      isActive
                        ? "border-accent bg-accent-subtle/40 ring-1 ring-accent/40"
                        : "border-border hover:border-accent/40 bg-bg-primary hover:bg-bg-secondary/60"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center gap-1.5 min-w-0">
                        <BookOpen className="w-3.5 h-3.5 text-accent shrink-0" />
                        <span className="text-[11px] font-semibold text-text-tertiary uppercase tracking-wider truncate">
                          {res.chapterTitle}
                        </span>
                      </div>
                      {res.matchCount > 0 && (
                        <span className="shrink-0 px-1.5 py-0.5 rounded bg-accent-subtle text-accent text-[10px] font-semibold">
                          {res.matchCount} khớp
                        </span>
                      )}
                    </div>

                    <h4
                      className={`text-sm font-semibold transition leading-snug ${
                        isActive ? "text-accent" : "text-text-primary group-hover:text-accent"
                      }`}
                    >
                      <HighlightedText text={res.lessonTitle} query={query} />
                    </h4>

                    {res.snippet && (
                      <p className="text-xs text-text-secondary line-clamp-2 leading-relaxed bg-bg-secondary/40 p-2 rounded border border-border/50">
                        <HighlightedText text={res.snippet} query={query} />
                      </p>
                    )}
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {/* Keyboard Shortcut Footer */}
        <div className="px-3 sm:px-4 py-2 border-t border-border bg-bg-secondary/30 text-[11px] text-text-tertiary flex items-center justify-between gap-2 shrink-0">
          <span className="font-medium">↑↓ Chọn · Enter Mở bài · Esc Đóng</span>
          {results.length > 0 && (
            <span className="hidden sm:inline tabular-nums">
              {activeIndex + 1}/{results.length}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
