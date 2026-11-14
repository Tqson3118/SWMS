"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  ArrowRight,
  BookOpen,
  Brain,
  Clock,
  Flame,
  FolderOpen,
  Plus,
  Search,
  Sparkles,
  TrendingUp,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

interface DashboardUser {
  id: string;
  fullName: string;
  email: string;
  streakDays: number;
}

interface ReviewStation {
  totalDue: number;
  totalCards: number;
  newCount: number;
  learningCount: number;
  reviewCount: number;
  estimatedMinutes: number;
}

interface CourseItem {
  id?: string;
  courseId?: string;
  title: string;
  category?: string;
  progressPercent?: number;
  progress?: number;
  totalLessons?: number;
  completedLessons?: number;
  chapters?: { lessons?: unknown[] }[];
}

export default function DashboardPage() {
  const { activeUserHeaderId, setCommandPaletteOpen } = useWorkspaceStore();
  const [user, setUser] = useState<DashboardUser>({ id: "", fullName: "Học viên", email: "", streakDays: 0 });
  const [reviewStation, setReviewStation] = useState<ReviewStation>({ totalDue: 0, totalCards: 0, newCount: 0, learningCount: 0, reviewCount: 0, estimatedMinutes: 0 });
  const [courses, setCourses] = useState<CourseItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const effectiveUserId = activeUserHeaderId ?? (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
      const headers = getClientRequestHeaders(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined);
      const [dashResp, coursesResp] = await Promise.all([
        fetch("/api/v1/workspace/dashboard", { headers }),
        fetch("/api/v1/courses", { headers }).catch(() => null),
      ]);
      if (dashResp.ok) {
        const data = await dashResp.json();
        setUser(data.user ?? { id: "", fullName: "Học viên", email: "", streakDays: 0 });
        setReviewStation(data.reviewStation ?? { totalDue: 0, totalCards: 0, newCount: 0, learningCount: 0, reviewCount: 0, estimatedMinutes: 0 });
        const extraCourses = coursesResp && coursesResp.ok ? await coursesResp.json().catch(() => null) : null;
        setCourses(Array.isArray(extraCourses) && extraCourses.length > 0 ? extraCourses : data.courses ?? []);
      }
    } catch { /* silent */ } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const firstName = user.fullName.split(" ").pop() || "bạn";

  // Find the course with lowest non-zero progress for "continue learning"
  const continueCourse = courses.find(
    (c) => (c.progressPercent ?? c.progress ?? 0) > 0 && (c.progressPercent ?? c.progress ?? 0) < 100
  ) || courses[0];

  const getCourseId = (c: CourseItem) => c.courseId || c.id || "";
  const getProgress = (c: CourseItem) => c.progressPercent ?? c.progress ?? 0;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pb-20 md:pb-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">
            Chào {firstName}, tiếp tục học nào.
          </h1>
          {user.streakDays > 0 && (
            <div className="flex items-center gap-1.5 mt-1.5 text-sm text-text-secondary">
              <Flame className="w-4 h-4 text-amber-500" />
              <span>{user.streakDays} ngày liên tục</span>
            </div>
          )}
        </div>
        <button
          type="button"
          onClick={() => setCommandPaletteOpen(true)}
          className="hidden sm:flex items-center gap-2 h-9 px-3 rounded-md border border-border bg-bg-secondary text-text-tertiary text-sm hover:text-text-secondary transition cursor-pointer"
        >
          <Search className="w-4 h-4" />
          <span>Tìm kiếm...</span>
          <kbd className="text-[10px] font-mono px-1 py-0.5 rounded bg-bg-tertiary border border-border text-text-tertiary ml-2">⌘K</kbd>
        </button>
      </div>

      {/* Two main cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-10">
        {/* Continue Learning Card */}
        {continueCourse && (
          <Link
            href={`/app/courses/${getCourseId(continueCourse)}/learn`}
            className="group block rounded-lg border border-border bg-bg-secondary p-5 hover:border-accent/50 transition"
          >
            <div className="flex items-center gap-2 text-sm text-text-secondary mb-3">
              <BookOpen className="w-4 h-4" />
              <span>Tiếp tục học</span>
            </div>
            <h3 className="text-base font-semibold text-text-primary mb-1 group-hover:text-accent transition">
              {continueCourse.title}
            </h3>
            <p className="text-sm text-text-tertiary mb-3">
              Tiến độ {getProgress(continueCourse)}%
            </p>
            {/* Progress bar */}
            <div className="w-full h-1.5 bg-bg-tertiary rounded-full overflow-hidden mb-3">
              <div
                className="h-full bg-accent rounded-full transition-all"
                style={{ width: `${Math.max(0, Math.min(100, getProgress(continueCourse)))}%` }}
              />
            </div>
            <div className="flex items-center gap-1.5 text-sm font-medium text-accent">
              <span>Tiếp tục</span>
              <ArrowRight className="w-3.5 h-3.5 group-hover:translate-x-0.5 transition-transform" />
            </div>
          </Link>
        )}

        {/* Review Card */}
        <Link
          href="/app/review"
          className="group block rounded-lg border border-border bg-bg-secondary p-5 hover:border-accent/50 transition"
        >
          <div className="flex items-center gap-2 text-sm text-text-secondary mb-3">
            <Brain className="w-4 h-4" />
            <span>Ôn tập hôm nay</span>
          </div>
          <div className="flex items-baseline gap-2 mb-1">
            <span className="text-3xl font-bold text-text-primary">
              {reviewStation.totalDue}
            </span>
            <span className="text-sm text-text-secondary">thẻ cần ôn</span>
          </div>
          {reviewStation.estimatedMinutes > 0 && (
            <p className="text-sm text-text-tertiary mb-3 flex items-center gap-1.5">
              <Clock className="w-3.5 h-3.5" />
              ~{reviewStation.estimatedMinutes} phút
            </p>
          )}
          <div className="flex items-center gap-1.5 text-sm font-medium text-accent">
            <span>{reviewStation.totalDue > 0 ? "Bắt đầu ôn tập" : "Không có thẻ nào"}</span>
            <ArrowRight className="w-3.5 h-3.5 group-hover:translate-x-0.5 transition-transform" />
          </div>
        </Link>
      </div>

      {/* Courses Section */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-base font-semibold text-text-primary">
            Khóa học
          </h2>
          <Link
            href="/app/library"
            className="text-sm text-text-tertiary hover:text-accent transition flex items-center gap-1"
          >
            <span>Xem tất cả</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {courses.slice(0, 6).map((course) => {
            const id = getCourseId(course);
            const progress = getProgress(course);
            return (
              <Link
                key={id}
                href={`/app/courses/${id}`}
                className="group rounded-lg border border-border bg-bg-primary p-4 hover:border-accent/50 transition"
              >
                <div className="flex items-start justify-between mb-2">
                  <h3 className="text-sm font-medium text-text-primary group-hover:text-accent transition line-clamp-2 flex-1">
                    {course.title}
                  </h3>
                </div>
                {course.category && (
                  <span className="inline-block text-xs font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mb-3">
                    {course.category}
                  </span>
                )}
                {/* Progress */}
                <div className="mt-auto">
                  <div className="flex items-center justify-between text-xs text-text-tertiary mb-1.5">
                    <span>{progress}% hoàn thành</span>
                    {progress === 100 && <span className="text-emerald-600 dark:text-emerald-400 font-medium">✓ Xong</span>}
                  </div>
                  <div className="w-full h-1 bg-bg-tertiary rounded-full overflow-hidden">
                    <div
                      className={`h-full rounded-full transition-all ${progress === 100 ? "bg-emerald-500" : "bg-accent"}`}
                      style={{ width: `${Math.max(0, Math.min(100, progress))}%` }}
                    />
                  </div>
                </div>
              </Link>
            );
          })}

          {/* Create new course card */}
          <Link
            href="/app/studio/create"
            className="group rounded-lg border border-dashed border-border bg-bg-primary p-4 hover:border-accent hover:bg-accent-subtle transition flex flex-col items-center justify-center gap-2 min-h-[120px]"
          >
            <div className="w-10 h-10 rounded-lg bg-bg-tertiary group-hover:bg-accent/10 flex items-center justify-center transition">
              <Plus className="w-5 h-5 text-text-tertiary group-hover:text-accent transition" />
            </div>
            <span className="text-sm font-medium text-text-tertiary group-hover:text-accent transition">
              Tạo khóa học
            </span>
          </Link>
        </div>
      </div>

      {/* Quick stats */}
      <div className="grid grid-cols-3 gap-3">
        <div className="rounded-lg border border-border bg-bg-secondary p-4">
          <div className="flex items-center gap-2 text-text-tertiary mb-2">
            <FolderOpen className="w-4 h-4" />
            <span className="text-xs font-medium">Khóa học</span>
          </div>
          <p className="text-xl font-bold text-text-primary">{courses.length}</p>
        </div>
        <div className="rounded-lg border border-border bg-bg-secondary p-4">
          <div className="flex items-center gap-2 text-text-tertiary mb-2">
            <Sparkles className="w-4 h-4" />
            <span className="text-xs font-medium">Thẻ FSRS</span>
          </div>
          <p className="text-xl font-bold text-text-primary">{reviewStation.totalCards}</p>
        </div>
        <div className="rounded-lg border border-border bg-bg-secondary p-4">
          <div className="flex items-center gap-2 text-text-tertiary mb-2">
            <TrendingUp className="w-4 h-4" />
            <span className="text-xs font-medium">Chuỗi ngày</span>
          </div>
          <p className="text-xl font-bold text-text-primary">{user.streakDays}</p>
        </div>
      </div>
    </div>
  );
}
