"use client";

import React, { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  BookOpen,
  FolderOpen,
  Pencil,
  Plus,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";

interface CourseItem {
  id?: string;
  courseId?: string;
  title: string;
  category?: string;
  totalLessons?: number;
  completedLessons?: number;
  chapters?: { lessons?: unknown[] }[];
}

export default function StudioListPage() {
  const { activeUserHeaderId } = useWorkspaceStore();
  const [courses, setCourses] = useState<CourseItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const fetchCourses = useCallback(async () => {
    setIsLoading(true);
    try {
      const effectiveUserId =
        activeUserHeaderId ??
        (typeof window !== "undefined"
          ? window.localStorage.getItem("swms_active_user_id")
          : null);
      const headers = getClientRequestHeaders(
        effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
      );
      const resp = await fetch("/api/v1/courses", { headers });
      if (resp.ok) {
        const data = await resp.json();
        setCourses(Array.isArray(data) ? data : []);
      }
    } catch {
      /* silent */
    } finally {
      setIsLoading(false);
    }
  }, [activeUserHeaderId]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  const getCourseId = (c: CourseItem) => c.courseId || c.id || "";

  const getLessonCount = (c: CourseItem): number => {
    if (c.totalLessons != null) return c.totalLessons;
    if (c.chapters) {
      return c.chapters.reduce(
        (sum, ch) => sum + (ch.lessons?.length ?? 0),
        0
      );
    }
    return 0;
  };

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
        <h1 className="text-2xl font-bold text-text-primary">Studio</h1>
        <Link
          href="/app/studio/create"
          className="inline-flex items-center gap-2 h-9 px-4 rounded-md bg-accent text-white text-sm font-medium hover:bg-accent-hover transition"
        >
          <Plus className="w-4 h-4" />
          <span>Tạo khóa học mới</span>
        </Link>
      </div>

      {/* Course Grid */}
      {courses.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <div className="w-16 h-16 rounded-full bg-bg-tertiary flex items-center justify-center mb-4">
            <FolderOpen className="w-8 h-8 text-text-tertiary" />
          </div>
          <p className="text-text-secondary text-sm mb-1">
            Chưa có khóa học nào.
          </p>
          <p className="text-text-tertiary text-sm">
            Hãy tạo khóa học đầu tiên!
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {courses.map((course) => {
            const id = getCourseId(course);
            const lessonCount = getLessonCount(course);
            return (
              <div
                key={id}
                className="rounded-lg border border-border bg-bg-primary p-4 flex flex-col"
              >
                <div className="flex items-start justify-between mb-2">
                  <h3 className="text-sm font-medium text-text-primary line-clamp-2 flex-1">
                    {course.title}
                  </h3>
                </div>
                {course.category && (
                  <span className="inline-block text-xs font-medium text-text-tertiary bg-bg-tertiary rounded-md px-2 py-0.5 mb-3 self-start">
                    {course.category}
                  </span>
                )}
                <div className="flex items-center gap-1.5 text-xs text-text-tertiary mb-4">
                  <BookOpen className="w-3.5 h-3.5" />
                  <span>{lessonCount} bài học</span>
                </div>
                <div className="mt-auto">
                  <Link
                    href={`/app/studio/${id}`}
                    className="inline-flex items-center gap-1.5 text-sm font-medium text-accent hover:underline transition"
                  >
                    <Pencil className="w-3.5 h-3.5" />
                    <span>Chỉnh sửa</span>
                  </Link>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
