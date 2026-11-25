"use client";

import React, { useMemo, useState } from "react";
import Link from "next/link";
import {
  Award,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronRight,
  ClipboardCheck,
  ExternalLink,
  FlaskConical,
  Play,
  RotateCcw,
  Search,
  Sparkles,
  Trophy,
  X,
  Zap,
} from "lucide-react";
import { Button, Card, Badge, ProgressBar } from "@/components/ui";
import { normalizeVi } from "@/lib/course-helpers";
import type { CourseDetail, CourseLesson } from "./types";

export interface CoursePracticeTabProps {
  course: CourseDetail;
  courseId: string;
}

type PracticeStatusFilter = "all" | "pending" | "passed";

export function CoursePracticeTab({ course, courseId }: CoursePracticeTabProps) {
  const [labStatusFilter, setLabStatusFilter] = useState<PracticeStatusFilter>("all");
  const [labSearchQuery, setLabSearchQuery] = useState("");

  const allLessons = useMemo(
    () => course.chapters.flatMap((ch) => ch.lessons ?? []),
    [course.chapters]
  );

  // Find all lessons with practice activities
  const practiceLessons = useMemo(
    () => allLessons.filter((l) => (l.practiceActivityCount ?? 0) > 0),
    [allLessons]
  );

  const filteredPracticeLessons = useMemo(() => {
    const q = normalizeVi(labSearchQuery);
    return practiceLessons.filter((lesson) => {
      const acts = lesson.practiceActivityCount ?? 0;
      const passed = lesson.practicePassedCount ?? 0;
      const isAllPassed = acts > 0 && passed >= acts;

      if (labStatusFilter === "passed" && !isAllPassed) return false;
      if (labStatusFilter === "pending" && isAllPassed) return false;
      if (q && !normalizeVi(lesson.title).includes(q)) return false;
      return true;
    });
  }, [practiceLessons, labStatusFilter, labSearchQuery]);

  const firstPracticeLesson = practiceLessons[0] || allLessons[0];
  const firstPracticeLessonId = firstPracticeLesson?.id || course.firstLessonId || "";

  const totalActivities = course.practiceTotalActivities ?? 0;
  const passedActivities = course.practicePassedActivities ?? 0;
  const averageScore = course.practiceAverageScore ?? 0;
  const practiceProgressPct =
    totalActivities > 0 ? Math.round((passedActivities * 100) / totalActivities) : 0;

  const totalCards = course.flashcardCount ?? 0;
  const dueCards = course.dueFlashcardCount ?? 0;
  const masteredCards = course.masteredFlashcardCount ?? 0;

  const quizCount = course.quizCount ?? 0;

  const labLabel = course.labLabel?.trim() || "Phòng Thực Hành Tương Tác";

  const continuePracticeHref = firstPracticeLessonId
    ? `/app/practice/${courseId}/${firstPracticeLessonId}`
    : `/app/courses/${courseId}/learn`;

  return (
    <div className="space-y-6">
      {/* 1. Main Practice Lab Hero Card - ALWAYS ENABLED if practiceTotalActivities > 0 */}
      {totalActivities > 0 ? (
        <Card className="p-6 bg-[var(--surface-card)] border-indigo-500/20 relative overflow-hidden">
          <div className="absolute top-0 right-0 w-96 h-96 bg-indigo-500/5 rounded-full blur-3xl -mr-20 -mt-20 pointer-events-none" />

          <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 relative z-10">
            <div className="space-y-3 max-w-2xl">
              <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-indigo-500/10 text-indigo-700 dark:text-indigo-300 border border-indigo-500/20">
                <FlaskConical className="w-3.5 h-3.5" />
                <span>Phòng Thực Hành Trực Tiếp (Pillar 2)</span>
              </div>

              <h2 className="text-xl sm:text-2xl font-bold tracking-tight text-[var(--text-primary)]">
                {labLabel}
              </h2>

              <p className="text-xs sm:text-sm text-[var(--text-secondary)] leading-relaxed">
                Môi trường thực hành chuyên sâu theo từng bài học: luyện viết, chạy code, giải quyết bài toán tình huống thực tế và kiểm tra phản xạ kiến thức theo tiêu chuẩn ngành.
              </p>

              {/* Progress Bar & Metric Pills */}
              <div className="pt-2 space-y-2">
                <div className="flex items-center justify-between text-xs">
                  <span className="font-medium text-[var(--text-secondary)]">
                    Tiến độ hoàn thành bài tập Lab
                  </span>
                  <span className="font-bold text-indigo-700 dark:text-indigo-300">
                    {passedActivities}/{totalActivities} bài đạt ({practiceProgressPct}%)
                  </span>
                </div>
                <div className="w-full h-2 rounded-full bg-[var(--surface-elevated)] overflow-hidden">
                  <div
                    className="h-full rounded-full bg-gradient-to-r from-indigo-500 to-indigo-600 transition-all duration-300"
                    style={{ width: `${Math.min(100, Math.max(0, practiceProgressPct))}%` }}
                  />
                </div>
              </div>
            </div>

            {/* Score & Action CTA */}
            <div className="shrink-0 flex flex-col items-center md:items-end justify-center gap-3 p-5 rounded-2xl bg-[var(--surface-elevated)] border border-[var(--border-calm)]">
              {averageScore > 0 ? (
                <div className="text-center md:text-right">
                  <p className="text-[11px] font-semibold text-[var(--text-tertiary)] uppercase tracking-wider">
                    Điểm trung bình
                  </p>
                  <p className="text-3xl font-extrabold text-indigo-600 dark:text-indigo-400">
                    {averageScore}
                    <span className="text-sm font-medium text-[var(--text-tertiary)]">/100</span>
                  </p>
                </div>
              ) : (
                <div className="text-center md:text-right">
                  <p className="text-[11px] font-semibold text-[var(--text-tertiary)] uppercase tracking-wider">
                    Trạng thái
                  </p>
                  <p className="text-sm font-bold text-[var(--text-primary)]">
                    Sẵn sàng bắt đầu
                  </p>
                </div>
              )}

              <Link href={continuePracticeHref} className="w-full">
                <Button
                  variant="primary"
                  className="w-full whitespace-nowrap bg-indigo-600 hover:bg-indigo-700 text-white"
                  leftIcon={<FlaskConical className="w-4 h-4" />}
                >
                  <span>Mở Lab thực hành</span>
                </Button>
              </Link>
            </div>
          </div>
        </Card>
      ) : (
        <Card className="p-6 bg-[var(--surface-card)] text-center space-y-3">
          <FlaskConical className="w-8 h-8 text-[var(--text-tertiary)] mx-auto" />
          <h3 className="text-base font-bold text-[var(--text-primary)]">
            Phòng Thực Hành Tương Tác
          </h3>
          <p className="text-xs text-[var(--text-secondary)] max-w-lg mx-auto">
            Khóa học hiện đang áp dụng phương pháp củng cố trọng tâm qua Thẻ nhớ FSRS v5 và Ngân hàng Đề thi Trắc nghiệm phía dưới.
          </p>
        </Card>
      )}

      {/* 2. Practice Lessons List: Each lesson with its Lab status */}
      {practiceLessons.length > 0 && (
        <div className="space-y-3">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
            <h3 className="text-sm font-bold uppercase tracking-wider text-[var(--text-primary)] flex items-center gap-2 min-w-0">
              <Zap className="w-4 h-4 text-amber-500 shrink-0" />
              <span className="truncate">Danh sách các bài có Lab thực hành ({practiceLessons.length} bài)</span>
            </h3>
            <span className="text-xs text-[var(--text-tertiary)]">
              Bấm vào bài để vào phòng Lab tương ứng
            </span>
          </div>

          {/* Lab Status Filters & Quick Search */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2.5">
            <div className="flex flex-wrap items-center gap-1.5">
              {(
                [
                  { id: "all", label: `Tất cả (${practiceLessons.length})` },
                  {
                    id: "pending",
                    label: `Chưa đạt (${
                      practiceLessons.filter(
                        (l) => (l.practicePassedCount ?? 0) < (l.practiceActivityCount ?? 0)
                      ).length
                    })`,
                  },
                  {
                    id: "passed",
                    label: `Đã đạt (${
                      practiceLessons.filter(
                        (l) =>
                          (l.practiceActivityCount ?? 0) > 0 &&
                          (l.practicePassedCount ?? 0) >= (l.practiceActivityCount ?? 0)
                      ).length
                    })`,
                  },
                ] as { id: PracticeStatusFilter; label: string }[]
              ).map((tab) => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setLabStatusFilter(tab.id)}
                  className={`h-8 px-3 rounded-[8px] text-xs font-medium transition cursor-pointer whitespace-nowrap ${
                    labStatusFilter === tab.id
                      ? "bg-[#5E51E8] text-white dark:bg-[#818CF8] dark:text-[#0F0F12]"
                      : "bg-[var(--surface-elevated)] text-[var(--text-secondary)] border border-[var(--border-calm)] hover:text-[var(--text-primary)]"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {practiceLessons.length > 4 && (
              <div className="relative w-full sm:w-64">
                <Search className="w-3.5 h-3.5 text-[var(--text-tertiary)] absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={labSearchQuery}
                  onChange={(e) => setLabSearchQuery(e.target.value)}
                  placeholder="Tìm bài Lab theo tên..."
                  className="w-full h-8 pl-8 pr-7 rounded-[8px] border border-[var(--border-calm)] bg-[var(--surface-card)] text-xs text-[var(--text-primary)] placeholder:text-[var(--text-tertiary)] focus:outline-none focus:border-[#5E51E8] transition"
                />
                {labSearchQuery && (
                  <button
                    type="button"
                    onClick={() => setLabSearchQuery("")}
                    aria-label="Xóa từ khóa tìm bài Lab"
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-[var(--text-tertiary)] hover:text-[var(--text-primary)] cursor-pointer"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
            )}
          </div>

          {filteredPracticeLessons.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {filteredPracticeLessons.map((lesson) => {
                const origIdx = practiceLessons.findIndex((l) => l.id === lesson.id);
                const acts = lesson.practiceActivityCount ?? 0;
                const passed = lesson.practicePassedCount ?? 0;
                const isAllPassed = acts > 0 && passed >= acts;
                const pct = acts > 0 ? Math.min(100, Math.round((passed * 100) / acts)) : 0;

                return (
                  <div
                    key={lesson.id}
                    className={`min-w-0 p-4 rounded-xl border transition flex flex-col justify-between gap-3 ${
                      isAllPassed
                        ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50/20 dark:bg-emerald-950/10"
                        : "border-[var(--border-calm)] bg-[var(--surface-card)] hover:border-indigo-500/40"
                    }`}
                  >
                    <div className="space-y-2 min-w-0">
                      <div className="flex items-center justify-between gap-2">
                        <span className="text-[11px] font-bold text-indigo-700 dark:text-indigo-300">
                          Bài thực hành {origIdx + 1}
                        </span>
                        <span
                          className={`text-[11px] font-semibold px-2 py-0.5 rounded-full shrink-0 ${
                            isAllPassed
                              ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300"
                              : passed > 0
                              ? "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300"
                              : "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300"
                          }`}
                        >
                          {passed}/{acts} bài đạt
                        </span>
                      </div>

                      <p className="text-sm font-semibold text-[var(--text-primary)] line-clamp-2 break-words">
                        {lesson.title}
                      </p>

                      <div
                        className="w-full h-1.5 rounded-full bg-[var(--surface-elevated)] overflow-hidden"
                        aria-label={`Tiến độ Lab: ${passed}/${acts}`}
                      >
                        <div
                          className={`h-full rounded-full transition-all duration-300 ${
                            isAllPassed
                              ? "bg-emerald-500"
                              : "bg-indigo-500 dark:bg-indigo-400"
                          }`}
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>

                    <div className="flex flex-wrap items-center justify-between gap-2 pt-1 border-t border-[var(--border-calm)]/50">
                      <Link
                        href={`/app/courses/${courseId}/learn?lessonId=${lesson.id}`}
                        className="text-xs text-[var(--text-secondary)] hover:text-[var(--text-primary)] inline-flex items-center gap-1"
                      >
                        <BookOpen className="w-3.5 h-3.5 shrink-0" />
                        <span>Xem lý thuyết</span>
                      </Link>

                      <Link
                        href={`/app/practice/${courseId}/${lesson.id}`}
                        className="inline-flex items-center gap-1.5 h-7 px-3 rounded-lg bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 text-xs font-semibold hover:bg-indigo-100 dark:hover:bg-indigo-900 transition shrink-0"
                      >
                        <FlaskConical className="w-3 h-3" />
                        <span>Vào Lab</span>
                        <ChevronRight className="w-3 h-3" />
                      </Link>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <Card className="p-6 bg-[var(--surface-card)] text-center space-y-2.5">
              <p className="text-xs text-[var(--text-secondary)]">
                Không tìm thấy bài Lab nào khớp bộ lọc hiện tại.
              </p>
              <button
                type="button"
                onClick={() => {
                  setLabStatusFilter("all");
                  setLabSearchQuery("");
                }}
                className="inline-flex items-center gap-1 h-8 px-3 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs font-medium text-[var(--text-primary)] hover:border-indigo-500/50 transition cursor-pointer"
              >
                <X className="w-3.5 h-3.5" />
                <span>Xóa bộ lọc</span>
              </button>
            </Card>
          )}
        </div>
      )}

      {/* 3. FSRS Flashcards & Practice Quizzes Section */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* FSRS Flashcards Card */}
        <Card className="p-5 bg-[var(--surface-card)] space-y-4 flex flex-col justify-between">
          <div className="space-y-2">
            <div className="flex items-center justify-between gap-2">
              <div className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-amber-600 dark:text-amber-400">
                <Brain className="w-4 h-4" />
                <span>Thẻ nhớ ngắt quãng FSRS v5</span>
              </div>
              {dueCards > 0 && (
                <span className="text-[11px] font-bold px-2.5 py-0.5 rounded-full bg-amber-50 text-amber-800 border border-amber-300 dark:bg-amber-950/60 dark:text-amber-300 dark:border-amber-700">
                  {dueCards} thẻ đến hạn
                </span>
              )}
            </div>

            <h4 className="text-base font-bold text-[var(--text-primary)]">
              Ghi nhớ dài hạn với FSRS
            </h4>

            <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
              Thuật toán FSRS tính toán thời điểm ngắt quãng tối ưu theo đường cong quên lãng, giúp bạn ghi nhớ từ vựng, cú pháp và nguyên lý cốt lõi vĩnh viễn.
            </p>

            <div className="grid grid-cols-3 gap-2 pt-2">
              <div className="p-2.5 rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-center">
                <p className="text-[10px] text-[var(--text-tertiary)] uppercase font-semibold">Tổng số</p>
                <p className="text-base font-bold text-[var(--text-primary)]">{totalCards}</p>
              </div>
              <div className="p-2.5 rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-center">
                <p className="text-[10px] text-[var(--text-tertiary)] uppercase font-semibold">Cần ôn hôm nay</p>
                <p className="text-base font-bold text-amber-600 dark:text-amber-400">{dueCards}</p>
              </div>
              <div className="p-2.5 rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] text-center">
                <p className="text-[10px] text-[var(--text-tertiary)] uppercase font-semibold">Đã thuộc</p>
                <p className="text-base font-bold text-emerald-600 dark:text-emerald-400">{masteredCards}</p>
              </div>
            </div>
          </div>

          <Link href={`/app/review?courseId=${courseId}`} className="pt-2">
            <Button
              variant="secondary"
              className="w-full text-xs font-semibold justify-center"
              leftIcon={<Brain className="w-4 h-4 text-amber-600 dark:text-amber-400" />}
            >
              <span>{dueCards > 0 ? `Ôn ngay (${dueCards} thẻ)` : "Luyện thẻ FSRS"}</span>
            </Button>
          </Link>
        </Card>

        {/* Practice Quizzes Card */}
        <Card className="p-5 bg-[var(--surface-card)] space-y-4 flex flex-col justify-between">
          <div className="space-y-2">
            <div className="flex items-center justify-between gap-2">
              <div className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">
                <ClipboardCheck className="w-4 h-4" />
                <span>Ngân hàng Đề thi &amp; Trắc nghiệm</span>
              </div>
              {quizCount > 0 && (
                <span className="text-[11px] font-bold px-2.5 py-0.5 rounded-full bg-emerald-50 text-emerald-800 border border-emerald-300 dark:bg-emerald-950/60 dark:text-emerald-300 dark:border-emerald-700">
                  {quizCount} đề thi
                </span>
              )}
            </div>

            <h4 className="text-base font-bold text-[var(--text-primary)]">
              Đánh giá năng lực định kỳ
            </h4>

            <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
              Các bài kiểm tra trắc nghiệm từng bài và đề tổng kết toàn diện giúp kiểm tra mức độ nắm bắt kiến thức, phát hiện lỗ hổng và nhận đề xuất ôn tập kịp thời.
            </p>

            <div className="p-3 rounded-lg bg-[var(--surface-elevated)] border border-[var(--border-calm)] flex items-center justify-between gap-3 text-xs">
              <div className="space-y-0.5">
                <p className="font-semibold text-[var(--text-primary)]">
                  Bộ đề kiểm tra tổng hợp
                </p>
                <p className="text-[var(--text-tertiary)]">
                  Bao gồm bài kiểm tra sau từng chương và đề thi cuối khóa
                </p>
              </div>
              <Badge variant="neutral" size="sm">
                {quizCount} bộ đề
              </Badge>
            </div>
          </div>

          <Link href={`/app/courses/${courseId}/quiz`} className="pt-2">
            <Button
              variant="secondary"
              className="w-full text-xs font-semibold justify-center"
              leftIcon={<ClipboardCheck className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />}
            >
              <span>Vào làm Quiz &amp; Đề thi</span>
            </Button>
          </Link>
        </Card>
      </div>
    </div>
  );
}
