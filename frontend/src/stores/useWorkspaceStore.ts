import { create } from "zustand";
import { persist } from "zustand/middleware";
import { getAccessToken } from "@/lib/api-client";

export type WorkspaceMode = "dashboard" | "reading" | "split" | "review" | "studio";
export type PdfViewMode = "dual" | "canvas" | "text";
export type LibraryFilter = "all" | "course" | "pdf" | "lesson" | "notebook";

export interface AiClientConfig {
  apiKey?: string;
  baseUrl?: string;
  model?: string;
}

export function getClientRequestHeaders(
  extraHeaders?: Record<string, string>
): Record<string, string> {
  const headers: Record<string, string> = { ...(extraHeaders ?? {}) };
  const token = getAccessToken();
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  if (typeof window === "undefined") return headers;
  try {
    const activeUserId = window.localStorage.getItem("swms_active_user_id");
    if (activeUserId && activeUserId.trim()) {
      headers["X-Test-User-Id"] = activeUserId.trim();
    }
    const rawAiConfig = window.localStorage.getItem("swms_ai_config");
    if (rawAiConfig) {
      const parsed = JSON.parse(rawAiConfig) as AiClientConfig;
      if (parsed.apiKey && parsed.apiKey.trim()) {
        headers["X-Ai-Api-Key"] = parsed.apiKey.trim();
      }
      if (parsed.baseUrl && parsed.baseUrl.trim()) {
        headers["X-Ai-Base-Url"] = parsed.baseUrl.trim();
      }
      if (parsed.model && parsed.model.trim()) {
        headers["X-Ai-Model"] = parsed.model.trim();
      }
    }
  } catch {
    // Ignore localStorage parse errors
  }
  return headers;
}

export interface SemanticHighlightConfig {
  key: "yellow" | "green" | "blue" | "purple" | "rose";
  icon: string;
  label: string;
  cssClass: string;
  badgeColor: string;
}

export const SEMANTIC_HIGHLIGHT_KEYS: SemanticHighlightConfig[] = [
  {
    key: "yellow",
    icon: "Lightbulb",
    label: "Khái niệm cốt lõi",
    cssClass: "swms-highlight-yellow",
    badgeColor: "bg-yellow-200/90 text-yellow-950 border-yellow-400 dark:bg-amber-950/80 dark:text-amber-200 dark:border-amber-500/60"
  },
  {
    key: "green",
    icon: "Code2",
    label: "Ví dụ & Dẫn chứng",
    cssClass: "swms-highlight-green",
    badgeColor: "bg-emerald-200/90 text-emerald-950 border-emerald-400 dark:bg-emerald-950/80 dark:text-emerald-200 dark:border-emerald-500/60"
  },
  {
    key: "blue",
    icon: "HelpCircle",
    label: "Câu hỏi & Nghi vấn",
    cssClass: "swms-highlight-blue",
    badgeColor: "bg-sky-200/90 text-sky-950 border-sky-400 dark:bg-sky-950/80 dark:text-sky-200 dark:border-sky-500/60"
  },
  {
    key: "purple",
    icon: "Link2",
    label: "Ý tưởng tổng hợp",
    cssClass: "swms-highlight-purple",
    badgeColor: "bg-purple-200/90 text-purple-950 border-purple-400 dark:bg-purple-950/80 dark:text-purple-200 dark:border-purple-500/60"
  },
  {
    key: "rose",
    icon: "AlertTriangle",
    label: "Lưu ý quan trọng",
    cssClass: "swms-highlight-rose",
    badgeColor: "bg-rose-200/90 text-rose-950 border-rose-400 dark:bg-rose-950/80 dark:text-rose-200 dark:border-rose-500/60"
  }
];

interface WorkspaceState {
  activeMode: WorkspaceMode;
  activeReviewSubTab: "fsrs" | "quiz";
  activeLessonId: string;
  activeDocumentId: string | null;
  sourceType: "lesson" | "document";
  selectedTagFilterId: string | null;
  sidebarCollapsed: boolean;
  splitRatios: [number, number, number];
  syncScrollEnabled: boolean;
  darkMode: boolean;
  serifFont: boolean;
  fontSizePx: number;
  zenMode: boolean;
  cleanReadingMode: boolean;
  commandPaletteOpen: boolean;
  orphanedDrawerOpen: boolean;
  dismissedOrphanBanner: boolean;
  // Persistent drafts across mode switches (Decision G & ADR-007)
  notebookDraftTextBySource: Record<string, string>;
  practiceQuizDraftAnswers: Record<string, string>;
  currentDocumentPage: number;
  activeDocumentTotalPages: number | null;
  pdfViewMode: PdfViewMode;
  libraryFilter: LibraryFilter;
  activeUserHeaderId: string | null;
  activeCourseId: string | null;
  activeQuizId: string | null;
  editingCourseId: string | null;
  editingLessonId: string | null;
  aiConfigModalOpen: boolean;
  courseSearchQuery: string;
  selectedCategory: string;

  setActiveMode: (mode: WorkspaceMode) => void;
  setActiveReviewSubTab: (tab: "fsrs" | "quiz" | "flashcards") => void;
  setActiveLesson: (lessonId: string) => void;
  setActiveDocument: (documentId: string, initialPage?: number, totalPages?: number | null) => void;
  setCurrentDocumentPage: (page: number) => void;
  setPdfViewMode: (mode: PdfViewMode) => void;
  setLibraryFilter: (f: LibraryFilter) => void;
  setActiveUserHeaderId: (id: string | null) => void;
  setActiveCourseId: (id: string | null) => void;
  setActiveQuizId: (id: string | null) => void;
  setEditingCourseId: (id: string | null) => void;
  setEditingLessonId: (id: string | null) => void;
  setAiConfigModalOpen: (open: boolean) => void;
  setCourseSearchQuery: (query: string) => void;
  setSelectedCategory: (category: string) => void;
  setSelectedTagFilterId: (tagId: string | null) => void;
  toggleSidebar: () => void;
  setSidebarCollapsed: (collapsed: boolean) => void;
  setSplitRatios: (ratios: [number, number, number]) => void;
  resetSplitRatios: () => void;
  toggleSyncScroll: () => void;
  toggleDarkMode: () => void;
  toggleSerifFont: () => void;
  setFontSizePx: (size: number) => void;
  toggleZenMode: () => void;
  toggleCleanReadingMode: () => void;
  setCommandPaletteOpen: (open: boolean) => void;
  setOrphanedDrawerOpen: (open: boolean) => void;
  setDismissedOrphanBanner: (dismissed: boolean) => void;
  setNotebookDraftText: (sourceId: string, text: string) => void;
  setPracticeQuizDraftAnswer: (itemId: string, answer: string) => void;
}

export const useWorkspaceStore = create<WorkspaceState>()(
  persist(
    (set) => ({
      activeMode: "dashboard",
      activeReviewSubTab: "fsrs",
      activeLessonId: "44444444-4444-4444-8444-444444444444",
      activeDocumentId: null,
      sourceType: "lesson",
      selectedTagFilterId: null,
      sidebarCollapsed: false,
      splitRatios: [15, 45, 40],
      syncScrollEnabled: true,
      darkMode: false,
      serifFont: true,
      fontSizePx: 18,
      zenMode: false,
      cleanReadingMode: true,
      commandPaletteOpen: false,
      orphanedDrawerOpen: false,
      dismissedOrphanBanner: false,
      notebookDraftTextBySource: {},
      practiceQuizDraftAnswers: {},
      currentDocumentPage: 1,
      activeDocumentTotalPages: null,
      pdfViewMode: "dual",
      libraryFilter: "all",
      activeUserHeaderId:
        typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null,
      activeCourseId: null,
      activeQuizId: null,
      editingCourseId: null,
      editingLessonId: null,
      aiConfigModalOpen: false,
      courseSearchQuery: "",
      selectedCategory: "Tất cả",

      setActiveMode: (mode) => set({ activeMode: mode }),
      setActiveReviewSubTab: (tab) =>
        set({ activeReviewSubTab: tab === "flashcards" ? "fsrs" : tab }),
      setActiveLesson: (lessonId) =>
        set({
          activeLessonId: lessonId,
          sourceType: "lesson",
          activeDocumentId: null,
          activeDocumentTotalPages: null
        }),
      setActiveDocument: (documentId, initialPage = 1, totalPages = null) =>
        set({
          activeDocumentId: documentId,
          sourceType: "document",
          currentDocumentPage: Math.max(1, initialPage),
          activeDocumentTotalPages:
            typeof totalPages === "number" && totalPages > 0 ? totalPages : null
        }),
      setCurrentDocumentPage: (page) => set({ currentDocumentPage: Math.max(1, page) }),
      setPdfViewMode: (mode) => set({ pdfViewMode: mode }),
      setLibraryFilter: (f) => set({ libraryFilter: f }),
      setActiveUserHeaderId: (id) => {
        if (typeof window !== "undefined") {
          if (id) {
            window.localStorage.setItem("swms_active_user_id", id);
          } else {
            window.localStorage.removeItem("swms_active_user_id");
          }
        }
        set({ activeUserHeaderId: id });
      },
      setActiveCourseId: (id) => set({ activeCourseId: id }),
      setActiveQuizId: (id) => set({ activeQuizId: id }),
      setEditingCourseId: (id) => set({ editingCourseId: id }),
      setEditingLessonId: (id) => set({ editingLessonId: id }),
      setAiConfigModalOpen: (open) => set({ aiConfigModalOpen: open }),
      setCourseSearchQuery: (query) => set({ courseSearchQuery: query }),
      setSelectedCategory: (category) => set({ selectedCategory: category }),
      setSelectedTagFilterId: (tagId) => set({ selectedTagFilterId: tagId }),
      toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
      setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
      setSplitRatios: (ratios) => set({ splitRatios: ratios }),
      resetSplitRatios: () => set({ splitRatios: [15, 45, 40] }),
      toggleSyncScroll: () => set((s) => ({ syncScrollEnabled: !s.syncScrollEnabled })),
      toggleDarkMode: () => set((s) => ({ darkMode: !s.darkMode })),
      toggleSerifFont: () => set((s) => ({ serifFont: !s.serifFont })),
      setFontSizePx: (size) => set({ fontSizePx: size }),
      toggleZenMode: () => set((s) => ({ zenMode: !s.zenMode })),
      toggleCleanReadingMode: () => set((s) => ({ cleanReadingMode: !s.cleanReadingMode })),
      setCommandPaletteOpen: (open) => set({ commandPaletteOpen: open }),
      setOrphanedDrawerOpen: (open) => set({ orphanedDrawerOpen: open }),
      setDismissedOrphanBanner: (dismissed) => set({ dismissedOrphanBanner: dismissed }),
      setNotebookDraftText: (sourceId, text) =>
        set((s) => ({
          notebookDraftTextBySource: { ...s.notebookDraftTextBySource, [sourceId]: text }
        })),
      setPracticeQuizDraftAnswer: (itemId, answer) =>
        set((s) => ({
          practiceQuizDraftAnswers: { ...s.practiceQuizDraftAnswers, [itemId]: answer }
        }))
    }),
    {
      name: "swms-workspace-persistent-state-v1",
      partialize: (state) => ({
        splitRatios: state.splitRatios,
        syncScrollEnabled: state.syncScrollEnabled,
        darkMode: state.darkMode,
        serifFont: state.serifFont,
        fontSizePx: state.fontSizePx,
        cleanReadingMode: state.cleanReadingMode,
        notebookDraftTextBySource: state.notebookDraftTextBySource,
        practiceQuizDraftAnswers: state.practiceQuizDraftAnswers
      })
    }
  )
);
