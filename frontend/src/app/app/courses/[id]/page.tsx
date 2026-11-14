"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeft,
  BookOpen,
  Brain,
  CheckCircle2,
  Circle,
  ClipboardCheck,
  Clock,
  GraduationCap,
  Play,
  Sparkles,
  TrendingUp,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

interface CourseChapter {
  id: string;
  title: string;
  sortOrder?: number;
  lessons: CourseLesson[];
}

interface CourseLesson {
  id: string;
  title: string;
  durationMinutes?: number;
  estimatedMinutes?: number;
  isCompleted?: boolean;
  sortOrder?: number;
  courseId?: string;
}

interface CourseDetail {
  id: string;
  title: string;
  description?: string;
  category?: string;
  visibility?: string;
  progressPercent: number;
  totalLessons: number;
  completedLessons: number;
  completedLessonIds?: string[];
  flashcardCount?: number;
  quizCount?: number;
  quizId?: string | null;
  chapters: CourseChapter[];
}

export default function CourseDetailPage() {
  const params = useParams();
  const router = useRouter();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchCourse = useCallback(async () => {
    if (!courseId) return;
    setIsLoading(true);
    try {
      const effectiveUserId = activeUserHeaderId ?? (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined);

      // Try getting course details - fall back to courses list
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
        const total = Number(
          raw.totalLessons ?? raw.lessonCount ?? flatLessons.length
        );
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
        const found = allCourses.find((c: Record<string, unknown>) =>
          (c.courseId || c.id) === courseId
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
      }

      setCourse(courseData);
    } catch { /* silent */ } finally {
      setIsLoading(false);
    }
  }, [courseId, activeUserHeaderId]);

  useEffect(() => { fetchCourse(); }, [fetchCourse]);

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
        <p className="text-text-secondary mb-4">Không tìm thấy khóa học.</p>
        <Link href="/app" className="text-sm text-accent hover:underline">← Quay lại tổng quan</Link>
      </div>
    );
  }

  const completedSet = new Set(course.completedLessonIds ?? []);
  const allLessons = course.chapters.flatMap((ch) => ch.lessons ?? []);
  const isDone = (lesson: CourseLesson) =>
    Boolean(lesson.isCompleted || completedSet.has(lesson.id));
  const nextLesson = allLessons.find((l) => !isDone(l)) || allLessons[0];
  const continueLearnHref = nextLesson
    ? `/app/courses/${courseId}/learn?lesson=${nextLesson.id}`
    : `/app/courses/${courseId}/learn`;

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-8">
      {/* Back button */}
      <button
        type="button"
        onClick={() => router.back()}
        className="flex items-center gap-1.5 text-sm text-text-secondary hover:text-text-primary transition mb-6 cursor-pointer"
      >
        <ArrowLeft className="w-4 h-4" />
        <span>Quay lại</span>
      </button>

      {/* Course Header */}
      <div className="rounded-lg border border-border bg-bg-secondary p-6 mb-6">
        <div className="flex items-start justify-between gap-4 mb-4">
          <div className="flex-1">
            {course.category && (
              <span className="inline-block text-xs font-medium text-accent bg-accent-subtle rounded-md px-2 py-0.5 mb-2">
                {course.category}
              </span>
            )}
            <h1 className="text-2xl font-bold text-text-primary mb-2">
              {course.title}
            </h1>
            {course.description && (
              <p className="text-sm text-text-secondary line-clamp-2">
                {course.description}
              </p>
            )}
          </div>
        </div>

        {/* Progress bar */}
        <div className="mb-4">
          <div className="flex items-center justify-between text-sm text-text-secondary mb-1.5">
            <span>Tiến độ: {course.completedLessons}/{course.totalLessons} bài</span>
            <span className="font-medium">{course.progressPercent}%</span>
          </div>
          <div className="w-full h-2 bg-bg-tertiary rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all ${course.progressPercent === 100 ? "bg-emerald-500" : "bg-accent"}`}
              style={{ width: `${Math.max(0, Math.min(100, course.progressPercent))}%` }}
            />
          </div>
        </div>

        {/* Action buttons */}
        <div className="flex flex-wrap gap-2">
          <Link
            href={continueLearnHref}
            data-testid="course-continue-learn-btn"
            className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
          >
            <Play className="w-4 h-4" />
            <span>{course.progressPercent > 0 ? "Tiếp tục học" : "Bắt đầu học"}</span>
          </Link>
          <Link
            href={`/app/review?courseId=${courseId}`}
            className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <Brain className="w-4 h-4" />
            <span>Ôn tập{course.flashcardCount ? ` (${course.flashcardCount})` : ""}</span>
          </Link>
          <Link
            href={`/app/courses/${courseId}/quiz`}
            data-testid="course-quiz-btn"
            className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <ClipboardCheck className="w-4 h-4" />
            <span>Làm Quiz{course.quizCount ? ` (${course.quizCount})` : ""}</span>
          </Link>
          <Link
            href={`/app/studio/${courseId}`}
            className="inline-flex items-center gap-2 h-9 px-4 rounded-md border border-border text-text-primary text-sm font-medium hover:bg-bg-tertiary transition"
          >
            <GraduationCap className="w-4 h-4" />
            <span>Studio</span>
          </Link>
        </div>
      </div>

      {/* Course Roadmap */}
      <div className="mb-8">
        <h2 className="text-base font-semibold text-text-primary mb-4">Lộ trình khóa học</h2>

        {course.chapters.length > 0 ? (
          <div className="space-y-4">
            {course.chapters.map((chapter, chIdx) => (
              <div key={chapter.id} className="rounded-lg border border-border bg-bg-primary">
                <div className="px-4 py-3 border-b border-border-subtle">
                  <h3 className="text-sm font-semibold text-text-primary">
                    Chương {chIdx + 1}: {chapter.title}
                  </h3>
                </div>
                <div className="divide-y divide-border-subtle">
                  {(chapter.lessons ?? []).map((lesson) => {
                    const completed = isDone(lesson);
                    const mins = lesson.durationMinutes ?? lesson.estimatedMinutes;
                    return (
                    <Link
                      key={lesson.id}
                      href={`/app/courses/${courseId}/learn?lesson=${lesson.id}`}
                      className="flex items-center gap-3 px-4 py-3 hover:bg-bg-secondary transition group"
                    >
                      {completed ? (
                        <CheckCircle2 className="w-4.5 h-4.5 text-emerald-500 shrink-0" />
                      ) : lesson.id === nextLesson?.id ? (
                        <Play className="w-4.5 h-4.5 text-accent shrink-0" />
                      ) : (
                        <Circle className="w-4.5 h-4.5 text-text-tertiary shrink-0" />
                      )}
                      <span className={`text-sm flex-1 ${
                        completed
                          ? "text-text-tertiary"
                          : lesson.id === nextLesson?.id
                          ? "text-accent font-medium"
                          : "text-text-primary"
                      } group-hover:text-accent transition`}>
                        {lesson.title}
                      </span>
                      {mins && (
                        <span className="text-xs text-text-tertiary flex items-center gap-1">
                          <Clock className="w-3 h-3" />
                          {mins}p
                        </span>
                      )}
                    </Link>
                    );
                  })}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="rounded-lg border border-border bg-bg-secondary p-8 text-center">
            <BookOpen className="w-8 h-8 text-text-tertiary mx-auto mb-3" />
            <p className="text-sm text-text-secondary">Khóa học chưa có nội dung bài giảng.</p>
          </div>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-3">
        <div className="rounded-lg border border-border bg-bg-secondary p-4 text-center">
          <Sparkles className="w-4 h-4 text-text-tertiary mx-auto mb-1.5" />
          <p className="text-lg font-bold text-text-primary">{course.totalLessons}</p>
          <p className="text-xs text-text-tertiary">Bài giảng</p>
        </div>
        <div className="rounded-lg border border-border bg-bg-secondary p-4 text-center">
          <CheckCircle2 className="w-4 h-4 text-emerald-500 mx-auto mb-1.5" />
          <p className="text-lg font-bold text-text-primary">{course.completedLessons}</p>
          <p className="text-xs text-text-tertiary">Đã hoàn thành</p>
        </div>
        <div className="rounded-lg border border-border bg-bg-secondary p-4 text-center">
          <TrendingUp className="w-4 h-4 text-accent mx-auto mb-1.5" />
          <p className="text-lg font-bold text-text-primary">{course.progressPercent}%</p>
          <p className="text-xs text-text-tertiary">Tiến độ</p>
        </div>
      </div>
    </div>
  );
}
