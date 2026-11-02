"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { LandingHeader } from "./LandingHeader";
import { HeroSection } from "./HeroSection";
import { VisualWorkspacePreview } from "./VisualWorkspacePreview";
import { CorePillarsSection } from "./CorePillarsSection";
import { ComparisonMatrixSection } from "./ComparisonMatrixSection";
import { FeaturesGridSection } from "./FeaturesGridSection";
import { WorkflowSection } from "./WorkflowSection";
import { PhilosophySection } from "./PhilosophySection";
import { ShortcutsTeaserSection } from "./ShortcutsTeaserSection";
import { LandingFooter } from "./LandingFooter";
import { AboutSwmsModal } from "@/components/auxiliary/AboutSwmsModal";
import { KeyboardShortcutsModal } from "@/components/auxiliary/KeyboardShortcutsModal";

export function LandingPageClient() {
  const router = useRouter();
  const [isAboutOpen, setIsAboutOpen] = useState(false);
  const [isShortcutsOpen, setIsShortcutsOpen] = useState(false);
  const [shortcutsTab, setShortcutsTab] = useState<"all" | "global" | "reading" | "split" | "review">("all");

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Don't trigger if focus is inside an input, textarea or contenteditable element
      const target = e.target as HTMLElement | null;
      if (
        target &&
        (target.tagName === "INPUT" ||
          target.tagName === "TEXTAREA" ||
          target.isContentEditable)
      ) {
        return;
      }

      if (e.key === "?" && !isShortcutsOpen && !isAboutOpen) {
        e.preventDefault();
        setShortcutsTab("all");
        setIsShortcutsOpen(true);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isShortcutsOpen, isAboutOpen]);

  const handleOpenShortcuts = (tab: "all" | "global" | "reading" | "split" | "review" = "all") => {
    setShortcutsTab(tab);
    setIsShortcutsOpen(true);
  };

  return (
    <main
      data-testid="landing-page-root"
      className="min-h-screen w-full bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col selection:bg-indigo-500/20 selection:text-indigo-900 dark:selection:text-indigo-200 transition-colors"
    >
      <LandingHeader
        onOpenShortcuts={() => handleOpenShortcuts("all")}
        onOpenAbout={() => setIsAboutOpen(true)}
      />

      <div className="flex-1 flex flex-col items-center w-full">
        <HeroSection
          onOpenShortcuts={() => handleOpenShortcuts("all")}
          onOpenAbout={() => setIsAboutOpen(true)}
        />

        <VisualWorkspacePreview />

        <CorePillarsSection />

        <ComparisonMatrixSection />

        <FeaturesGridSection />

        <WorkflowSection />

        <PhilosophySection onOpenAbout={() => setIsAboutOpen(true)} />

        <ShortcutsTeaserSection
          onOpenShortcuts={() => handleOpenShortcuts("all")}
        />
      </div>

      <LandingFooter
        onOpenAbout={() => setIsAboutOpen(true)}
        onOpenShortcuts={() => handleOpenShortcuts("all")}
      />

      {/* Auxiliary Modals */}
      <AboutSwmsModal
        isOpen={isAboutOpen}
        onClose={() => setIsAboutOpen(false)}
        onExploreWorkspace={() => router.push("/workspace")}
      />

      <KeyboardShortcutsModal
        isOpen={isShortcutsOpen}
        initialTab={shortcutsTab}
        onClose={() => setIsShortcutsOpen(false)}
      />
    </main>
  );
}
