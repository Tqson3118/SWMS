"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  AlertCircle,
  BookOpen,
  Brain,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ChevronsDownUp,
  ChevronsUpDown,
  Circle,
  ClipboardCheck,
  Clock,
  Layers,
  Pencil,
  Play,
  RefreshCw,
  Search,
  X,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { classifyCourseDomain, normalizeVi } from "@/lib/course-helpers";

interface CourseLesson {
  id: string;
  title: string;
  durationMinutes?: number;
  estimatedMinutes?: number;
  isCompleted?: boolean;
  sortOrder?: number;
  courseId?: string;
}

interface CourseChapter {
  id: string;
  chapterId?: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLesson[];
}

interface CourseDetail {
  id: string;
  title: string;
  description?: string;
  category?: string;
  visibility?: string;
  isPublished?: boolean;
  progressPercent: number;
  totalLessons: number;
  completedLessons: number;
  completedLessonIds?: string[];
  flashcardCount?: number;
  quizCount?: number;
  quizId?: string | null;
  chapters: CourseChapter[];
}

type LessonFilter = "all" | "pending" | "completed";

export default function CourseDetailPage() {
  const params = useParams();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState("");
  const [lessonFilter, setLessonFilter] = useState<LessonFilter>("all");
  const [expandedChapters, setExpandedChapters] = useState<Set<string>>(new Set());

  const fetchCourse = useCallback(async () => {
    if (!courseId) return;
    setIsLoading(true);
    setFetchError(null);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );

      const [courseResp, dashResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
        fetch("/api/v1/workspace/dashboard", { headers }).catch(() => null),
      ]);

      let courseData: CourseDetail | null = null;

      if (courseResp && courseResp.ok) {
        const raw = await courseResp.json();
        const chaps: CourseChapter[] = raw.chapters ?? [];
        const flatLessons = chaps.flatMap((ch) => ch.lessons ?? []);
        const completedIds: string[] = Array.isArray(raw.completedLessonIds)
          ? raw.completedLessonIds
          : [];
        const total = Number(raw.totalLessons ?? raw.lessonCount ?? flatLessons.length);
        const completed = Number(raw.completedLessons ?? completedIds.length);
        const progress =
          raw.progressPercent != null
            ? Number(raw.progressPercent)
            : total > 0
              ? Math.round((completed * 100) / total)
              : 0;
        courseData = {
          ...raw,
          id: String(raw.courseId || raw.id || courseId),
          totalLessons: total,
          completedLessons: completed,
          completedLessonIds: completedIds,
          progressPercent: progress,
          chapters: chaps,
        };
      } else if (dashResp && dashResp.ok) {
        const dashData = await dashResp.json();
        const allCourses = dashData.courses ?? [];
        const found = allCourses.find(
          (c: Record<string, unknown>) => (c.courseId || c.id) === courseId
        );
        if (found) {
          const chaps = (found.chapters as CourseChapter[]) ?? [];
          const total = Number(
            found.totalLessons ??
              found.lessonCount ??
              chaps.reduce((s, ch) => s + (ch.lessons?.length ?? 0), 0)
          );
          const completedIds = Array.isArray(found.completedLessonIds)
            ? (found.completedLessonIds as string[])
            : [];
          const completed = Number(found.completedLessons ?? completedIds.length);
          courseData = {
            id: String(found.courseId || found.id),
            title: String(found.title || "Khóa học"),
            description: String(found.description || ""),
            category: String(found.category || ""),
            visibility: String(found.visibility || "personal"),
            isPublished: Boolean(found.isPublished ?? found.visibility === "public"),
            progressPercent: Number(
              found.progressPercent ??
                found.progress ??
                (total > 0 ? Math.round((completed * 100) / total) : 0)
            ),
            totalLessons: total,
            completedLessons: completed,
            completedLessonIds: completedIds,
            flashcardCount: Number(found.flashcardCount ?? 0),
            quizCount: Number(found.quizCount ?? 0),
            quizId: (found.quizId as string | null | undefined) ?? null,
            chapters: chaps,
          };
        }
      } else if (courseResp && !courseResp.ok) {
        setFetchError(`Không tải được thông tin khóa học (HTTP ${courseResp.status}).`);
      }

      setCourse(courseData);
      if (courseData?.chapters) {
        setExpandedChapters(
          new Set(courseData.chapters.map((ch) => String(ch.chapterId || ch.id)))
        );
      }
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, activeUserHeaderId]);

  useEffect(() => {
    fetchCourse();
  }, [fetchCourse]);

  const completedSet = useMemo(
    () => new Set(course?.completedLessonIds ?? []),
    [course?.completedLessonIds]
  );

  const isDone = useCallback(
    (lesson: CourseLesson) => Boolean(lesson.isCompleted || completedSet.has(lesson.id)),
    [completedSet]
  );

  const allLessons = useMemo(
    () => (course?.chapters ?? []).flatMap((ch) => ch.lessons ?? []),
    [course?.chapters]
  );

  const nextLesson = useMemo(
    () => allLessons.find((l) => !isDone(l)) || allLessons[0],
    [allLessons, isDone]
  );

  const totalMinutes = useMemo(
    () =>
      allLessons.reduce(
        (sum, l) => sum + Number(l.durationMinutes ?? l.estimatedMinutes ?? 10),
        0
      ),
    [allLessons]
  );

  const filteredChapters = useMemo(() => {
    if (!course) return [];
    const q = normalizeVi(searchQuery);
    return course.chapters
      .map((ch) => {
        const matchingLessons = (ch.lessons ?? []).filter((lesson) => {
          const done = isDone(lesson);
          if (lessonFilter === "completed" && !done) return false;
          if (lessonFilter === "pending" && done) return false;
          if (q && !normalizeVi(lesson.title).includes(q)) return false;
          return true;
        });
        return { ...ch, lessons: matchingLessons };
      })
      .filter((ch) => ch.lessons.length > 0 || (!q && lessonFilter === "all"));
  }, [course, searchQuery, lessonFilter, isDone]);

  const allExpanded =
    course !== null &&
    course.chapters.length > 0 &&
    course.chapters.every((ch) => expandedChapters.has(String(ch.chapterId || ch.id)));

  const toggleAllChapters = () => {
    if (!course) return;
    if (allExpanded) {
      setExpandedChapters(new Set());
    } else {
      setExpandedChapters(new Set(course.chapters.map((ch) => String(ch.chapterId || ch.id))));
    }
  };

  const toggleChapter = (chId: string) => {
    setExpandedChapters((prev) => {
      const next = new Set(prev);
      if (next.has(chId)) next.delete(chId);
      else next.add(chId);
      return next;
    });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (!course) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12 text-center">
        {fetchError && (
          <div className="mb-4 rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-600 dark:text-red-400 inline-flex items-center gap-2">
            <AlertCircle className="w-4 h-4" />
            <span>{fetchError}</span>
          </div>
        )}
        <p className="text-text-secondary mb-4">Không tìm thấy khóa học.</p>
        <div className="flex items-center justify-center gap-3">
          <button
            type="button"
            onClick={fetchCourse}
            className="inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-border text-xs font-medium text-text-primary hover:bg-bg-secondary cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            Thử lại
          </button>
          <Link href="/app/courses" className="text-sm text-accent hover:underline">
            ← Về danh sách khóa học
          </Link>
        </div>
      </div>
    );
  }

  const domain = classifyCourseDomain(course);
  const continueLearnHref = nextLesson
    ? `/app/courses/${courseId}/learn?lesson=${nextLesson.id}`
    : `/app/courses/${courseId}/learn`;

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10">
      {/* Breadcrumb */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-1.5 text-sm text-text-tertiary mb-6 overflow-x-auto whitespace-nowrap"
      >
        <Link href="/app/courses" className="hover:text-text-primary transition">
          Khóa học
        </Link>
        <ChevronRight className="w-3.5 h-3.5 shrink-0" />
        <span className="text-text-primary font-medium truncate">{course.title}</span>
      </nav>

      {/* Hero Summary Card */}
      <div className="rounded-lg border border-border bg-bg-secondary p-6 mb-8">
        <div className="flex flex-wrap items-center gap-2 mb-3">
          <span className="text-xs font-medium text-accent bg-accent-subtle rounded-md px-2.5 py-0.5">
            {domain.label}
          </span>
          {(course.isPublished || course.visibility === "public") && (
            <span className="text-xs font-medium text-emerald-600 dark:text-emerald-400 bg-emerald-500/10 rounded-md px-2.5 py-0.5">
              Công khai
            </span>
          )}
        </div>

        <h1 className="text-2xl font-semibold tracking-tight text-text-primary mb-2">
          {course.title}
        </h1>
        {course.description && (
          <p className="text-sm text-text-secondary mb-4 leading-relaxed">
            {course.description}
          </p>
        )}

        {/* Stats Pills */}
        <div className="flex flex-wrap items-center gap-2 mb-5 text-xs text-text-secondary">
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-bg-primary border border-border">
            <Layers className="w-3.5 h-3.5 text-text-tertiary" />
            {course.chapters.length} chương
          </span>
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-bg-primary border border-border">
            <BookOpen className="w-3.5 h-3.5 text-text-tertiary" />
            {course.totalLessons} bài giảng
          </span>
          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-bg-primary border border-border">
            <Clock className="w-3.5 h-3.5 text-text-tertiary" />
            {totalMinutes} phút
          </span>
        </div>

        {/* Overall Progress */}
        <div className="mb-5">
          <div className="flex items-center justify-between text-xs text-text-secondary mb-1.5">
            <span>
              Tiến độ: {course.completedLessons}/{course.totalLessons} bài hoàn thành
            </span>
            <span className="font-semibold text-text-primary">{course.progressPercent}%</span>
          </div>
          <div className="w-full h-2 bg-bg-tertiary rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all ${
                course.progressPercent === 100 ? "bg-emerald-500" : "bg-accent"
              }`}
              style={{ width: `${Math.max(0, Math.min(100, course.progressPercent))}%` }}
            />
          </div>
        </div>

        {/* Next Lesson Highlight Banner */}
        {nextLesson && (
          <div className="rounded-md border border-border bg-bg-primary p-3.5 mb-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
            <div className="min-w-0">
              <p className="text-[11px] font-medium uppercase tracking-wider text-text-tertiary">
                {course.progressPercent === 100 ? "Ôn lại lộ trình" : "Bài học tiếp theo"}
              </p>
              <p className="text-sm font-medium text-text-primary truncate mt-0.5">
                {nextLesson.title}
              </p>
            </div>
            <Link
              href={continueLearnHref}
              data-testid="course-continue-learn-btn"
              className="inline-flex items-center justify-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition shrink-0"
            >
              <Play className="w-4 h-4" />
              <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
            </Link>
          </div>
        )}

        {/* Secondary Actions */}
        <div className="flex flex-wrap items-center gap-2">
          {!nextLesson && (
            <Link
              href={continueLearnHref}
              data-testid="course-continue-learn-btn"
              className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
            >
              <Play className="w-4 h-4" />
              <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
            </Link>
          )}
          <Link
            href={`/app/courses/${courseId}/quiz`}
            data-testid="course-quiz-btn"
            className="inline-flex items-center gap-2 h-9 px-3.5 rounded-md border border-border bg-bg-primary text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <ClipboardCheck className="w-4 h-4 text-text-secondary" />
            <span>Làm bài kiểm tra{course.quizCount ? ` (${course.quizCount})` : ""}</span>
          </Link>
          <Link
            href={`/app/review?courseId=${courseId}`}
            className="inline-flex items-center gap-2 h-9 px-3.5 rounded-md border border-border bg-bg-primary text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <Brain className="w-4 h-4 text-text-secondary" />
            <span>Ôn thẻ ghi nhớ{course.flashcardCount ? ` (${course.flashcardCount})` : ""}</span>
          </Link>
          <Link
            href={`/app/courses/${courseId}/edit`}
            className="inline-flex items-center gap-2 h-9 px-3.5 rounded-md border border-border bg-bg-primary text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <Pencil className="w-4 h-4 text-text-secondary" />
            <span>Biên tập khóa học</span>
          </Link>
        </div>
      </div>

      {/* Interactive Syllabus Explorer */}
      <div>
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
          <h2 className="text-base font-semibold text-text-primary">Đề cương khóa học</h2>
          {course.chapters.length > 0 && (
            <button
              type="button"
              onClick={toggleAllChapters}
              className="inline-flex items-center gap-1.5 text-xs font-medium text-text-secondary hover:text-text-primary transition cursor-pointer self-start sm:self-auto"
            >
              {allExpanded ? (
                <>
                  <ChevronsDownUp className="w-3.5 h-3.5" />
                  <span>Thu gọn tất cả</span>
                </>
              ) : (
                <>
                  <ChevronsUpDown className="w-3.5 h-3.5" />
                  <span>Mở rộng tất cả</span>
                </>
              )}
            </button>
          )}
        </div>

        {/* Search & Lesson Status Filter */}
        {allLessons.length > 0 && (
          <div className="flex flex-col sm:flex-row sm:items-center gap-2.5 mb-4">
            <div className="relative flex-1">
              <Search className="w-4 h-4 text-text-tertiary absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Tìm bài học trong đề cương..."
                className="w-full h-9 pl-9 pr-8 rounded-md border border-border bg-bg-primary text-sm text-text-primary placeholder:text-text-tertiary focus:outline-none focus:border-accent transition"
              />
              {searchQuery && (
                <button
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 text-text-tertiary hover:text-text-primary cursor-pointer"
                >
                  <X className="w-3.5 h-3.5" />
                </button>
              )}
            </div>

            <div className="flex items-center gap-1.5">
              {(
                [
                  { id: "all", label: "Tất cả" },
                  { id: "pending", label: "Chưa học" },
                  { id: "completed", label: "Đã hoàn thành" },
                ] as { id: LessonFilter; label: string }[]
              ).map((tab) => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setLessonFilter(tab.id)}
                  className={`h-8 px-3 rounded-md text-xs font-medium transition cursor-pointer ${
                    lessonFilter === tab.id
                      ? "bg-text-primary text-bg-primary"
                      : "bg-bg-secondary text-text-secondary border border-border hover:text-text-primary"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>
          </div>
        )}

        {filteredChapters.length > 0 ? (
          <div className="space-y-3">
            {filteredChapters.map((chapter, chIdx) => {
              const chKey = String(chapter.chapterId || chapter.id);
              const isOpen =
                searchQuery.trim().length > 0 ||
                lessonFilter !== "all" ||
                expandedChapters.has(chKey);
              const lessons = chapter.lessons ?? [];
              const origChapter =
                course.chapters.find((c) => String(c.chapterId || c.id) === chKey) ?? chapter;
              const origLessons = origChapter.lessons ?? [];
              const doneInChapter = origLessons.filter((l) => isDone(l)).length;

              return (
                <div
                  key={chKey}
                  className="rounded-lg border border-border bg-bg-primary overflow-hidden"
                >
                  <button
                    type="button"
                    onClick={() => toggleChapter(chKey)}
                    className="w-full px-4 py-3 bg-bg-secondary/50 hover:bg-bg-secondary flex items-center justify-between gap-3 text-left transition cursor-pointer"
                  >
                    <div className="flex items-center gap-2 min-w-0">
                      {isOpen ? (
                        <ChevronDown className="w-4 h-4 text-text-tertiary shrink-0" />
                      ) : (
                        <ChevronRight className="w-4 h-4 text-text-tertiary shrink-0" />
                      )}
                      <h3 className="text-sm font-semibold text-text-primary truncate">
                        Chương {chIdx + 1}: {chapter.title}
                      </h3>
                    </div>
                    <span className="text-xs font-medium px-2 py-0.5 rounded-md bg-bg-tertiary text-text-secondary shrink-0">
                      {doneInChapter}/{origLessons.length} bài
                    </span>
                  </button>

                  {isOpen && lessons.length > 0 && (
                    <div className="divide-y divide-border-subtle border-t border-border-subtle">
                      {lessons.map((lesson) => {
                        const completed = isDone(lesson);
                        const isNext = lesson.id === nextLesson?.id && !completed;
                        const mins = lesson.durationMinutes ?? lesson.estimatedMinutes;
                        return (
                          <Link
                            key={lesson.id}
                            href={`/app/courses/${courseId}/learn?lesson=${lesson.id}`}
                            className="flex items-center gap-3 px-4 py-3 hover:bg-bg-secondary transition group"
                          >
                            {completed ? (
                              <CheckCircle2 className="w-4 h-4 text-emerald-500 shrink-0" />
                            ) : isNext ? (
                              <Play className="w-4 h-4 text-accent shrink-0" />
                            ) : (
                              <Circle className="w-4 h-4 text-text-tertiary shrink-0" />
                            )}

                            <span
                              className={`text-sm flex-1 min-w-0 truncate ${
                                completed
                                  ? "text-text-tertiary"
                                  : isNext
                                    ? "text-accent font-medium"
                                    : "text-text-primary"
                              } group-hover:text-accent transition`}
                            >
                              {lesson.title}
                            </span>

                            {mins ? (
                              <span className="text-xs text-text-tertiary flex items-center gap-1 shrink-0">
                                <Clock className="w-3 h-3" />
                                {mins} phút
                              </span>
                            ) : null}
                          </Link>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        ) : (
          <div className="rounded-lg border border-border bg-bg-secondary p-8 text-center">
            <BookOpen className="w-8 h-8 text-text-tertiary mx-auto mb-3" />
            <p className="text-sm text-text-secondary">
              {allLessons.length > 0
                ? "Không có bài giảng nào khớp bộ lọc hiện tại."
                : "Khóa học chưa có nội dung bài giảng."}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}

