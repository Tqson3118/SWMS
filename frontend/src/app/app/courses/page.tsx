"use client";

import React, { useCallback, useEffect, useMemo, useRef, useState, Suspense } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  Clock,
  ExternalLink,
  Eye,
  Filter,
  Headphones,
  Layers,
  LayoutGrid,
  List,
  MoreVertical,
  Pin,
  Play,
  Plus,
  RefreshCw,
  Search,
  Sparkles,
  Trash2,
  X,
  Archive,
  ArchiveRestore,
  Copy,
  Brain,
  Bookmark,
  AlertTriangle,
  Flame,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import {
  fetchCoursesPage,
  fetchDashboardCached,
  getCachedCoursesSnapshot,
  invalidateDashboardCache,
  setCachedCoursesSnapshot,
} from "@/lib/api-client";
import {
  Button,
  Card,
  ProgressBar,
  EmptyState,
  ShowMoreFooter,
} from "@/components/ui";
import {
  COURSE_DOMAINS,
  classifyCourseDomain,
  deduplicateCourses,
  detectPersonalDuplicates,
  extractCourseSkillTags,
  getCourseChapterCount,
  getCourseId,
  getCourseLessonCount,
  getCourseProgress,
  getCourseTotalMinutes,
  getCourseVisualTheme,
  normalizeCourseSummary,
  normalizeVi,
  sortCoursesSmartly,
  type CourseDomainId,
  type CourseSummary,
} from "@/lib/course-helpers";
import { LearningRoadmapsShowcase } from "@/components/dashboard/LearningRoadmapsShowcase";
import { CourseCardGraphicBanner } from "@/components/dashboard/CourseCardGraphicBanner";
import { CourseQuickPreviewDrawer } from "@/components/courses/CourseQuickPreviewDrawer";
import { CourseDuplicateCleanupModal } from "@/components/courses/CourseDuplicateCleanupModal";

type CollectionTab = "all" | "in_progress" | "pinned" | "mine" | "archived";
type LevelFilter = "all" | "foundation" | "intermediate" | "advanced";
type SortOption = "recommended" | "progress_desc" | "recent" | "title_asc" | "duration_asc";
type ViewLayout = "grid" | "list";

const INITIAL_GRID_COUNT = 8;
const INITIAL_LIST_COUNT = 6;
const LOAD_MORE_STEP = 8;

function CoursesCatalogContent() {
  const searchParams = useSearchParams();
  const { activeUserHeaderId } = useWorkspaceStore();

  const [rawCourses, setRawCourses] = useState<CourseSummary[]>([]);
  const [totalServerCount, setTotalServerCount] = useState<number>(0);
  const [isHydratingRemaining, setIsHydratingRemaining] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [visibleCount, setVisibleCount] = useState(INITIAL_GRID_COUNT);
  const remainingFetchRef = useRef<Promise<CourseSummary[]> | null>(null);
  const ensureAllLoadedRef = useRef<(() => Promise<void>) | null>(null);

  // Tabs & filters
  const [activeTab, setActiveTab] = useState<CollectionTab>("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDomain, setSelectedDomain] = useState<CourseDomainId>("all");
  const [levelFilter, setLevelFilter] = useState<LevelFilter>("all");
  const [sortBy, setSortBy] = useState<SortOption>("recommended");

  // Feature filters
  const [filterHasLab, setFilterHasLab] = useState(false);
  const [filterHasAudio, setFilterHasAudio] = useState(false);
  const [filterHasFlashcards, setFilterHasFlashcards] = useState(false);

  // View mode (persisted in localStorage)
  const [viewLayout, setViewLayout] = useState<ViewLayout>("grid");

  // Action states & Modals
  const [previewCourse, setPreviewCourse] = useState<CourseSummary | null>(null);
  const [isCleanupModalOpen, setIsCleanupModalOpen] = useState(false);
  const [activeMenuCourseId, setActiveMenuCourseId] = useState<string | null>(null);
  const [courseToDelete, setCourseToDelete] = useState<CourseSummary | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [toast, setToast] = useState<{ text: string; tone: "success" | "error" } | null>(null);
  const searchInputRef = useRef<HTMLInputElement | null>(null);

  // Focus search input on `/` shortcut when not typing in a field
  useEffect(() => {
    const handleGlobalKeyDown = (e: KeyboardEvent) => {
      if (e.key !== "/" || e.ctrlKey || e.metaKey || e.altKey || e.shiftKey) return;
      const activeEl = document.activeElement as HTMLElement | null;
      const tagName = activeEl?.tagName?.toUpperCase();
      if (
        tagName === "INPUT" ||
        tagName === "TEXTAREA" ||
        tagName === "SELECT" ||
        activeEl?.isContentEditable
      ) {
        return;
      }
      e.preventDefault();
      searchInputRef.current?.focus();
    };
    document.addEventListener("keydown", handleGlobalKeyDown);
    return () => document.removeEventListener("keydown", handleGlobalKeyDown);
  }, []);

  // Read view preference from localStorage
  useEffect(() => {
    if (typeof window !== "undefined") {
      try {
        const saved = window.localStorage.getItem("studxy_courses_layout") as ViewLayout | null;
        if (saved === "grid" || saved === "list") {
          setViewLayout(saved);
        }
      } catch {
        // ignore
      }
    }
  }, []);

  const handleToggleLayout = (layout: ViewLayout) => {
    setViewLayout(layout);
    setVisibleCount(layout === "grid" ? INITIAL_GRID_COUNT : INITIAL_LIST_COUNT);
    if (typeof window !== "undefined") {
      try {
        window.localStorage.setItem("studxy_courses_layout", layout);
      } catch {
        // ignore
      }
    }
  };

  // Sync tab from URL query if provided
  useEffect(() => {
    const tabParam = searchParams.get("tab");
    if (
      tabParam === "archived" ||
      tabParam === "pinned" ||
      tabParam === "in_progress" ||
      tabParam === "mine" ||
      tabParam === "all"
    ) {
      setActiveTab(tabParam as CollectionTab);
    }
  }, [searchParams]);

  // Toast timer
  useEffect(() => {
    if (!toast) return;
    const timer = setTimeout(() => {
      setToast(null);
    }, 4000);
    return () => clearTimeout(timer);
  }, [toast]);

  // Close 3-dot course menu on outside click or Escape key
  useEffect(() => {
    if (!activeMenuCourseId) return;

    const handlePointerDown = (e: MouseEvent) => {
      const target = e.target as HTMLElement | null;
      if (!target?.closest(`[data-course-menu="${activeMenuCourseId}"]`)) {
        setActiveMenuCourseId(null);
      }
    };

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setActiveMenuCourseId(null);
      }
    };

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [activeMenuCourseId]);

  const fetchCourses = useCallback(async () => {
    setFetchError(null);
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
    const headers = getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );

    const cachedSnap = getCachedCoursesSnapshot<CourseSummary>(effectiveUserId);
    if (cachedSnap && cachedSnap.items.length > 0) {
      setRawCourses(cachedSnap.items.map(normalizeCourseSummary));
      setTotalServerCount(cachedSnap.totalCount);
      setIsLoading(false);
    } else {
      setIsLoading(true);
    }

    try {
      const page1 = await fetchCoursesPage<CourseSummary>({
        limit: INITIAL_GRID_COUNT,
        offset: 0,
        headers,
      });
      const firstNorm = page1.items.map(normalizeCourseSummary);
      setTotalServerCount(page1.totalCount);
      setRawCourses((prev) => {
        // Preserve already-hydrated items during SWR revalidation unless user count shrunk
        if (!page1.hasMore || prev.length <= firstNorm.length) {
          return firstNorm;
        }
        const byId = new Map(firstNorm.map((c) => [c.id, c]));
        const rest = prev.filter((c) => !byId.has(c.id));
        return [...firstNorm, ...rest];
      });
      setIsLoading(false);

      if (!page1.hasMore) {
        setCachedCoursesSnapshot(firstNorm, page1.totalCount, effectiveUserId);
        return;
      }

      const loadRemaining = () => {
        if (remainingFetchRef.current) return remainingFetchRef.current;
        setIsHydratingRemaining(true);
        const p = (async () => {
          try {
            const restPage = await fetchCoursesPage<CourseSummary>({
              limit: 100,
              offset: INITIAL_GRID_COUNT,
              headers,
            });
            const restNorm = restPage.items.map(normalizeCourseSummary);
            const mergedMap = new Map<string, CourseSummary>();
            for (const c of [...firstNorm, ...restNorm]) {
              if (!mergedMap.has(c.id)) mergedMap.set(c.id, c);
            }
            const merged = Array.from(mergedMap.values());
            setRawCourses(merged);
            setTotalServerCount(Math.max(page1.totalCount, merged.length));
            setCachedCoursesSnapshot(merged, Math.max(page1.totalCount, merged.length), effectiveUserId);
            return merged;
          } finally {
            setIsHydratingRemaining(false);
            remainingFetchRef.current = null;
          }
        })();
        remainingFetchRef.current = p;
        return p;
      };

      ensureAllLoadedRef.current = async () => {
        await loadRemaining();
      };

      if (typeof window !== "undefined" && "requestIdleCallback" in window) {
        (window as unknown as { requestIdleCallback: (cb: () => void, opts?: { timeout: number }) => number }).requestIdleCallback(
          () => {
            void loadRemaining();
          },
          { timeout: 300 }
        );
      } else {
        setTimeout(() => {
          void loadRemaining();
        }, 80);
      }
    } catch {
      try {
        const data = await fetchDashboardCached({ compact: true, headers });
        if (data && Array.isArray(data.courses)) {
          const norm = data.courses.map(normalizeCourseSummary);
          setRawCourses(norm);
          setTotalServerCount(norm.length);
          setCachedCoursesSnapshot(norm, norm.length, effectiveUserId);
        } else if (!cachedSnap) {
          setFetchError("Không tải được danh sách khóa học.");
        }
      } catch {
        if (!cachedSnap) {
          setFetchError("Không thể kết nối tới máy chủ.");
        }
      } finally {
        setIsLoading(false);
      }
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  // Reset progressive visible slice and hydrate full catalog when filters/search are active
  useEffect(() => {
    setVisibleCount(viewLayout === "grid" ? INITIAL_GRID_COUNT : INITIAL_LIST_COUNT);
    const hasFilterActive =
      activeTab !== "all" ||
      Boolean(searchQuery.trim()) ||
      selectedDomain !== "all" ||
      levelFilter !== "all" ||
      filterHasLab ||
      filterHasAudio ||
      filterHasFlashcards;
    if (hasFilterActive) {
      void ensureAllLoadedRef.current?.();
    }
  }, [
    activeTab,
    searchQuery,
    selectedDomain,
    levelFilter,
    sortBy,
    filterHasLab,
    filterHasAudio,
    filterHasFlashcards,
    viewLayout,
  ]);

  // Detect personal duplicates for the header cleanup trigger
  const duplicateReport = useMemo(() => detectPersonalDuplicates(rawCourses), [rawCourses]);

  // Tab counts
  const tabCounts = useMemo(() => {
    return {
      all: rawCourses.filter((c) => !c.isArchived).length,
      in_progress: rawCourses.filter(
        (c) => !c.isArchived && Number(c.completedLessons ?? 0) > 0 && getCourseProgress(c) < 100
      ).length,
      pinned: rawCourses.filter((c) => !c.isArchived && Boolean(c.isPinned)).length,
      mine: rawCourses.filter((c) => !c.isArchived && !c.isSystemCourse).length,
      archived: rawCourses.filter((c) => Boolean(c.isArchived)).length,
    };
  }, [rawCourses]);

  // Filter pipeline
  const filteredCourses = useMemo(() => {
    const q = normalizeVi(searchQuery);

    return rawCourses.filter((course) => {
      // 1. Collection Tab Filter
      if (activeTab === "all" && course.isArchived) return false;
      if (activeTab === "in_progress") {
        if (course.isArchived) return false;
        const prog = getCourseProgress(course);
        const comp = Number(course.completedLessons ?? 0);
        if (!(comp > 0 && prog < 100)) return false;
      }
      if (activeTab === "pinned") {
        if (course.isArchived || !course.isPinned) return false;
      }
      if (activeTab === "mine") {
        if (course.isArchived || course.isSystemCourse) return false;
      }
      if (activeTab === "archived") {
        if (!course.isArchived) return false;
      }

      // 2. Search Box Filter (Vietnamese unaccented supported)
      if (q) {
        const hay = normalizeVi(
          `${course.title || ""} ${course.description || ""} ${course.category || ""} ${course.targetAudience || ""}`
        );
        if (!hay.includes(q)) return false;
      }

      // 3. Category Filter
      if (selectedDomain !== "all") {
        const domain = classifyCourseDomain(course);
        if (domain.id !== selectedDomain) return false;
      }

      // 4. Level Filter
      if (levelFilter !== "all") {
        if (course.level !== levelFilter) {
          const theme = getCourseVisualTheme(course);
          if (levelFilter === "foundation" && !theme.levelLabel.includes("Nền tảng")) return false;
          if (levelFilter === "advanced" && !theme.levelLabel.includes("Chuyên sâu")) return false;
          if (levelFilter === "intermediate" && !theme.levelLabel.includes("Trung cấp")) return false;
        }
      }

      // 5. Feature filters
      if (filterHasLab && Number(course.practiceTotalActivities ?? 0) <= 0) return false;
      if (filterHasAudio && !course.hasAudio) return false;
      if (filterHasFlashcards && Number(course.flashcardCount ?? 0) <= 0) return false;

      return true;
    });
  }, [
    rawCourses,
    activeTab,
    searchQuery,
    selectedDomain,
    levelFilter,
    filterHasLab,
    filterHasAudio,
    filterHasFlashcards,
  ]);

  // Sorting
  const sortedCourses = useMemo(() => {
    if (sortBy === "recommended") {
      return sortCoursesSmartly(filteredCourses);
    }

    return [...filteredCourses].sort((a, b) => {
      // In custom sorts, keep pinned courses on top within same tab if not archived tab
      if (activeTab !== "archived") {
        const aPinned = Boolean(a.isPinned);
        const bPinned = Boolean(b.isPinned);
        if (aPinned !== bPinned) return aPinned ? -1 : 1;
      }

      if (sortBy === "recent") {
        const tsA = Date.parse(a.updatedAt || a.createdAt || "") || 0;
        const tsB = Date.parse(b.updatedAt || b.createdAt || "") || 0;
        if (tsB !== tsA) return tsB - tsA;
        return getCourseProgress(b) - getCourseProgress(a);
      }

      if (sortBy === "progress_desc") {
        const diff = getCourseProgress(b) - getCourseProgress(a);
        if (diff !== 0) return diff;
        return Number(b.completedLessons ?? 0) - Number(a.completedLessons ?? 0);
      }

      if (sortBy === "title_asc") {
        return (a.title || "").localeCompare(b.title || "", "vi");
      }

      if (sortBy === "duration_asc") {
        return getCourseTotalMinutes(a) - getCourseTotalMinutes(b);
      }

      return 0;
    });
  }, [filteredCourses, sortBy, activeTab]);

  const visibleCourses = useMemo(
    () => sortedCourses.slice(0, visibleCount),
    [sortedCourses, visibleCount]
  );

  const isUnfilteredAllTab =
    activeTab === "all" &&
    !searchQuery.trim() &&
    selectedDomain === "all" &&
    levelFilter === "all" &&
    !filterHasLab &&
    !filterHasAudio &&
    !filterHasFlashcards;
  const effectiveTotalCount = isUnfilteredAllTab
    ? Math.max(sortedCourses.length, totalServerCount)
    : sortedCourses.length;

  // Actions
  const handleTogglePin = async (course: CourseSummary) => {
    const nextVal = !course.isPinned;
    setRawCourses((prev) =>
      prev.map((c) => (c.id === course.id ? { ...c, isPinned: nextVal } : c))
    );
    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${course.id}/preferences`, {
        method: "PUT",
        headers,
        body: JSON.stringify({ isPinned: nextVal }),
      });
      if (!resp.ok) throw new Error();
      invalidateDashboardCache();
      setToast({
        text: nextVal ? `Đã ghim "${course.title}".` : `Đã bỏ ghim "${course.title}".`,
        tone: "success",
      });
    } catch {
      // Revert on error
      setRawCourses((prev) =>
        prev.map((c) => (c.id === course.id ? { ...c, isPinned: course.isPinned } : c))
      );
      setToast({
        text: "Không thể cập nhật trạng thái ghim.",
        tone: "error",
      });
    }
  };

  const handleToggleArchive = async (course: CourseSummary) => {
    const nextVal = !course.isArchived;
    setRawCourses((prev) =>
      prev.map((c) => (c.id === course.id ? { ...c, isArchived: nextVal } : c))
    );
    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${course.id}/preferences`, {
        method: "PUT",
        headers,
        body: JSON.stringify({ isArchived: nextVal }),
      });
      if (!resp.ok) throw new Error();
      invalidateDashboardCache();
      setToast({
        text: nextVal
          ? `Đã chuyển "${course.title}" vào mục Lưu trữ.`
          : `Đã khôi phục "${course.title}" về thư viện chính.`,
        tone: "success",
      });
    } catch {
      // Revert on error
      setRawCourses((prev) =>
        prev.map((c) => (c.id === course.id ? { ...c, isArchived: course.isArchived } : c))
      );
      setToast({
        text: "Không thể cập nhật trạng thái lưu trữ.",
        tone: "error",
      });
    }
  };

  const handleCloneCourse = async (course: CourseSummary) => {
    try {
      const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${course.id}/clone`, {
        method: "POST",
        headers,
      });
      if (!resp.ok) {
        const err = await resp.text().catch(() => "");
        throw new Error(err || `HTTP ${resp.status}`);
      }
      const cloned = await resp.json();
      invalidateDashboardCache();
      await fetchCourses();
      setToast({
        text: `Đã nhân bản "${cloned.title || course.title}" vào kho cá nhân.`,
        tone: "success",
      });
    } catch (err) {
      setToast({
        text: `Nhân bản thất bại: ${err instanceof Error ? err.message : "Lỗi kết nối"}`,
        tone: "error",
      });
    }
  };

  const handleDeleteCourse = async () => {
    if (!courseToDelete || !courseToDelete.canDelete || courseToDelete.isSystemCourse) return;
    setIsDeleting(true);
    try {
      const headers = getClientRequestHeaders();
      const resp = await fetch(`/api/v1/courses/${courseToDelete.id}`, {
        method: "DELETE",
        headers,
      });
      if (!resp.ok) {
        const err = await resp.text().catch(() => "");
        throw new Error(err || `HTTP ${resp.status}`);
      }
      invalidateDashboardCache();
      const title = courseToDelete.title;
      setCourseToDelete(null);
      await fetchCourses();
      setToast({
        text: `Đã xóa vĩnh viễn khóa học "${title}".`,
        tone: "success",
      });
    } catch (err) {
      setToast({
        text: `Xóa thất bại: ${err instanceof Error ? err.message : "Lỗi kết nối"}`,
        tone: "error",
      });
    } finally {
      setIsDeleting(false);
    }
  };

  const activeFilterCount =
    (searchQuery.trim() ? 1 : 0) +
    (selectedDomain !== "all" ? 1 : 0) +
    (levelFilter !== "all" ? 1 : 0) +
    (filterHasLab ? 1 : 0) +
    (filterHasAudio ? 1 : 0) +
    (filterHasFlashcards ? 1 : 0);

  const renderCourseMenu = (course: CourseSummary) => {
    const id = course.id;
    const isMenuOpen = activeMenuCourseId === id;

    return (
      <div className="relative" data-course-menu={id}>
        <button
          type="button"
          onClick={() => setActiveMenuCourseId(isMenuOpen ? null : id)}
          className="p-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:bg-[var(--surface-card)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer"
          title="Tùy chọn khác"
          aria-label="Tùy chọn khóa học"
          aria-expanded={isMenuOpen}
          aria-haspopup="menu"
        >
          <MoreVertical className="w-4 h-4" />
        </button>

        {isMenuOpen && (
          <div
            role="menu"
            className="absolute right-0 bottom-full mb-1.5 w-48 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] shadow-xl py-1 z-30 text-xs font-medium animate-in zoom-in-95 duration-150"
          >
            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setActiveMenuCourseId(null);
                setPreviewCourse(course);
              }}
              className="w-full px-3 py-2 text-left hover:bg-[var(--surface-elevated)] flex items-center gap-2 text-[var(--text-primary)] cursor-pointer"
            >
              <Eye className="w-3.5 h-3.5 text-[#5E51E8]" />
              <span>Xem nhanh</span>
            </button>

            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setActiveMenuCourseId(null);
                handleTogglePin(course);
              }}
              className="w-full px-3 py-2 text-left hover:bg-[var(--surface-elevated)] flex items-center gap-2 text-[var(--text-primary)] cursor-pointer"
            >
              <Pin className="w-3.5 h-3.5 text-amber-500" />
              <span>{course.isPinned ? "Bỏ ghim" : "Ghim khóa học"}</span>
            </button>

            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setActiveMenuCourseId(null);
                handleToggleArchive(course);
              }}
              className="w-full px-3 py-2 text-left hover:bg-[var(--surface-elevated)] flex items-center gap-2 text-[var(--text-primary)] cursor-pointer"
            >
              {course.isArchived ? (
                <>
                  <ArchiveRestore className="w-3.5 h-3.5 text-emerald-500" />
                  <span>Khôi phục</span>
                </>
              ) : (
                <>
                  <Archive className="w-3.5 h-3.5 text-[var(--text-tertiary)]" />
                  <span>Lưu trữ</span>
                </>
              )}
            </button>

            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setActiveMenuCourseId(null);
                handleCloneCourse(course);
              }}
              className="w-full px-3 py-2 text-left hover:bg-[var(--surface-elevated)] flex items-center gap-2 text-[var(--text-primary)] cursor-pointer"
            >
              <Copy className="w-3.5 h-3.5 text-teal-500" />
              <span>Nhân bản cá nhân</span>
            </button>

            {course.canDelete && !course.isSystemCourse && (
              <div className="border-t border-[var(--border-calm)] mt-1 pt-1">
                <button
                  type="button"
                  role="menuitem"
                  onClick={() => {
                    setActiveMenuCourseId(null);
                    setCourseToDelete(course);
                  }}
                  className="w-full px-3 py-2 text-left hover:bg-red-500/10 text-red-600 dark:text-red-400 flex items-center gap-2 cursor-pointer"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                  <span>Xóa khóa học</span>
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh] space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div className="space-y-2">
            <div className="flex items-center gap-3">
              <div className="h-8 w-32 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
              <div className="h-6 w-16 rounded-full bg-[var(--surface-elevated)] swms-skeleton" />
            </div>
            <div className="h-4 w-72 max-w-full rounded bg-[var(--surface-elevated)] swms-skeleton" />
          </div>
          <div className="h-9 w-36 rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
        </div>

        <div className="flex items-center gap-2 overflow-hidden pb-1 border-b border-[var(--border-calm)]">
          {[0, 1, 2, 3, 4].map((i) => (
            <div key={i} className="h-9 w-24 rounded-t-lg bg-[var(--surface-elevated)] swms-skeleton shrink-0" />
          ))}
        </div>

        <div className="space-y-3">
          <div className="flex flex-col sm:flex-row gap-2.5 justify-between">
            <div className="h-9 w-full max-w-lg rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
            <div className="flex items-center gap-2">
              <div className="h-9 w-40 rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
              <div className="h-9 w-16 rounded-xl bg-[var(--surface-elevated)] swms-skeleton" />
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            {[0, 1, 2, 3, 4].map((i) => (
              <div key={i} className="h-8 w-28 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[0, 1, 2, 3, 4, 5].map((i) => (
            <div
              key={i}
              className="rounded-2xl border border-[var(--border-calm)] bg-[var(--surface-card)] overflow-hidden flex flex-col justify-between"
            >
              <div className="h-24 w-full bg-[var(--surface-elevated)] swms-skeleton" />
              <div className="p-5 space-y-4">
                <div className="flex gap-1.5">
                  <div className="h-5 w-16 rounded-md bg-[var(--surface-elevated)] swms-skeleton" />
                  <div className="h-5 w-14 rounded-md bg-[var(--surface-elevated)] swms-skeleton" />
                </div>
                <div className="space-y-2">
                  <div className="h-5 w-4/5 rounded bg-[var(--surface-elevated)] swms-skeleton" />
                  <div className="h-4 w-full rounded bg-[var(--surface-elevated)] swms-skeleton" />
                  <div className="h-4 w-2/3 rounded bg-[var(--surface-elevated)] swms-skeleton" />
                </div>
                <div className="pt-3 border-t border-[var(--border-calm)] space-y-3">
                  <div className="h-3.5 w-2/3 rounded bg-[var(--surface-elevated)] swms-skeleton" />
                  <div className="h-2 w-full rounded-full bg-[var(--surface-elevated)] swms-skeleton" />
                  <div className="flex items-center gap-2">
                    <div className="h-8 flex-1 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
                    <div className="h-8 w-9 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
                    <div className="h-8 w-9 rounded-lg bg-[var(--surface-elevated)] swms-skeleton" />
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Toast Notification */}
      {toast && (
        <div
          role="status"
          aria-live="polite"
          className={`fixed bottom-20 md:bottom-6 right-4 sm:right-6 z-50 max-w-[calc(100vw-2rem)] sm:max-w-sm rounded-xl border px-4 py-2.5 shadow-xl backdrop-blur-md text-xs font-semibold flex items-center gap-2.5 animate-in slide-in-from-bottom duration-200 ${
            toast.tone === "error"
              ? "border-rose-500/40 bg-rose-50/95 text-rose-900 dark:bg-rose-950/95 dark:text-rose-100"
              : "border-emerald-500/40 bg-emerald-50/95 text-emerald-900 dark:bg-emerald-950/95 dark:text-emerald-100"
          }`}
        >
          {toast.tone === "error" ? (
            <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
          ) : (
            <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
          )}
          <span className="break-words">{toast.text}</span>
          <button
            type="button"
            onClick={() => setToast(null)}
            aria-label="Đóng thông báo"
            className="ml-auto opacity-60 hover:opacity-100 cursor-pointer shrink-0"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      )}

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">
              Khóa học
            </h1>
            <span className="text-xs font-semibold px-2 py-0.5 rounded-full bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)]">
              {Math.max(tabCounts.all, totalServerCount)} khóa
            </span>
          </div>
          <p className="text-xs text-[var(--text-secondary)] mt-1">
            Quản lý lộ trình học tập, phòng thực hành tương tác, bài kiểm tra và ôn tập thẻ ghi nhớ FSRS.
          </p>
        </div>

        <div className="flex items-center gap-2.5 flex-wrap shrink-0">
          {duplicateReport.totalRemovableCount > 0 && (
            <button
              type="button"
              onClick={() => setIsCleanupModalOpen(true)}
              className="inline-flex items-center gap-1.5 min-h-9 py-1.5 px-3 rounded-xl border border-amber-300 dark:border-amber-800/80 bg-amber-50/70 dark:bg-amber-950/30 text-amber-800 dark:text-amber-300 text-xs font-semibold whitespace-nowrap hover:bg-amber-100 transition shadow-2xs cursor-pointer"
            >
              <Trash2 className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400 shrink-0" />
              <span>Dọn bản trùng</span>
              <span className="w-5 h-5 rounded-full bg-amber-500 text-white text-[10px] font-bold flex items-center justify-center ml-0.5">
                {duplicateReport.totalRemovableCount}
              </span>
            </button>
          )}

          <Link href="/app/courses/new" className="shrink-0">
            <Button
              variant="primary"
              leftIcon={<Plus className="w-4 h-4" />}
              aria-label="Tạo khóa học mới"
              className="whitespace-nowrap bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
            >
              <span>Tạo khóa học</span>
            </Button>
          </Link>
        </div>
      </div>

      {fetchError && (
        <div className="mb-6 rounded-xl border border-red-500/30 bg-red-500/5 px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-xs font-medium text-red-600 dark:text-red-400">
            <AlertCircle className="w-4 h-4 shrink-0" />
            <span>{fetchError}</span>
          </div>
          <Button
            variant="secondary"
            size="sm"
            onClick={fetchCourses}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          >
            Thử lại
          </Button>
        </div>
      )}

      {/* Collection Tabs */}
      <div className="flex items-center gap-1.5 max-w-full overflow-x-auto no-scrollbar pb-1 mb-5 border-b border-[var(--border-calm)]">
        {(
          [
            { id: "all", label: "Tất cả", count: tabCounts.all },
            { id: "in_progress", label: "Đang học", count: tabCounts.in_progress },
            { id: "pinned", label: "Đã ghim", count: tabCounts.pinned },
            { id: "mine", label: "Của tôi", count: tabCounts.mine },
            { id: "archived", label: "Đã lưu trữ", count: tabCounts.archived },
          ] as { id: CollectionTab; label: string; count: number }[]
        ).map((tab) => {
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => setActiveTab(tab.id)}
              className={`inline-flex items-center gap-1.5 h-9 px-3.5 rounded-t-lg text-xs font-semibold whitespace-nowrap transition cursor-pointer border-b-2 -mb-px ${
                isActive
                  ? "border-[#5E51E8] text-[#5E51E8] dark:text-[#818CF8] bg-[var(--surface-elevated)]/40"
                  : "border-transparent text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:bg-[var(--surface-elevated)]/20"
              }`}
            >
              <span>{tab.label}</span>
              <span
                className={`text-[11px] px-1.5 py-0.2 rounded-full ${
                  isActive
                    ? "bg-[#5E51E8]/15 text-[#5E51E8] dark:text-[#818CF8]"
                    : "bg-[var(--surface-elevated)] text-[var(--text-tertiary)]"
                }`}
              >
                {tab.count}
              </span>
            </button>
          );
        })}
      </div>

      {/* Search, Filter Chips & View Mode Controls */}
      <div className="space-y-3.5 mb-6">
        <div className="flex flex-col sm:flex-row gap-2.5 items-stretch sm:items-center justify-between">
          {/* Search box with Vietnamese unaccented search */}
          <div className="relative flex-1 min-w-0 sm:max-w-lg">
            <Search className="w-4 h-4 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              ref={searchInputRef}
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm khóa học theo tên hoặc mô tả..."
              aria-label="Tìm khóa học theo tên hoặc mô tả (hỗ trợ không dấu)"
              className="w-full h-9 pl-9 pr-9 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] focus:ring-1 focus:ring-[#5E51E8] transition"
            />
            {searchQuery ? (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                className="absolute right-2.5 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                aria-label="Xóa từ khóa"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            ) : (
              <kbd className="hidden sm:inline-flex swms-kbd text-[10px] pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2">
                /
              </kbd>
            )}
          </div>

          {/* Right controls: Sorting + Grid/List View switch */}
          <div className="flex flex-wrap items-center justify-between sm:justify-end gap-2 w-full sm:w-auto">
            <select
              aria-label="Sắp xếp khóa học"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as SortOption)}
              className="h-9 px-2.5 min-w-0 max-w-full flex-1 sm:flex-initial rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-primary)] focus:outline-none focus:border-[#5E51E8] cursor-pointer"
            >
              <option value="recommended">Sắp xếp: Mặc định</option>
              <option value="progress_desc">Tiến độ cao nhất</option>
              <option value="recent">Mới cập nhật</option>
              <option value="title_asc">Tên: A → Z</option>
              <option value="duration_asc">Thời lượng tăng dần</option>
            </select>

            {/* Grid / List View Toggle */}
            <div className="inline-flex items-center rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] p-0.5">
              <button
                type="button"
                onClick={() => handleToggleLayout("grid")}
                className={`p-1.5 rounded-lg transition cursor-pointer ${
                  viewLayout === "grid"
                    ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] shadow-2xs"
                    : "text-[var(--text-tertiary)] hover:text-[var(--text-primary)]"
                }`}
                title="Dạng lưới"
                aria-label="Dạng lưới"
              >
                <LayoutGrid className="w-4 h-4" />
              </button>
              <button
                type="button"
                onClick={() => handleToggleLayout("list")}
                className={`p-1.5 rounded-lg transition cursor-pointer ${
                  viewLayout === "list"
                    ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] shadow-2xs"
                    : "text-[var(--text-tertiary)] hover:text-[var(--text-primary)]"
                }`}
                title="Dạng danh sách"
                aria-label="Dạng danh sách"
              >
                <List className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        {/* Filter Chips: Feature Toggles + Domain + Level */}
        <div className="flex flex-wrap items-center gap-2 pt-1">
          {/* Feature Chips */}
          <button
            type="button"
            onClick={() => setFilterHasLab((prev) => !prev)}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer border ${
              filterHasLab
                ? "bg-purple-50 text-purple-700 border-purple-300 dark:bg-purple-950/60 dark:text-purple-300 dark:border-purple-800"
                : "bg-[var(--surface-card)] text-[var(--text-secondary)] border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]"
            }`}
          >
            <Sparkles className="w-3.5 h-3.5 text-purple-500" />
            <span>Có Lab thực hành</span>
          </button>

          <button
            type="button"
            onClick={() => setFilterHasAudio((prev) => !prev)}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer border ${
              filterHasAudio
                ? "bg-teal-50 text-teal-700 border-teal-300 dark:bg-teal-950/60 dark:text-teal-300 dark:border-teal-800"
                : "bg-[var(--surface-card)] text-[var(--text-secondary)] border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]"
            }`}
          >
            <Headphones className="w-3.5 h-3.5 text-teal-500" />
            <span>Có Audio / Nghe-Nói</span>
          </button>

          <button
            type="button"
            onClick={() => setFilterHasFlashcards((prev) => !prev)}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer border ${
              filterHasFlashcards
                ? "bg-emerald-50 text-emerald-700 border-emerald-300 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-800"
                : "bg-[var(--surface-card)] text-[var(--text-secondary)] border-[var(--border-calm)] hover:bg-[var(--surface-elevated)]"
            }`}
          >
            <Brain className="w-3.5 h-3.5 text-emerald-500" />
            <span>Có Flashcard FSRS</span>
          </button>

          <div className="h-4 w-px bg-[var(--border-calm)] mx-1 hidden sm:block" />

          {/* Level Filter Dropdown */}
          <select
            aria-label="Lọc theo trình độ"
            value={levelFilter}
            onChange={(e) => setLevelFilter(e.target.value as LevelFilter)}
            className="h-8 px-2.5 max-w-full rounded-lg border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] focus:outline-none focus:border-[#5E51E8] cursor-pointer"
          >
            <option value="all">Mọi trình độ</option>
            <option value="foundation">Nền tảng</option>
            <option value="intermediate">Trung cấp</option>
            <option value="advanced">Chuyên sâu</option>
          </select>

          {/* Category Quick Filter Dropdown */}
          <select
            aria-label="Lọc theo lĩnh vực"
            value={selectedDomain}
            onChange={(e) => setSelectedDomain(e.target.value as CourseDomainId)}
            className="h-8 px-2.5 max-w-full rounded-lg border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs font-medium text-[var(--text-secondary)] focus:outline-none focus:border-[#5E51E8] cursor-pointer"
          >
            <option value="all">Tất cả lĩnh vực</option>
            {COURSE_DOMAINS.map((domain) => (
              <option key={domain.id} value={domain.id}>
                {domain.id === "other" ? "Tâm lý & Kỹ năng sống" : domain.label}
              </option>
            ))}
          </select>

          {activeFilterCount > 0 && (
            <button
              type="button"
              onClick={() => {
                setFilterHasLab(false);
                setFilterHasAudio(false);
                setFilterHasFlashcards(false);
                setLevelFilter("all");
                setSelectedDomain("all");
                setSearchQuery("");
              }}
              className="inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold border border-rose-200 bg-rose-50/80 text-rose-700 hover:bg-rose-100 dark:border-rose-800/70 dark:bg-rose-950/40 dark:text-rose-300 dark:hover:bg-rose-950/70 transition ml-auto cursor-pointer shrink-0"
            >
              <X className="w-3.5 h-3.5" />
              <span>Xóa bộ lọc ({activeFilterCount})</span>
            </button>
          )}
        </div>

        {/* Removable Active Filter Summary Pills */}
        {activeFilterCount > 0 && (
          <div className="flex flex-wrap items-center gap-1.5 pt-0.5">
            <span className="text-[11px] font-medium text-[var(--text-tertiary)] mr-1 inline-flex items-center gap-1">
              <Filter className="w-3 h-3" />
              <span>Đang lọc:</span>
            </span>
            {searchQuery.trim() && (
              <button
                type="button"
                onClick={() => setSearchQuery("")}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[#5E51E8]/50 transition cursor-pointer"
              >
                <span>Loại bỏ: Từ khóa &ldquo;{searchQuery.trim()}&rdquo;</span>
                <X className="w-3 h-3" />
              </button>
            )}
            {filterHasLab && (
              <button
                type="button"
                onClick={() => setFilterHasLab(false)}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/50 dark:text-purple-300 dark:border-purple-800 hover:opacity-85 transition cursor-pointer"
              >
                <span>Loại bỏ: Lab thực hành</span>
                <X className="w-3 h-3" />
              </button>
            )}
            {filterHasAudio && (
              <button
                type="button"
                onClick={() => setFilterHasAudio(false)}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-teal-50 text-teal-700 border border-teal-200 dark:bg-teal-950/50 dark:text-teal-300 dark:border-teal-800 hover:opacity-85 transition cursor-pointer"
              >
                <span>Loại bỏ: Audio</span>
                <X className="w-3 h-3" />
              </button>
            )}
            {filterHasFlashcards && (
              <button
                type="button"
                onClick={() => setFilterHasFlashcards(false)}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/50 dark:text-emerald-300 dark:border-emerald-800 hover:opacity-85 transition cursor-pointer"
              >
                <span>Loại bỏ: Flashcard</span>
                <X className="w-3 h-3" />
              </button>
            )}
            {levelFilter !== "all" && (
              <button
                type="button"
                onClick={() => setLevelFilter("all")}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[#5E51E8]/50 transition cursor-pointer"
              >
                <span>
                  Loại bỏ: Trình độ{" "}
                  {levelFilter === "foundation"
                    ? "Nền tảng"
                    : levelFilter === "intermediate"
                    ? "Trung cấp"
                    : "Chuyên sâu"}
                </span>
                <X className="w-3 h-3" />
              </button>
            )}
            {selectedDomain !== "all" && (
              <button
                type="button"
                onClick={() => setSelectedDomain("all")}
                className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-medium bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[#5E51E8]/50 transition cursor-pointer"
              >
                <span>
                  Loại bỏ:{" "}
                  {selectedDomain === "other"
                    ? "Tâm lý & Kỹ năng sống"
                    : COURSE_DOMAINS.find((d) => d.id === selectedDomain)?.label ?? selectedDomain}
                </span>
                <X className="w-3 h-3" />
              </button>
            )}
          </div>
        )}
      </div>

      {/* Courses Display: Grid or List */}
      {sortedCourses.length > 0 ? (
        <>
          {viewLayout === "grid" ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-6">
              {visibleCourses.map((course) => {
                const id = course.id;
                const progress = course.progressPercent ?? 0;
                const lessonCount = course.lessonCount ?? 0;
                const completedLessons = course.completedLessons ?? 0;
                const chapterCount = course.chapterCount ?? 0;
                const theme = getCourseVisualTheme(course);
                const skillTags = extractCourseSkillTags(course);

                const primaryCtaLabel =
                  progress === 100
                    ? "Ôn lại bài"
                    : progress > 0 || completedLessons > 0
                    ? "Tiếp tục học"
                    : "Bắt đầu học";

                return (
                  <Card
                    key={id}
                    hoverable
                    data-course-slug={course.slug ?? ""}
                    className="swms-cv-card relative overflow-hidden flex flex-col justify-between p-0 shadow-xs hover:shadow-md transition-all duration-200"
                  >
                  {/* Banner with course theme */}
                  <CourseCardGraphicBanner
                    domainId={theme.domainId}
                    domainLabel={theme.domainLabel}
                    progressPercent={progress}
                    levelLabel={theme.levelLabel}
                    courseTitle={course.title}
                    courseSlug={course.slug ?? undefined}
                  />

                  {/* Pin Indicator Icon (Floating) */}
                  {course.isPinned && !course.isArchived && (
                    <div
                      className="absolute top-2 right-2 z-20 w-6 h-6 rounded-full bg-amber-500 text-white shadow-sm flex items-center justify-center"
                      title="Khóa học đã ghim ưu tiên"
                    >
                      <Pin className="w-3 h-3 fill-current" />
                    </div>
                  )}

                  <div className="p-5 flex-1 flex flex-col justify-between gap-4">
                    <div className="min-w-0">
                      {/* Top Badges Row */}
                      <div className="flex flex-wrap items-center gap-1.5 mb-2">
                        <span className="text-[10px] font-bold px-2 py-0.5 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)]">
                          {course.category || theme.domainLabel}
                        </span>

                        {course.isSystemCourse ? (
                          <span className="text-[10px] font-bold px-1.5 py-0.5 rounded-md bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800">
                            Hệ thống
                          </span>
                        ) : (
                          <span className="text-[10px] font-bold px-1.5 py-0.5 rounded-md bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800">
                            Cá nhân
                          </span>
                        )}

                        {Number(course.practiceTotalActivities ?? 0) > 0 && (
                          <span className="inline-flex items-center gap-1 text-[10px] font-bold px-1.5 py-0.5 rounded-md bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800">
                            <Sparkles className="w-2.5 h-2.5" />
                            <span>{course.labLabel || "Lab"}</span>
                          </span>
                        )}

                        {course.hasAudio && (
                          <span className="inline-flex items-center gap-1 text-[10px] font-bold px-1.5 py-0.5 rounded-md bg-teal-50 text-teal-700 border border-teal-200 dark:bg-teal-950/40 dark:text-teal-300 dark:border-teal-800">
                            <Headphones className="w-2.5 h-2.5" />
                            <span>Audio</span>
                          </span>
                        )}

                        {Number(course.bookmarkedLessonCount ?? 0) > 0 && (
                          <span className="inline-flex items-center gap-0.5 text-[10px] font-semibold text-amber-600 dark:text-amber-400">
                            <Bookmark className="w-2.5 h-2.5 fill-current" />
                            <span>{course.bookmarkedLessonCount}</span>
                          </span>
                        )}
                      </div>

                      {/* Course Title */}
                      <Link
                        href={`/app/courses/${id}`}
                        title={course.title || "Khóa học"}
                        className="block text-base font-bold text-[var(--text-primary)] hover:text-[#5E51E8] dark:hover:text-[#818CF8] transition line-clamp-2 leading-snug mb-1.5 break-words min-w-0 min-h-[2.75rem]"
                      >
                        {course.title || "Khóa học"}
                      </Link>

                      {/* Course Description */}
                      <p
                        title={course.description || ""}
                        className="text-xs text-[var(--text-secondary)] line-clamp-2 leading-relaxed mb-3 break-words min-h-[2.4rem]"
                      >
                        {course.description ||
                          "Lộ trình học theo chương, bài giảng tương tác, phòng thực hành và thẻ nhớ FSRS."}
                      </p>

                      {/* Skill Tags */}
                      <div className="flex flex-wrap gap-1.5">
                        {skillTags.slice(0, 3).map((tag) => (
                          <span
                            key={tag}
                            className="inline-flex items-center px-2 py-0.5 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-xs font-medium text-[var(--text-secondary)] truncate max-w-[180px]"
                            title={tag}
                          >
                            {tag}
                          </span>
                        ))}
                      </div>
                    </div>

                    <div className="pt-3 border-t border-[var(--border-calm)] space-y-3">
                      {/* Chapter, Lesson & Duration counters */}
                      <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-[var(--text-secondary)]">
                        <div className="flex items-center gap-1.5 flex-wrap">
                          {chapterCount > 0 && (
                            <>
                              <span className="inline-flex items-center gap-1 font-medium whitespace-nowrap shrink-0">
                                <Layers className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                                {chapterCount} chương
                              </span>
                              <span>·</span>
                            </>
                          )}
                          <span className="inline-flex items-center gap-1 font-medium whitespace-nowrap shrink-0">
                            <BookOpen className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                            {lessonCount} bài
                          </span>
                          <span>·</span>
                          <span className="inline-flex items-center gap-1 font-medium whitespace-nowrap shrink-0">
                            <Clock className="w-3.5 h-3.5 text-[var(--text-tertiary)] shrink-0" />
                            {theme.estimatedHoursLabel}
                          </span>
                        </div>
                      </div>

                      {/* Progress Bar */}
                      {progress === 0 && completedLessons === 0 ? (
                        <div className="flex items-center justify-between gap-2 rounded-lg bg-[var(--surface-elevated)]/80 border border-[var(--border-calm)] px-2.5 py-1.5 text-xs text-[var(--text-secondary)]">
                          <span className="font-medium whitespace-nowrap shrink-0">
                            Chưa bắt đầu • {lessonCount} bài
                          </span>
                          <span
                            title={theme.levelLabel}
                            className="text-[11px] font-semibold text-[#5E51E8] dark:text-[#A5B4FC] min-w-0 truncate"
                          >
                            {theme.levelLabel}
                          </span>
                        </div>
                      ) : (
                        <div className="space-y-1">
                          <div className="flex items-center justify-between gap-2 text-xs font-semibold text-[var(--text-secondary)]">
                            <span className="whitespace-nowrap shrink-0">
                              Tiến độ: {completedLessons}/{lessonCount} bài
                            </span>
                            <span className="text-[var(--text-primary)] font-bold whitespace-nowrap shrink-0">
                              {progress}%
                            </span>
                          </div>
                          <ProgressBar
                            value={progress}
                            color={progress === 100 ? "success" : "brand"}
                            size="sm"
                          />
                        </div>
                      )}

                      {/* Primary Actions & 3-Dot Dropdown */}
                      <div className="flex items-center gap-2 pt-1 relative">
                        <Link href={`/app/courses/${id}/learn`} className="flex-1 min-w-0">
                          <Button
                            variant="primary"
                            size="sm"
                            className="w-full whitespace-nowrap bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                            leftIcon={<Play className="w-3 h-3 fill-current" />}
                          >
                            {primaryCtaLabel}
                          </Button>
                        </Link>

                        <button
                          type="button"
                          onClick={() => setPreviewCourse(course)}
                          className="px-2.5 py-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition cursor-pointer"
                          title="Xem nhanh đề cương"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>

                        {renderCourseMenu(course)}
                      </div>
                    </div>
                  </div>
                </Card>
              );
            })}
          </div>
        ) : (
          /* List View Layout */
          <div className="space-y-3">
            {visibleCourses.map((course) => {
              const id = course.id;
              const progress = course.progressPercent ?? 0;
              const lessonCount = course.lessonCount ?? 0;
              const completedLessons = course.completedLessons ?? 0;
              const theme = getCourseVisualTheme(course);

              return (
                <div
                  key={id}
                  data-course-slug={course.slug ?? ""}
                  className="swms-cv-card rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[#5E51E8]/50 p-4 transition flex flex-col md:flex-row md:items-center justify-between gap-4 shadow-2xs"
                >
                  <div className="min-w-0 flex-1 space-y-1.5">
                    <div className="flex flex-wrap items-center gap-1.5">
                      <span className="text-[10px] font-bold px-2 py-0.5 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[var(--text-secondary)]">
                        {course.category || theme.domainLabel}
                      </span>
                      {course.isPinned && !course.isArchived && (
                        <span className="inline-flex items-center gap-1 text-[10px] font-bold px-1.5 py-0.5 rounded-md bg-amber-50 text-amber-700 border border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800">
                          <Pin className="w-2.5 h-2.5 fill-current" />
                          <span>Đã ghim</span>
                        </span>
                      )}
                      {course.isSystemCourse ? (
                        <span className="text-[10px] font-semibold px-1.5 py-0.5 rounded bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800">
                          Hệ thống
                        </span>
                      ) : (
                        <span className="text-[10px] font-semibold px-1.5 py-0.5 rounded bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800">
                          Cá nhân
                        </span>
                      )}
                      {Number(course.practiceTotalActivities ?? 0) > 0 && (
                        <span className="inline-flex items-center gap-1 text-[10px] font-semibold px-1.5 py-0.5 rounded bg-purple-50 text-purple-700 border border-purple-200 dark:bg-purple-950/40 dark:text-purple-300 dark:border-purple-800">
                          <Sparkles className="w-2.5 h-2.5" />
                          <span>Lab</span>
                        </span>
                      )}
                      {course.hasAudio && (
                        <span className="inline-flex items-center gap-1 text-[10px] font-semibold px-1.5 py-0.5 rounded bg-teal-50 text-teal-700 border border-teal-200 dark:bg-teal-950/40 dark:text-teal-300 dark:border-teal-800">
                          <Headphones className="w-2.5 h-2.5" />
                          <span>Audio</span>
                        </span>
                      )}
                    </div>

                    <Link
                      href={`/app/courses/${id}`}
                      className="text-base font-bold text-[var(--text-primary)] hover:text-[#5E51E8] transition block truncate"
                    >
                      {course.title}
                    </Link>

                    <p className="text-xs text-[var(--text-secondary)] line-clamp-1">
                      {course.description || "Lộ trình học theo chương, bài giảng tương tác và thẻ nhớ FSRS."}
                    </p>

                    <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-[var(--text-tertiary)] pt-1">
                      <span>{lessonCount} bài</span>
                      <span>·</span>
                      <span>{theme.estimatedHoursLabel}</span>
                      <span>·</span>
                      <span>Đã học {completedLessons}/{lessonCount} bài ({progress}%)</span>
                    </div>
                  </div>

                  {/* Progress & Actions in List Row */}
                  <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 shrink-0">
                    <div className="w-full sm:w-36 space-y-1">
                      <div className="flex justify-between text-[11px] font-semibold">
                        <span className="text-[var(--text-tertiary)]">Tiến độ</span>
                        <span className="text-[var(--text-primary)]">{progress}%</span>
                      </div>
                      <ProgressBar
                        value={progress}
                        color={progress === 100 ? "success" : "brand"}
                        size="sm"
                      />
                    </div>

                    <div className="flex flex-wrap items-center gap-2">
                      <button
                        type="button"
                        onClick={() => setPreviewCourse(course)}
                        className="px-3 py-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] hover:bg-[var(--surface-card)] text-xs font-semibold text-[var(--text-secondary)] transition cursor-pointer inline-flex items-center gap-1"
                        title="Xem nhanh đề cương"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        <span>Xem nhanh</span>
                      </button>

                      <Link href={`/app/courses/${id}/learn`} className="flex-1 sm:flex-initial">
                        <Button
                          variant="primary"
                          size="sm"
                          className="w-full sm:w-auto bg-[#5E51E8] hover:bg-[#4F46E5] text-white dark:bg-[#818CF8] dark:text-[#0F0F12] dark:hover:bg-[#A5B4FC]"
                          leftIcon={<Play className="w-3 h-3 fill-current" />}
                        >
                          {progress > 0 ? "Học tiếp" : "Vào học"}
                        </Button>
                      </Link>

                      <button
                        type="button"
                        onClick={() => handleTogglePin(course)}
                        className={`p-1.5 rounded-lg border border-[var(--border-calm)] transition cursor-pointer ${
                          course.isPinned
                            ? "bg-amber-50 text-amber-600 border-amber-300 dark:bg-amber-950/60 dark:text-amber-300"
                            : "bg-[var(--surface-elevated)] text-[var(--text-tertiary)] hover:text-[var(--text-primary)]"
                        }`}
                        title={course.isPinned ? "Bỏ ghim" : "Ghim"}
                        aria-label={course.isPinned ? "Bỏ ghim" : "Ghim"}
                      >
                        <Pin className="w-3.5 h-3.5" />
                      </button>

                      {renderCourseMenu(course)}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
          )}

          <ShowMoreFooter
            testIdPrefix="courses"
            shownCount={visibleCourses.length}
            totalCount={effectiveTotalCount}
            stepSize={LOAD_MORE_STEP}
            itemLabel="khóa học"
            isLoadingMore={false}
            onShowMore={() => {
              setVisibleCount((v) => v + LOAD_MORE_STEP);
              void ensureAllLoadedRef.current?.();
            }}
            onShowAll={() => {
              setVisibleCount((v) => Math.max(v, effectiveTotalCount, 100));
              void ensureAllLoadedRef.current?.();
            }}
          />
        </>
      ) : rawCourses.length === 0 ? (
        <EmptyState
          icon={BookOpen}
          title="Chào mừng bạn đến với Studxy!"
          description="Bạn chưa có khóa học nào trong danh sách. Hãy tạo khóa học đầu tiên để bắt đầu học tập và ghi nhớ có hệ thống."
          action={{
            label: "Tạo khóa học đầu tiên",
            href: "/app/courses/new",
            icon: <Plus className="w-4 h-4" />,
          }}
        />
      ) : (
        <EmptyState
          icon={BookOpen}
          title="Không tìm thấy khóa học phù hợp"
          description="Thử đổi từ khóa tìm kiếm, chọn lại tab danh mục hoặc điều chỉnh bộ lọc."
          action={{
            label: "Xóa toàn bộ bộ lọc",
            onClick: () => {
              setActiveTab("all");
              setSearchQuery("");
              setSelectedDomain("all");
              setLevelFilter("all");
              setFilterHasLab(false);
              setFilterHasAudio(false);
              setFilterHasFlashcards(false);
            },
          }}
        />
      )}

      {/* Delete Confirmation Modal (2-step clear confirmation) */}
      {courseToDelete && (
        <div
          role="dialog"
          aria-modal="true"
          className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-xs animate-in fade-in duration-200"
        >
          <div className="w-full max-w-md rounded-2xl border border-[var(--border-calm)] bg-[var(--surface-card)] p-6 shadow-2xl space-y-4 animate-in zoom-in-95 duration-200">
            <div className="w-10 h-10 rounded-xl bg-red-500/10 border border-red-500/20 text-red-600 dark:text-red-400 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5" />
            </div>

            <div className="space-y-1">
              <h3 className="text-base font-bold text-[var(--text-primary)]">
                Xác nhận xóa khóa học cá nhân
              </h3>
              <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                Bạn có chắc chắn muốn xóa khóa học{" "}
                <strong className="text-[var(--text-primary)]">
                  &ldquo;{courseToDelete.title}&rdquo;
                </strong>
                ? Toàn bộ bài giảng cá nhân, tiến độ và ghi chú liên quan sẽ bị xóa vĩnh viễn và không thể khôi phục.
              </p>
            </div>

            <div className="flex items-center justify-end gap-2.5 pt-2">
              <button
                type="button"
                onClick={() => setCourseToDelete(null)}
                disabled={isDeleting}
                className="px-4 py-2 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-secondary)] hover:text-[var(--text-primary)] transition disabled:opacity-50 cursor-pointer"
              >
                Hủy bỏ
              </button>

              <button
                type="button"
                onClick={handleDeleteCourse}
                disabled={isDeleting}
                className="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-700 text-white text-xs font-semibold inline-flex items-center gap-1.5 shadow-xs transition disabled:opacity-50 cursor-pointer"
              >
                {isDeleting ? (
                  <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                ) : (
                  <Trash2 className="w-3.5 h-3.5" />
                )}
                <span>Xóa vĩnh viễn</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Quick Preview Drawer */}
      <CourseQuickPreviewDrawer
        course={previewCourse}
        isOpen={Boolean(previewCourse)}
        onClose={() => setPreviewCourse(null)}
        onCloneSuccess={() => {
          fetchCourses();
        }}
      />

      {/* Duplicate Cleanup Modal */}
      <CourseDuplicateCleanupModal
        isOpen={isCleanupModalOpen}
        onClose={() => setIsCleanupModalOpen(false)}
        courses={rawCourses}
        onCleanupSuccess={() => {
          fetchCourses();
        }}
      />

      {/* Roadmaps & Use Cases Explorer */}
      <div className="mt-12">
        <LearningRoadmapsShowcase courses={rawCourses} compact={true} />
      </div>
    </div>
  );
}

export default function CoursesCatalogPage() {
  return (
    <Suspense
      fallback={
        <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 min-h-[80vh] flex items-center justify-center">
          <div className="flex items-center gap-2 text-xs text-[var(--text-secondary)] font-medium">
            <RefreshCw className="w-4 h-4 animate-spin text-[#5E51E8]" />
            <span>Đang tải danh mục khóa học...</span>
          </div>
        </div>
      }
    >
      <CoursesCatalogContent />
    </Suspense>
  );
}
