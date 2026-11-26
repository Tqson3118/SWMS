"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { deduplicateCourses, type CourseLike } from "@/lib/course-helpers";
import {
  Search,
  Zap,
  Plus,
  CheckCircle2,
  AlertCircle,
  BookOpen,
  FileText,
  GraduationCap,
  NotebookPen,
  Layers,
  Tag,
  Compass,
} from "lucide-react";

type SearchResultType =
  | "action"
  | "course"
  | "lesson"
  | "document"
  | "notebook"
  | "flashcard"
  | "tag";

interface SearchResultItem {
  id: string;
  courseId?: string | null;
  type: SearchResultType;
  title: string;
  subtitle: string;
  snippet: string;
}

interface QuickActionDef {
  id: string;
  title: string;
  subtitle: string;
  snippet: string;
  keywords: string;
  featured: boolean;
}

interface CommandPaletteModalProps {
  onQuickCaptureCreated?: () => void;
}

function normalizeVietnamese(str: string): string {
  return str
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function cleanSearchText(str: string): string {
  return str
    .replace(/Thẻ FSRS\s*\(cloze\)/gi, "Thẻ điền khuyết")
    .replace(/Thẻ FSRS\s*\([^)]*\)/gi, "Thẻ ghi nhớ")
    .replace(/FSRS/gi, "ôn tập")
    .replace(/Cloze/gi, "điền khuyết");
}

const TYPE_LABELS: Record<SearchResultType, string> = {
  action: "Hành động",
  course: "Khóa học",
  lesson: "Bài giảng",
  document: "Tài liệu PDF",
  notebook: "Sổ tay",
  flashcard: "Thẻ ghi nhớ",
  tag: "Chủ đề",
};

const QUICK_ACTIONS: QuickActionDef[] = [
  {
    id: "action-new-course",
    title: "Tạo khóa học mới",
    subtitle: "Khởi tạo khóa học & giáo trình mới",
    snippet: "Mở trình tạo khóa học mới",
    keywords: "tao khoa hoc moi create new course studio",
    featured: true,
  },
  {
    id: "action-practice-hub",
    title: "Mở trung tâm luyện tập",
    subtitle: "Thực hành tương tác & bài tập kỹ năng",
    snippet: "Mở không gian luyện tập kỹ năng",
    keywords: "mo hub luyen tap thuc hanh lab practice",
    featured: true,
  },
  {
    id: "action-exams",
    title: "Ngân hàng đề & Thi thử",
    subtitle: "Luyện đề thi thử & chấm điểm",
    snippet: "Mở ngân hàng đề thi thử",
    keywords: "ngan hang de thi thu quiz exam test giai de",
    featured: true,
  },
  {
    id: "action-review-fsrs",
    title: "Ôn tập thẻ FSRS",
    subtitle: "Lặp lại ngắt quãng với thuật toán FSRS",
    snippet: "Mở phòng ôn tập thẻ ghi nhớ",
    keywords: "on tap the fsrs flashcard review",
    featured: true,
  },
  {
    id: "action-toggle-theme",
    title: "Chuyển giao diện sáng / tối",
    subtitle: "Bật hoặc tắt chế độ tối bảo vệ mắt",
    snippet: "Phím tắt nhanh: Ctrl + D",
    keywords: "chuyen giao dien sang toi dark light mode theme",
    featured: false,
  },
  {
    id: "action-open-shortcuts",
    title: "Xem bảng phím tắt (?)",
    subtitle: "Tra cứu phím tắt và luồng học tập",
    snippet: "Nhấn ? để mở bảng tra cứu phím tắt",
    keywords: "xem bang phim tat keyboard shortcuts help ?",
    featured: false,
  },
];

function TypeIcon({ type }: { type: SearchResultType }) {
  switch (type) {
    case "action":
      return <Compass className="w-4 h-4 text-[var(--accent)] shrink-0" />;
    case "course":
      return <GraduationCap className="w-4 h-4 text-accent shrink-0" />;
    case "lesson":
      return <BookOpen className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />;
    case "document":
      return <FileText className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />;
    case "notebook":
      return <NotebookPen className="w-4 h-4 text-blue-600 dark:text-blue-400 shrink-0" />;
    case "flashcard":
      return <Layers className="w-4 h-4 text-purple-600 dark:text-purple-400 shrink-0" />;
    default:
      return <Tag className="w-4 h-4 text-text-tertiary shrink-0" />;
  }
}

export function CommandPaletteModal({ onQuickCaptureCreated }: CommandPaletteModalProps) {
  const pathname = usePathname();
  const router = useRouter();
  const {
    commandPaletteOpen,
    setCommandPaletteOpen,
    toggleDarkMode,
    setActiveMode,
    setActiveCourseId,
    setActiveLesson,
    setActiveDocument,
    setSelectedTagFilterId,
  } = useWorkspaceStore();

  const [query, setQuery] = useState("");
  const [results, setResults] = useState<SearchResultItem[]>([]);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [loading, setLoading] = useState(false);
  const [quickCaptureText, setQuickCaptureText] = useState("");
  const [captureSavedMsg, setCaptureSavedMsg] = useState<string | null>(null);
  const [captureErrorMsg, setCaptureErrorMsg] = useState<string | null>(null);
  const resultRefs = useRef<(HTMLButtonElement | null)[]>([]);

  const quickActionItems = useMemo<SearchResultItem[]>(() => {
    const normQ = normalizeVietnamese(query.trim());
    const matched = normQ
      ? QUICK_ACTIONS.filter((a) =>
          normalizeVietnamese(`${a.title} ${a.subtitle} ${a.keywords}`).includes(normQ)
        )
      : QUICK_ACTIONS.filter((a) => a.featured);

    return matched.slice(0, 4).map((a) => ({
      id: a.id,
      type: "action" as const,
      title: a.title,
      subtitle: a.subtitle,
      snippet: a.snippet,
    }));
  }, [query]);

  const combinedResults = useMemo(
    () => [...quickActionItems, ...results],
    [quickActionItems, results]
  );

  const safeSelectedIndex =
    combinedResults.length > 0 ? Math.min(selectedIndex, combinedResults.length - 1) : 0;

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setCommandPaletteOpen(!commandPaletteOpen);
        return;
      }
      if (!commandPaletteOpen) return;
      if (e.key === "Escape") {
        e.preventDefault();
        setCommandPaletteOpen(false);
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [commandPaletteOpen, setCommandPaletteOpen]);

  useEffect(() => {
    if (!commandPaletteOpen) {
      setSelectedIndex(0);
      return;
    }

    const controller = new AbortController();
    setLoading(true);

    const timer = setTimeout(() => {
      const headers = getClientRequestHeaders();

      Promise.all([
        fetch(`/api/v1/workspace/search?q=${encodeURIComponent(query)}`, {
          headers,
          signal: controller.signal,
        })
          .then((r) => (r.ok ? r.json() : { results: [] }))
          .catch((err) => {
            if (err instanceof DOMException && err.name === "AbortError") throw err;
            return { results: [] };
          }),
        fetch(`/api/v1/courses`, {
          headers,
          signal: controller.signal,
        })
          .then((r) => (r.ok ? r.json() : []))
          .catch((err) => {
            if (err instanceof DOMException && err.name === "AbortError") throw err;
            return [];
          }),
      ])
        .then(([searchData, coursesRaw]) => {
          if (controller.signal.aborted) return;
          const normQ = normalizeVietnamese(query.trim());
          const uniqueCourses = Array.isArray(coursesRaw)
            ? deduplicateCourses(coursesRaw as CourseLike[])
            : [];
          const courseItems: SearchResultItem[] = uniqueCourses
            .filter((c) => {
              if (!normQ) return true;
              const haystack = normalizeVietnamese(
                `${c.title || ""} ${(c as { description?: string }).description || ""}`
              );
              return haystack.includes(normQ);
            })
            .slice(0, 4)
            .map((c) => {
              const cObj = c as {
                courseId?: string;
                id?: string;
                title?: string;
                description?: string;
                totalLessons?: number;
                progressPercent?: number;
              };
              return {
                id: String(cObj.courseId || cObj.id || ""),
                courseId: String(cObj.courseId || cObj.id || ""),
                type: "course" as const,
                title: String(cObj.title || "Khóa học"),
                subtitle: `${Number(cObj.totalLessons || 0)} bài giảng · ${Math.round(
                  Number(cObj.progressPercent || 0)
                )}% hoàn thành`,
                snippet: String(cObj.description || ""),
              };
            })
            .filter((c) => Boolean(c.id));

          const seenWorkspaceKeys = new Set<string>();
          const workspaceItems: SearchResultItem[] = [];
          const rawResults = Array.isArray(searchData?.results) ? searchData.results : [];
          for (const item of rawResults) {
            const titleStr = String(item?.title || "").trim();
            const dedupKey = `${item?.type || ""}::${titleStr.toLowerCase()}`;
            if (!titleStr || seenWorkspaceKeys.has(dedupKey)) continue;
            seenWorkspaceKeys.add(dedupKey);
            workspaceItems.push({
              ...item,
              title: cleanSearchText(titleStr),
              subtitle: cleanSearchText(String(item?.subtitle || "")),
              snippet: cleanSearchText(String(item?.snippet || "")),
            });
            if (workspaceItems.length >= 10) break;
          }

          setResults([...courseItems, ...workspaceItems]);
          setSelectedIndex(0);
          setLoading(false);
        })
        .catch((err) => {
          if (
            controller.signal.aborted ||
            (err instanceof DOMException && err.name === "AbortError")
          ) {
            return;
          }
          setLoading(false);
        });
    }, 150);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [query, commandPaletteOpen]);

  useEffect(() => {
    if (!commandPaletteOpen) return;
    const el = resultRefs.current[safeSelectedIndex];
    if (el && typeof el.scrollIntoView === "function") {
      el.scrollIntoView({ block: "nearest", behavior: "smooth" });
    }
  }, [safeSelectedIndex, commandPaletteOpen, combinedResults.length]);

  if (!commandPaletteOpen) return null;

  const handleSelectItem = (item: SearchResultItem) => {
    if (item.type === "action") {
      switch (item.id) {
        case "action-new-course":
          router.push("/app/courses/new");
          break;
        case "action-practice-hub":
          router.push("/app/practice");
          break;
        case "action-exams":
          router.push("/app/exams");
          break;
        case "action-review-fsrs":
          setActiveMode("review");
          router.push("/app/review");
          break;
        case "action-toggle-theme":
          toggleDarkMode();
          break;
        case "action-open-shortcuts":
          if (typeof window !== "undefined") {
            window.dispatchEvent(new CustomEvent("studxy:open-shortcuts"));
          }
          break;
      }
      setCommandPaletteOpen(false);
      return;
    }

    const inAppShell = pathname?.startsWith("/app");

    if (item.type === "course") {
      setActiveCourseId(item.id);
      if (inAppShell) {
        router.push(`/app/courses/${item.id}`);
      } else {
        setActiveMode("dashboard");
      }
    } else if (item.type === "lesson") {
      if (item.courseId) setActiveCourseId(item.courseId);
      setActiveLesson(item.id);
      setActiveMode("reading");
      if (inAppShell) {
        router.push(
          item.courseId
            ? `/app/courses/${item.courseId}/learn?lesson=${item.id}`
            : "/app/courses"
        );
      }
    } else if (item.type === "document") {
      if (item.courseId) setActiveCourseId(item.courseId);
      setActiveDocument(item.id);
      setActiveMode("reading");
      if (inAppShell) {
        router.push(
          item.courseId
            ? `/app/courses/${item.courseId}/learn?doc=${item.id}`
            : `/app/documents?doc=${item.id}`
        );
      }
    } else if (item.type === "notebook") {
      setActiveMode("split");
      if (inAppShell) {
        router.push(
          item.courseId
            ? `/app/courses/${item.courseId}/learn?notes=1`
            : `/app/notes?notebook=${item.id}`
        );
      }
    } else if (item.type === "flashcard") {
      setActiveMode("review");
      if (inAppShell) {
        router.push("/app/review");
      }
    } else if (item.type === "tag") {
      setSelectedTagFilterId(item.id);
      setActiveMode("review");
      if (inAppShell) {
        router.push(`/app/review?tag=${encodeURIComponent(item.id)}`);
      }
    }
    setCommandPaletteOpen(false);
  };

  const handleInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (combinedResults.length === 0) return;
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setSelectedIndex((prev) => (prev + 1) % combinedResults.length);
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setSelectedIndex((prev) => (prev - 1 + combinedResults.length) % combinedResults.length);
    } else if (e.key === "Enter") {
      e.preventDefault();
      const target = combinedResults[safeSelectedIndex] || combinedResults[0];
      if (target) handleSelectItem(target);
    }
  };

  const handleQuickCaptureFlashcard = async () => {
    if (!quickCaptureText.trim()) return;
    setCaptureErrorMsg(null);
    const trimmed = quickCaptureText.trim();
    try {
      const resp = await fetch("/api/v1/flashcards", {
        method: "POST",
        headers: getClientRequestHeaders({ "Content-Type": "application/json" }),
        body: JSON.stringify({
          cardType: "cloze",
          frontText: trimmed.includes("{{c1::")
            ? trimmed
            : `Ghi nhớ nhanh: {{c1::${trimmed}}}`,
          backText: "Được tạo nhanh từ bảng lệnh tìm kiếm (Ctrl+K).",
          tagNames: ["OS-Paging"],
        }),
      });
      if (resp.ok) {
        setQuickCaptureText("");
        setCaptureSavedMsg("Đã lưu thẻ ghi nhớ nhanh vào hàng đợi ôn tập!");
        onQuickCaptureCreated?.();
        setTimeout(() => setCaptureSavedMsg(null), 2500);
      } else {
        const err = await resp.json().catch(() => ({}));
        setCaptureErrorMsg(err.error || `Không thể lưu thẻ ghi nhớ (mã lỗi ${resp.status}).`);
        setTimeout(() => setCaptureErrorMsg(null), 3500);
      }
    } catch {
      setCaptureErrorMsg("Lỗi kết nối mạng khi lưu thẻ. Bạn hãy thử lại nhé.");
      setTimeout(() => setCaptureErrorMsg(null), 3500);
    }
  };

  return (
    <div
      data-testid="command-palette-modal"
      role="dialog"
      aria-modal="true"
      aria-label="Bảng lệnh tìm kiếm nhanh"
      className="fixed inset-0 z-50 bg-black/50 backdrop-blur-xs flex items-start justify-center pt-16 sm:pt-20 px-4"
      onClick={() => setCommandPaletteOpen(false)}
    >
      <div
        className="w-full max-w-2xl rounded-xl bg-[var(--surface-card)] border border-[var(--border-calm)] shadow-2xl overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="p-3.5 border-b border-[var(--border-calm)] flex items-center gap-3">
          <Search className="w-4 h-4 text-[var(--text-tertiary)] shrink-0" />
          <input
            data-testid="command-palette-input"
            type="text"
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleInputKeyDown}
            placeholder="Tìm nhanh khóa học, bài giảng, tài liệu PDF, sổ tay hoặc thẻ ghi nhớ..."
            className="w-full bg-transparent text-sm text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none"
          />
          <button
            type="button"
            onClick={() => setCommandPaletteOpen(false)}
            className="px-2 py-0.5 text-xs rounded border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer transition"
          >
            Esc
          </button>
        </div>

        {/* Quick Capture Bar */}
        <div className="px-4 py-2.5 bg-[var(--surface-elevated)]/80 border-b border-[var(--border-calm)] flex items-center gap-2">
          <span className="text-xs font-semibold text-[#5E51E8] dark:text-[#818CF8] shrink-0 flex items-center gap-1">
            <Zap className="w-3.5 h-3.5" />
            <span>Tạo thẻ nhanh:</span>
          </span>
          <input
            data-testid="quick-capture-input"
            type="text"
            value={quickCaptureText}
            onChange={(e) => setQuickCaptureText(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                void handleQuickCaptureFlashcard();
              }
            }}
            placeholder="Nhập ý cần nhớ để đưa ngay vào mục Ôn tập..."
            className="flex-1 text-xs bg-[var(--surface-card)] text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] px-2.5 py-1.5 rounded-md border border-[var(--border-calm)] focus:outline-none focus:border-[#5E51E8] dark:focus:border-[#818CF8]"
          />
          <button
            type="button"
            data-testid="quick-capture-save-btn"
            onClick={() => void handleQuickCaptureFlashcard()}
            className="inline-flex items-center gap-1 px-3 py-1.5 rounded-md bg-[#5E51E8] hover:bg-[#4F46E5] dark:bg-[#818CF8] dark:hover:bg-[#6366F1] text-white dark:text-[#0F0F12] text-xs font-medium shrink-0 cursor-pointer transition"
          >
            <Plus className="w-3.5 h-3.5" />
            <span>Lưu thẻ</span>
          </button>
        </div>

        {captureSavedMsg && (
          <div className="px-4 py-2 bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 text-xs font-semibold flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5 shrink-0" />
            <span>{captureSavedMsg}</span>
          </div>
        )}

        {captureErrorMsg && (
          <div
            data-testid="quick-capture-error-alert"
            className="px-4 py-2 bg-rose-500/15 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-center gap-1.5"
          >
            <AlertCircle className="w-3.5 h-3.5 shrink-0" />
            <span>{captureErrorMsg}</span>
          </div>
        )}

        <div
          role="listbox"
          aria-label="Kết quả tìm kiếm và hành động nhanh"
          className="max-h-80 overflow-y-auto divide-y divide-[var(--border-calm)]/60"
        >
          {combinedResults.length === 0 && loading ? (
            <div className="p-6 text-center text-xs text-[var(--text-tertiary)]">Đang tìm kiếm...</div>
          ) : combinedResults.length === 0 ? (
            <div className="p-6 text-center text-xs text-[var(--text-tertiary)]">
              Không tìm thấy kết quả phù hợp cho &ldquo;{query}&rdquo;.
            </div>
          ) : (
            combinedResults.map((item, idx) => {
              const active = idx === safeSelectedIndex;
              return (
                <button
                  key={`${item.type}-${item.id}`}
                  ref={(el) => {
                    resultRefs.current[idx] = el;
                  }}
                  type="button"
                  role="option"
                  aria-selected={active}
                  data-testid={`cmd-result-${item.type}`}
                  onMouseEnter={() => setSelectedIndex(idx)}
                  onClick={() => handleSelectItem(item)}
                  className={`w-full text-left px-4 py-3 transition flex items-center justify-between gap-3 cursor-pointer ${
                    active
                      ? "bg-[#EEF0FD]/80 dark:bg-[#1E1B4B]/70"
                      : "hover:bg-[var(--surface-elevated)]/70"
                  }`}
                >
                  <div className="flex items-start gap-2.5 min-w-0">
                    <div className="mt-0.5">
                      <TypeIcon type={item.type} />
                    </div>
                    <div className="min-w-0">
                      <div className="text-xs font-semibold text-[var(--text-primary)] truncate">
                        {item.title}
                      </div>
                      <div className="text-[11px] text-[#5E51E8] dark:text-[#818CF8] font-medium truncate">
                        {item.subtitle}
                      </div>
                      {item.snippet && (
                        <div className="text-[11px] text-[var(--text-tertiary)] truncate mt-0.5">
                          {item.snippet}
                        </div>
                      )}
                    </div>
                  </div>
                  <span className="text-[10px] font-medium px-2 py-0.5 rounded-md border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-secondary)] shrink-0">
                    {TYPE_LABELS[item.type] ?? item.type}
                  </span>
                </button>
              );
            })
          )}
        </div>

        {loading && combinedResults.length > 0 && (
          <div className="px-4 py-1.5 border-t border-[var(--border-calm)]/60 text-center text-[11px] text-[var(--text-tertiary)]">
            Đang tìm thêm kết quả...
          </div>
        )}

        <div className="px-4 py-2 bg-[var(--surface-elevated)] border-t border-[var(--border-calm)] flex flex-wrap items-center justify-between gap-2 text-[11px] text-[var(--text-tertiary)]">
          <span>Hỗ trợ tìm tiếng Việt có dấu và không dấu (ví dụ: &ldquo;tai chinh&rdquo;, &ldquo;ielts&rdquo;)</span>
          <span className="font-medium text-[var(--text-secondary)]">↑↓ Di chuyển · Enter Mở · Esc Đóng</span>
        </div>
      </div>
    </div>
  );
}
