"use client";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { getClientRequestHeaders, useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { TopNavHeader } from "@/components/layout/TopNavHeader";
import { PersonalWorkspaceDashboard } from "@/components/dashboard/PersonalWorkspaceDashboard";
import { ReaderAndSplitViews } from "@/components/reader/ReaderAndSplitViews";
import { ReviewAndPracticeView } from "@/components/review/ReviewAndPracticeView";
import { CommandPaletteModal } from "@/components/command/CommandPaletteModal";
import { KeyboardShortcutsModal } from "@/components/auxiliary/KeyboardShortcutsModal";
import { animateWorkspaceTransition } from "@/lib/animations";
const FALLBACK_DASHBOARD_DATA = {
    user: {
        id: "11111111-1111-4111-8111-111111111111",
        fullName: "Sơn Trần",
        email: "son.tran@studxy.vn",
        streakDays: 7,
        onboardingStep: 2,
        isActivated: false
    },
    reviewStation: {
        totalDue: 4,
        totalCards: 4,
        newCount: 2,
        learningCount: 1,
        reviewCount: 1,
        estimatedMinutes: 2
    },
    lessons: [
        {
            id: "44444444-4444-4444-8444-444444444444",
            title: "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp",
            slug: "bo-nho-ao-tlb-va-bang-trang-da-cap",
            currentVersion: 2,
            updatedAt: new Date().toISOString()
        },
        {
            id: "44444444-4444-4444-8444-444444444445",
            title: "Bài 3.3: Thuật toán Thay thế Trang (LRU, Clock & Working Set)",
            slug: "thuat-toan-thay-the-trang-lru-clock",
            currentVersion: 1,
            updatedAt: new Date().toISOString()
        }
    ],
    documents: [
        {
            id: "55555555-5555-4555-8555-555555555555",
            title: "Operating Systems: Three Easy Pieces (OSTEP - Ch.18-20 Paging & TLB).pdf",
            totalPages: 48,
            fileSizeBytes: 19293798,
            ocrStatus: "completed",
            updatedAt: new Date().toISOString()
        }
    ],
    notebooks: [
        {
            id: "66666666-6666-4666-8666-666666666666",
            title: "Ghi chú: Bài 3.2 Bộ nhớ ảo, TLB và Bảng trang đa cấp",
            version: 1,
            primarySourceLessonId: "44444444-4444-4444-8444-444444444444",
            primarySourceDocumentId: null,
            updatedAt: new Date().toISOString()
        }
    ],
    tags: [
        { id: "88888881-1111-4111-8111-111111111111", name: "OS-Paging", colorKey: "blue", cardCount: 2 },
        { id: "88888882-2222-4222-8222-222222222222", name: "Memory", colorKey: "yellow", cardCount: 2 },
        { id: "88888883-3333-4333-8333-333333333333", name: "TLB", colorKey: "green", cardCount: 2 },
        { id: "88888884-4444-4444-8444-444444444444", name: "Algorithms", colorKey: "purple", cardCount: 1 }
    ]
};
export default function WorkspacePage() {
    const { activeMode, setActiveMode, darkMode, toggleDarkMode, sourceType, activeDocumentId, activeLessonId, currentDocumentPage, commandPaletteOpen, aiConfigModalOpen, activeUserHeaderId } = useWorkspaceStore();
    const [dashboard, setDashboard] = useState(FALLBACK_DASHBOARD_DATA);
    const [quota, setQuota] = useState(null);
    const mainContainerRef = useRef(null);
    const fetchDashboardData = useCallback(async () => {
        try {
            const effectiveUserId = activeUserHeaderId ??
                (typeof window !== "undefined"
                    ? window.localStorage.getItem("swms_active_user_id")
                    : null);
            const headers = getClientRequestHeaders(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined);
            const resp = await fetch("/api/v1/workspace/dashboard", {
                headers: Object.keys(headers).length > 0 ? headers : undefined
            });
            if (resp.ok) {
                const data = await resp.json();
                setDashboard(data);
                if (data.quota)
                    setQuota(data.quota);
            }
        }
        catch {
            // Uses fallback data while offline
        }
    }, [activeUserHeaderId]);
    useEffect(() => {
        fetchDashboardData();
    }, [fetchDashboardData]);
    useEffect(() => {
        animateWorkspaceTransition(mainContainerRef.current);
    }, [activeMode]);
    const [isShortcutsOpen, setIsShortcutsOpen] = useState(false);
    useEffect(() => {
        const handleKeyDown = (e) => {
            const target = e.target;
            if (target &&
                (target.tagName === "INPUT" ||
                    target.tagName === "TEXTAREA" ||
                    target.tagName === "SELECT" ||
                    target.isContentEditable)) {
                return;
            }
            if ((e.ctrlKey || e.metaKey) && !e.altKey && (e.key === "d" || e.key === "D")) {
                e.preventDefault();
                toggleDarkMode();
                return;
            }
            if (isShortcutsOpen || commandPaletteOpen || aiConfigModalOpen || document.querySelector('[role="dialog"]')) {
                return;
            }
            if (e.key === "?") {
                e.preventDefault();
                setIsShortcutsOpen(true);
                return;
            }
            if (e.key === "0" && !e.ctrlKey && !e.altKey && !e.metaKey) {
                e.preventDefault();
                setActiveMode("dashboard");
                return;
            }
            if ((e.key === "1" || e.key === "2" || e.key === "3") &&
                !e.ctrlKey &&
                !e.metaKey) {
                const hasSelectedText = typeof window !== "undefined" &&
                    (window.getSelection()?.toString().trim().length ?? 0) >= 3;
                const canSwitchMode = e.altKey ||
                    activeMode === "dashboard" ||
                    ((activeMode === "reading" || activeMode === "split") && !hasSelectedText);
                if (canSwitchMode) {
                    e.preventDefault();
                    if (e.key === "1")
                        setActiveMode("reading");
                    if (e.key === "2")
                        setActiveMode("split");
                    if (e.key === "3")
                        setActiveMode("review");
                }
            }
        };
        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [isShortcutsOpen, commandPaletteOpen, aiConfigModalOpen, activeMode, setActiveMode, toggleDarkMode]);
    const activeDoc = dashboard.documents.find((d) => d.id === activeDocumentId);
    const activeLesson = dashboard.lessons.find((l) => l.id === activeLessonId) ?? dashboard.lessons[0];
    const activeTitle = sourceType === "document" && activeDoc
        ? `${activeDoc.title} • Trang ${currentDocumentPage} / ${activeDoc.totalPages}`
        : (activeLesson?.title ?? "Bài 3.2: Bộ nhớ ảo, TLB và Bảng trang đa cấp");
    return (<div className={darkMode ? "dark" : ""}>
      <div className="min-h-screen w-full overflow-x-hidden bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100">
        <TopNavHeader userName={dashboard.user.fullName} userEmail={dashboard.user.email} streakDays={dashboard.user.streakDays} activeTitle={activeTitle} dueCount={dashboard.reviewStation.totalDue} quota={quota} onOpenShortcuts={() => setIsShortcutsOpen(true)}/>

        <main ref={mainContainerRef} className="w-full overflow-x-hidden">
          {activeMode === "dashboard" && (<PersonalWorkspaceDashboard data={dashboard} onRefreshDashboard={fetchDashboardData}/>)}

          {(activeMode === "reading" || activeMode === "split") && (<ReaderAndSplitViews mode={activeMode} onDataMutated={fetchDashboardData} documents={dashboard.documents} lessons={dashboard.lessons} courses={dashboard.courses ?? []} quizzes={dashboard.quizzes ?? []}/>)}

          {activeMode === "review" && (<ReviewAndPracticeView tags={dashboard.tags} onReviewSessionUpdated={fetchDashboardData} courses={dashboard.courses ?? []} quizzes={dashboard.quizzes ?? []}/>)}
        </main>

        <CommandPaletteModal onQuickCaptureCreated={fetchDashboardData}/>
        <KeyboardShortcutsModal isOpen={isShortcutsOpen} onClose={() => setIsShortcutsOpen(false)}/>
      </div>
    </div>);
}
