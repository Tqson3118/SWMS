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
import { Button, Card, Badge, EmptyState } from "@/components/ui";

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
        <div className="w-6 h-6 border-2 border-[#5E51E8] border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-10 min-h-[80vh]">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-[var(--text-primary)]">Studio</h1>
          <p className="text-sm text-[var(--text-secondary)] mt-1">
            Không gian biên tập và thiết kế đề cương khóa học
          </p>
        </div>
        <Link href="/app/studio/create">
          <Button variant="primary" leftIcon={<Plus className="w-4 h-4" />}>
            <span>Tạo khóa học mới</span>
          </Button>
        </Link>
      </div>

      {/* Course Grid */}
      {courses.length === 0 ? (
        <EmptyState
          icon={FolderOpen}
          title="Chưa có khóa học nào"
          description="Hãy tạo khóa học đầu tiên để bắt đầu biên tập lộ trình học tập."
          action={{
            label: "Tạo khóa học mới",
            href: "/app/studio/create",
            icon: <Plus className="w-4 h-4" />,
          }}
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-5">
          {courses.map((course) => {
            const id = getCourseId(course);
            const lessonCount = getLessonCount(course);
            return (
              <Card
                key={id}
                hoverable
                className="p-5 flex flex-col justify-between"
              >
                <div className="min-w-0">
                  <div className="flex items-start justify-between mb-2">
                    <h3
                      title={course.title}
                      className="text-sm sm:text-base font-semibold text-[var(--text-primary)] line-clamp-2 flex-1 min-w-0 break-words"
                    >
                      {course.title}
                    </h3>
                  </div>
                  {course.category && (
                    <Badge variant="neutral" size="sm" className="mb-3">
                      {course.category}
                    </Badge>
                  )}
                  <div className="flex items-center gap-1.5 text-xs text-[var(--text-tertiary)] mb-4">
                    <BookOpen className="w-3.5 h-3.5" />
                    <span>{lessonCount} bài học</span>
                  </div>
                </div>
                <div className="pt-2 border-t border-[var(--border-calm)]">
                  <Link href={`/app/studio/${id}`}>
                    <Button variant="secondary" size="sm" className="w-full" leftIcon={<Pencil className="w-3.5 h-3.5" />}>
                      Chỉnh sửa
                    </Button>
                  </Link>
                </div>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
