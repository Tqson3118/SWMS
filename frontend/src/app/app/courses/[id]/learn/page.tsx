"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useParams, useSearchParams } from "next/navigation";
import { ArrowLeft, ClipboardCheck, Columns2, Maximize2 } from "lucide-react";
import Link from "next/link";
import {
  useWorkspaceStore,
  getClientRequestHeaders,
} from "@/stores/useWorkspaceStore";
import { ReaderAndSplitViews } from "@/components/reader/ReaderAndSplitViews";
import type {
  DocumentSummary,
  LessonSummary,
  CourseSummaryItemDto,
} from "@/components/reader/ReaderAndSplitViews";

interface CourseWithProgress extends CourseSummaryItemDto {
  completedLessonIds?: string[];
  quizCount?: number;
  quizId?: string | null;
}

export default function LearnPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const courseId = params.id as string;
  const lessonParam = searchParams.get("lesson");
  const docParam = searchParams.get("doc");
  const splitParam = searchParams.get("split") === "true";

  const {
    activeUserHeaderId,
    activeMode,
    activeLessonId,
    setActiveMode,
    setActiveCourseId,
    setActiveLesson,
    setActiveDocument,
  } = useWorkspaceStore();

  const [courseTitle, setCourseTitle] = useState("Đang tải...");
  const [lessonTitle, setLessonTitle] = useState("");
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [lessons, setLessons] = useState<LessonSummary[]>([]);
  const [courses, setCourses] = useState<CourseSummaryItemDto[]>([]);
  const [hasCourseQuiz, setHasCourseQuiz] = useState(false);
  const [isReady, setIsReady] = useState(false);

  const viewMode: "reading" | "split" =
    activeMode === "split" ? "split" : "reading";

  const resolvedActiveLessonTitle = useMemo(() => {
    if (activeLessonId) {
      for (const c of courses) {
        for (const ch of c.chapters ?? []) {
          const found = ch.lessons?.find((l) => l.id === activeLessonId);
          if (found?.title) return found.title;
        }
      }
      const flatFound = lessons.find((l) => l.id === activeLessonId);
      if (flatFound?.title) return flatFound.title;
    }
    return lessonTitle;
  }, [activeLessonId, courses, lessons, lessonTitle]);

  const fetchWorkspaceContext = useCallback(async () => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined"
        ? window.localStorage.getItem("swms_active_user_id")
        : null);
    const headers = getClientRequestHeaders(
      effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : undefined
    );

    const [dashResp, courseResp] = await Promise.all([
      fetch("/api/v1/workspace/dashboard", { headers }).catch(() => null),
      fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
    ]);

    let fetchedCourses: CourseWithProgress[] = [];
    let fetchedDocuments: DocumentSummary[] = [];

    if (dashResp && dashResp.ok) {
      const data = await dashResp.json();
      fetchedCourses = data.courses ?? [];
      fetchedDocuments = data.documents ?? [];
      setDocuments(fetchedDocuments);
      setLessons(data.lessons ?? []);
      setCourses(fetchedCourses);
    }

    let matchedCourse = fetchedCourses.find(
      (c) => (c.courseId || c.id) === courseId
    );
    if (!matchedCourse && courseResp && courseResp.ok) {
      matchedCourse = (await courseResp.json()) as CourseWithProgress;
      if (matchedCourse) {
        setCourses((prev) =>
          prev.some((c) => (c.courseId || c.id) === courseId)
            ? prev
            : [matchedCourse as CourseSummaryItemDto, ...prev]
        );
      }
    }

    if (matchedCourse) {
      setCourseTitle(matchedCourse.title);
      setHasCourseQuiz(
        Boolean(matchedCourse.quizId || (matchedCourse.quizCount ?? 0) > 0)
      );
    }

    return { headers, matchedCourse, fetchedDocuments };
  }, [activeUserHeaderId, courseId]);

  // Initialize workspace store with course and lesson context
  const initializeContext = useCallback(async () => {
    try {
      const { headers, matchedCourse, fetchedDocuments } =
        await fetchWorkspaceContext();

      // Set course context in store
      setActiveCourseId(courseId);
      setActiveMode(splitParam ? "split" : "reading");

      if (docParam) {
        const targetDoc = fetchedDocuments.find((d) => d.id === docParam);
        setActiveDocument(docParam, targetDoc?.totalPages ?? 1);
        if (targetDoc?.title) setLessonTitle(targetDoc.title);
      } else if (lessonParam) {
        try {
          const lessonResp = await fetch(`/api/v1/lessons/${lessonParam}`, {
            headers,
          });
          if (lessonResp.ok) {
            const lessonData = await lessonResp.json();
            setLessonTitle(lessonData.title || "");

            if (lessonData.sourceDocumentId) {
              setActiveDocument(
                lessonData.sourceDocumentId,
                lessonData.totalPages ?? 0
              );
              if (lessonData.startPage) {
                useWorkspaceStore
                  .getState()
                  .setCurrentDocumentPage(lessonData.startPage);
              }
            }
            setActiveLesson(lessonParam);
          } else {
            setActiveLesson(lessonParam);
          }
        } catch {
          setActiveLesson(lessonParam);
        }
      } else if (matchedCourse) {
        // Resolve next incomplete lesson first, falling back to firstLessonId
        const completedSet = new Set(matchedCourse.completedLessonIds ?? []);
        const allCourseLessons = (matchedCourse.chapters ?? []).flatMap(
          (ch) => ch.lessons ?? []
        );
        const nextIncomplete = allCourseLessons.find(
          (l) => !completedSet.has(l.id)
        );
        const resolvedLessonId =
          nextIncomplete?.id ||
          matchedCourse.firstLessonId ||
          allCourseLessons[0]?.id;
        if (resolvedLessonId) {
          setActiveLesson(resolvedLessonId);
          const resolvedTitle = allCourseLessons.find(
            (l) => l.id === resolvedLessonId
          )?.title;
          if (resolvedTitle) setLessonTitle(resolvedTitle);
        }
      }

      setIsReady(true);
    } catch {
      setIsReady(true);
    }
  }, [
    fetchWorkspaceContext,
    courseId,
    lessonParam,
    docParam,
    splitParam,
    setActiveMode,
    setActiveCourseId,
    setActiveLesson,
    setActiveDocument,
  ]);

  useEffect(() => {
    initializeContext();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [courseId, lessonParam, docParam]);

  const handleToggleView = () => {
    const next = viewMode === "reading" ? "split" : "reading";
    setActiveMode(next);
  };

  const handleDataMutated = useCallback(async () => {
    try {
      await fetchWorkspaceContext();
      if (typeof window !== "undefined") {
        window.dispatchEvent(new Event("swms:data-mutated"));
      }
    } catch {
      // ignore transient refresh failure
    }
  }, [fetchWorkspaceContext]);

  if (!isReady) {
    return (
      <div className="flex items-center justify-center h-screen bg-bg-primary">
        <div className="w-6 h-6 border-2 border-accent border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="h-screen flex flex-col bg-bg-primary">
      {/* Minimal top bar */}
      <div className="shrink-0 h-12 flex items-center gap-3 px-4 border-b border-border bg-bg-primary/80 backdrop-blur-sm z-10">
        <Link
          href={`/app/courses/${courseId}`}
          className="p-1.5 rounded-md hover:bg-bg-tertiary transition text-text-secondary hover:text-text-primary"
          title="Quay lại khóa học"
        >
          <ArrowLeft className="w-4 h-4" />
        </Link>

        <div className="flex-1 min-w-0">
          <h1 className="text-sm font-medium text-text-primary truncate">
            {resolvedActiveLessonTitle || courseTitle}
          </h1>
          {resolvedActiveLessonTitle && (
            <p className="text-xs text-text-tertiary truncate">{courseTitle}</p>
          )}
        </div>

        {hasCourseQuiz && (
          <Link
            href={`/app/courses/${courseId}/quiz`}
            className="inline-flex items-center gap-1.5 h-8 px-2.5 rounded-md border border-border text-xs font-medium text-text-secondary hover:text-text-primary hover:bg-bg-tertiary transition"
            title="Làm bài Quiz khóa học"
          >
            <ClipboardCheck className="w-3.5 h-3.5" />
            <span className="hidden sm:inline">Làm Quiz</span>
          </Link>
        )}

        <button
          onClick={handleToggleView}
          className={`p-1.5 rounded-md transition cursor-pointer ${
            viewMode === "split"
              ? "bg-accent/10 text-accent"
              : "hover:bg-bg-tertiary text-text-secondary hover:text-text-primary"
          }`}
          title={viewMode === "split" ? "Chế độ đọc" : "Chế độ chia đôi"}
        >
          {viewMode === "split" ? (
            <Maximize2 className="w-4 h-4" />
          ) : (
            <Columns2 className="w-4 h-4" />
          )}
        </button>
      </div>

      {/* Reader content — full remaining height */}
      <div
        data-swms-scroll-container="learn-page"
        className={`flex-1 min-h-0 ${
          viewMode === "split" ? "overflow-hidden" : "overflow-y-auto"
        }`}
      >
        <ReaderAndSplitViews
          mode={viewMode}
          onDataMutated={handleDataMutated}
          documents={documents}
          lessons={lessons}
          courses={courses}
        />
      </div>
    </div>
  );
}
