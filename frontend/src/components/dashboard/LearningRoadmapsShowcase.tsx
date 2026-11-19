"use client";

import React, { useMemo, useState } from "react";
import Link from "next/link";
import {
  ArrowRight,
  BookOpen,
  CheckCircle2,
  Compass,
  Layers,
  Play,
  Sparkles,
  Target,
  Zap,
} from "lucide-react";
import {
  LEARNING_ROADMAPS,
  STUDY_USE_CASES,
  resolveRoadmapProgress,
} from "@/lib/learning-roadmaps";
import type { CourseLike } from "@/lib/course-helpers";

interface LearningRoadmapsShowcaseProps {
  courses: CourseLike[];
  defaultTab?: "roadmaps" | "usecases";
  compact?: boolean;
}

const USE_CASE_ACCENT: Record<string, string> = {
  indigo: "from-indigo-500/15 to-violet-500/10 border-indigo-200/80 dark:border-indigo-800/60",
  sky: "from-sky-500/15 to-cyan-500/10 border-sky-200/80 dark:border-sky-800/60",
  violet: "from-violet-500/15 to-purple-500/10 border-violet-200/80 dark:border-violet-800/60",
  emerald: "from-emerald-500/15 to-teal-500/10 border-emerald-200/80 dark:border-emerald-800/60",
  amber: "from-amber-500/15 to-orange-500/10 border-amber-200/80 dark:border-amber-800/60",
  rose: "from-rose-500/15 to-pink-500/10 border-rose-200/80 dark:border-rose-800/60",
  purple: "from-purple-500/15 to-fuchsia-500/10 border-purple-200/80 dark:border-purple-800/60",
  teal: "from-teal-500/15 to-cyan-500/10 border-teal-200/80 dark:border-teal-800/60",
};

export function LearningRoadmapsShowcase({
  courses,
  defaultTab = "roadmaps",
  compact = false,
}: LearningRoadmapsShowcaseProps) {
  const [activeTab, setActiveTab] = useState<"roadmaps" | "usecases">(defaultTab);
  const [selectedRoadmapId, setSelectedRoadmapId] = useState<string>(LEARNING_ROADMAPS[0].id);
  const [showAllUseCases, setShowAllUseCases] = useState(!compact);

  const resolvedRoadmaps = useMemo(
    () => LEARNING_ROADMAPS.map((r) => resolveRoadmapProgress(r, courses)),
    [courses]
  );

  const activeResolved =
    resolvedRoadmaps.find((r) => r.roadmap.id === selectedRoadmapId) || resolvedRoadmaps[0];

  const visibleUseCases = showAllUseCases ? STUDY_USE_CASES : STUDY_USE_CASES.slice(0, 4);

  return (
    <section
      data-testid="learning-roadmaps-showcase"
      className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-card)] p-5 sm:p-6 space-y-5 shadow-xs"
    >
      {/* Top Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 pb-4 border-b border-[var(--border-calm)]">
        <div className="space-y-1">
          <div className="inline-flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wider text-[#5E51E8] dark:text-[#A5B4FC]">
            <Compass className="w-3.5 h-3.5" />
            <span>Hệ sinh thái Lộ trình &amp; Ứng dụng thực tế</span>
          </div>
          <h2 className="text-lg font-bold text-[var(--text-primary)] tracking-tight">
            {activeTab === "roadmaps"
              ? "6 Lộ Trình Học Tập Chuyên Sâu Theo Giai Đoạn"
              : "8 Kịch Bản Ứng Dụng (Use Cases) Thực Chiến"}
          </h2>
          <p className="text-xs text-[var(--text-secondary)]">
            {activeTab === "roadmaps"
              ? "Tự động đồng bộ tiến độ với các khóa học trong thư viện của bạn hoặc tạo nhanh bằng AI."
              : "Áp dụng ngay các quy trình học sâu, thực hành tương tác và ghi nhớ FSRS cho từng mục tiêu."}
          </p>
        </div>

        <div className="inline-flex items-center gap-1 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] p-1 self-start sm:self-auto shrink-0">
          <button
            type="button"
            data-testid="showcase-tab-roadmaps"
            onClick={() => setActiveTab("roadmaps")}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer ${
              activeTab === "roadmaps"
                ? "bg-[#5E51E8] text-white shadow-xs"
                : "text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
            }`}
          >
            <Layers className="w-3.5 h-3.5" />
            <span>Lộ trình ({LEARNING_ROADMAPS.length})</span>
          </button>
          <button
            type="button"
            data-testid="showcase-tab-usecases"
            onClick={() => setActiveTab("usecases")}
            className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-semibold transition cursor-pointer ${
              activeTab === "usecases"
                ? "bg-[#5E51E8] text-white shadow-xs"
                : "text-[var(--text-secondary)] hover:text-[var(--text-primary)]"
            }`}
          >
            <Zap className="w-3.5 h-3.5" />
            <span>Use Cases ({STUDY_USE_CASES.length})</span>
          </button>
        </div>
      </div>

      {activeTab === "roadmaps" ? (
        <div className="space-y-5">
          {/* Roadmap Selector Pills */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2.5">
            {resolvedRoadmaps.map((item) => {
              const isSelected = item.roadmap.id === activeResolved.roadmap.id;
              return (
                <button
                  key={item.roadmap.id}
                  type="button"
                  onClick={() => setSelectedRoadmapId(item.roadmap.id)}
                  className={`text-left rounded-xl border p-3.5 transition cursor-pointer flex flex-col justify-between gap-2 ${
                    isSelected
                      ? "border-[#5E51E8] bg-[#EEF0FD]/60 dark:bg-[#1E1B4B]/50 shadow-xs"
                      : "border-[var(--border-calm)] bg-[var(--surface-elevated)]/60 hover:border-[#5E51E8]/40"
                  }`}
                >
                  <div className="flex items-start justify-between gap-2">
                    <span
                      className={`inline-flex items-center px-2 py-0.5 rounded-md border text-[10px] font-semibold ${item.roadmap.badgeClasses}`}
                    >
                      {item.roadmap.badge}
                    </span>
                    <span className="text-[11px] font-bold text-[#5E51E8] dark:text-[#A5B4FC] shrink-0">
                      {item.overallProgress}%
                    </span>
                  </div>
                  <div className="space-y-0.5">
                    <p className="text-xs sm:text-sm font-semibold text-[var(--text-primary)] leading-snug">
                      {item.roadmap.title}
                    </p>
                    <p className="text-[11px] text-[var(--text-tertiary)]">
                      {item.matchedCoursesCount > 0
                        ? `Đã liên kết ${item.matchedCoursesCount} khóa học trong thư viện`
                        : item.roadmap.totalDuration}
                    </p>
                  </div>
                </button>
              );
            })}
          </div>

          {/* Active Roadmap Details + 4-Phase Visual Stepper */}
          {activeResolved && (
            <div className="rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 overflow-hidden">
              <div className={`h-1.5 w-full bg-gradient-to-r ${activeResolved.roadmap.accentGradient}`} />
              <div className="p-4 sm:p-5 space-y-4">
                <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3">
                  <div className="space-y-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <h3 className="text-base font-bold text-[var(--text-primary)]">
                        {activeResolved.roadmap.title}
                      </h3>
                      <span className="text-[11px] px-2 py-0.5 rounded-full bg-[var(--surface-card)] border border-[var(--border-calm)] text-[var(--text-secondary)]">
                        {activeResolved.roadmap.category}
                      </span>
                    </div>
                    <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
                      {activeResolved.roadmap.subtitle}
                    </p>
                  </div>
                  <div className="flex flex-wrap items-center gap-2 text-xs shrink-0">
                    <div className="px-3 py-1.5 rounded-lg bg-[var(--surface-card)] border border-[var(--border-calm)]">
                      <span className="text-[var(--text-tertiary)]">Đầu ra: </span>
                      <span className="font-semibold text-[var(--text-primary)]">
                        {activeResolved.roadmap.outcome}
                      </span>
                    </div>
                  </div>
                </div>

                {/* 4 Milestones Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-3.5">
                  {activeResolved.milestones.map((m, idx) => {
                    const hasCourse = Boolean(m.matchedCourseId);
                    const done = m.progress === 100;
                    return (
                      <div
                        key={m.milestone.id}
                        className={`rounded-xl border p-4 flex flex-col justify-between gap-3 transition ${
                          done
                            ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50/40 dark:bg-emerald-950/20"
                            : "border-[var(--border-calm)] bg-[var(--surface-card)]"
                        }`}
                      >
                        <div className="space-y-2.5">
                          <div className="flex items-center justify-between gap-2">
                            <span className="inline-flex items-center gap-1.5 text-[11px] font-bold text-[#5E51E8] dark:text-[#A5B4FC]">
                              <span className="w-5 h-5 rounded-full bg-[#EEF0FD] dark:bg-[#1E1B4B] flex items-center justify-center text-[10px]">
                                {idx + 1}
                              </span>
                              <span>{m.milestone.phaseLabel}</span>
                            </span>
                            <span className="text-[11px] text-[var(--text-tertiary)]">
                              {m.milestone.duration}
                            </span>
                          </div>

                          <div>
                            <h4 className="text-sm font-semibold text-[var(--text-primary)] leading-snug">
                              {m.milestone.title}
                            </h4>
                            <p className="text-xs text-[var(--text-secondary)] mt-1 leading-relaxed">
                              {m.milestone.summary}
                            </p>
                          </div>

                          <div className="flex flex-wrap gap-1">
                            {m.milestone.skills.map((skill) => (
                              <span
                                key={skill}
                                className="px-2 py-0.5 rounded-md bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-[10px] font-medium text-[var(--text-secondary)]"
                              >
                                {skill}
                              </span>
                            ))}
                          </div>

                          <div className="rounded-lg bg-[var(--surface-elevated)]/70 px-2.5 py-2 text-[11px] text-[var(--text-secondary)]">
                            <span className="font-semibold text-[var(--text-primary)]">Sản phẩm: </span>
                            {m.milestone.deliverable}
                          </div>
                        </div>

                        <div className="pt-2 border-t border-[var(--border-calm)] space-y-2">
                          {hasCourse && (
                            <div className="flex items-center justify-between text-[11px]">
                              <span className="text-[var(--text-tertiary)] truncate pr-2">
                                {m.matchedCourse?.title}
                              </span>
                              <span className="font-bold text-[#5E51E8] dark:text-[#A5B4FC] shrink-0">
                                {m.progress}%
                              </span>
                            </div>
                          )}

                          {hasCourse ? (
                            <Link
                              href={`/app/courses/${m.matchedCourseId}/learn`}
                              className="w-full inline-flex items-center justify-center gap-1.5 h-8 px-3 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-xs font-semibold transition"
                            >
                              {done ? (
                                <>
                                  <CheckCircle2 className="w-3.5 h-3.5" />
                                  <span>Ôn lại chặng này</span>
                                </>
                              ) : (
                                <>
                                  <Play className="w-3.5 h-3.5" />
                                  <span>Học chặng này</span>
                                </>
                              )}
                            </Link>
                          ) : (
                            <Link
                              href={`/app/courses/new?topic=${encodeURIComponent(m.milestone.aiTopicPrompt)}`}
                              className="w-full inline-flex items-center justify-center gap-1.5 h-8 px-3 rounded-lg border border-[#5E51E8]/40 bg-[#EEF0FD]/60 dark:bg-[#1E1B4B]/50 text-[#4F46E5] dark:text-[#A5B4FC] hover:bg-[#5E51E8] hover:text-white text-xs font-semibold transition"
                            >
                              <Sparkles className="w-3.5 h-3.5" />
                              <span>Tạo khóa chặng này</span>
                            </Link>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-3.5">
            {visibleUseCases.map((uc) => (
              <div
                key={uc.id}
                className={`rounded-xl border bg-gradient-to-br p-4 flex flex-col justify-between gap-3 ${
                  USE_CASE_ACCENT[uc.accentColor] || USE_CASE_ACCENT.indigo
                }`}
              >
                <div className="space-y-2.5">
                  <div className="flex items-center justify-between gap-2">
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-[var(--surface-card)]/90 border border-[var(--border-calm)] text-[10px] font-semibold text-[var(--text-secondary)]">
                      <Target className="w-3 h-3 text-[#5E51E8] dark:text-[#A5B4FC]" />
                      {uc.persona}
                    </span>
                  </div>

                  <h3 className="text-sm font-bold text-[var(--text-primary)] leading-snug">
                    {uc.title}
                  </h3>

                  <p className="text-[11px] text-[var(--text-secondary)] leading-relaxed">
                    <span className="font-semibold text-[var(--text-primary)]">Bài toán: </span>
                    {uc.painPoint}
                  </p>

                  <ol className="space-y-1 text-[11px] text-[var(--text-secondary)] list-decimal pl-4 leading-relaxed">
                    {uc.workflowSteps.map((step, idx) => (
                      <li key={idx}>{step}</li>
                    ))}
                  </ol>
                </div>

                <div className="pt-2.5 border-t border-[var(--border-calm)]/70 space-y-2.5">
                  <div className="flex flex-wrap gap-1">
                    {uc.recommendedFeatures.map((feat) => (
                      <span
                        key={feat}
                        className="px-2 py-0.5 rounded bg-[var(--surface-card)]/85 border border-[var(--border-calm)] text-[10px] font-medium text-[var(--text-primary)]"
                      >
                        {feat}
                      </span>
                    ))}
                  </div>
                  <div className="flex items-center gap-2">
                    <Link
                      href={uc.targetRoute}
                      className="flex-1 inline-flex items-center justify-center gap-1 h-8 px-2.5 rounded-lg bg-[#5E51E8] hover:bg-[#4F46E5] text-white text-xs font-semibold transition"
                    >
                      <BookOpen className="w-3.5 h-3.5" />
                      <span>{uc.ctaLabel}</span>
                    </Link>
                    <Link
                      href={`/app/courses/new?topic=${encodeURIComponent(uc.aiTemplateTopic)}`}
                      title="Khởi tạo khóa học AI theo Use Case này"
                      className="inline-flex items-center justify-center gap-1 h-8 px-2.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-[#5E51E8] text-xs font-semibold text-[var(--text-primary)] transition"
                    >
                      <Sparkles className="w-3.5 h-3.5 text-[#5E51E8]" />
                      <span>AI</span>
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {compact && STUDY_USE_CASES.length > 4 && (
            <div className="flex justify-center">
              <button
                type="button"
                onClick={() => setShowAllUseCases((v) => !v)}
                className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs font-semibold text-[var(--text-primary)] hover:border-[#5E51E8] transition cursor-pointer"
              >
                <span>{showAllUseCases ? "Thu gọn Use Cases" : `Xem thêm ${STUDY_USE_CASES.length - 4} Use Cases khác`}</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </button>
            </div>
          )}
        </div>
      )}
    </section>
  );
}
