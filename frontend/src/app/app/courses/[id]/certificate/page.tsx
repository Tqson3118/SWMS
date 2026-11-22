"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  AlertCircle,
  ArrowLeft,
  Award,
  BookOpen,
  Brain,
  CheckCircle2,
  ClipboardCheck,
  Clock,
  Flame,
  FlaskConical,
  Printer,
  RefreshCw,
  ShieldCheck,
  Sparkles,
  Trophy,
} from "lucide-react";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { fetchDashboardCached } from "@/lib/api-client";
import { Button, Card, Badge } from "@/components/ui";
import {
  classifyCourseDomain,
  extractCourseSkillTags,
  getCourseTotalMinutes,
  getCourseVisualTheme,
} from "@/lib/course-helpers";

interface CertificateCourseData {
  id: string;
  title: string;
  description?: string;
  category?: string;
  progressPercent: number;
  totalLessons: number;
  completedLessons: number;
  totalEstimatedMinutes?: number;
  flashcardCount?: number;
  dueFlashcardCount?: number;
  masteredFlashcardCount?: number;
  practiceTotalActivities?: number;
  practicePassedActivities?: number;
  practiceAverageScore?: number;
  quizCount?: number;
  chapters?: {
    id: string;
    title: string;
    lessons?: {
      id: string;
      title: string;
      estimatedMinutes?: number;
      durationMinutes?: number;
      isCompleted?: boolean;
    }[];
  }[];
  completedAt?: string | null;
}

interface QuizTranscriptStats {
  totalQuizzes: number;
  completedQuizzes: number;
  totalItems: number;
  correctItems: number;
  accuracyPercent: number;
}

export default function CourseCertificatePage() {
  const params = useParams();
  const courseId = params.id as string;
  const { activeUserHeaderId } = useWorkspaceStore();

  const [course, setCourse] = useState<CertificateCourseData | null>(null);
  const [quizTranscript, setQuizTranscript] = useState<QuizTranscriptStats>({
    totalQuizzes: 0,
    completedQuizzes: 0,
    totalItems: 0,
    correctItems: 0,
    accuracyPercent: 0,
  });
  const [learnerName, setLearnerName] = useState<string>("Học viên Studxy");
  const [learnerEmail, setLearnerEmail] = useState<string>("");
  const [studiedMinutes, setStudiedMinutes] = useState<number>(0);
  const [completedDateStr, setCompletedDateStr] = useState<string>("");
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
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

      const [courseResp, resumeResp, meResp, quizzesResp] = await Promise.all([
        fetch(`/api/v1/courses/${courseId}`, { headers }).catch(() => null),
        fetch(`/api/v1/courses/${courseId}/resume-state`, { headers }).catch(() => null),
        fetch("/api/v1/auth/me", { headers }).catch(() => null),
        fetch(`/api/v1/practice-quizzes?courseId=${encodeURIComponent(courseId)}`, { headers }).catch(() => null),
      ]);

      const needsDashFallback = !(courseResp && courseResp.ok) || !(meResp && meResp.ok);
      const dashData = needsDashFallback
        ? await fetchDashboardCached({ headers }).catch(() => null)
        : null;

      if (quizzesResp && quizzesResp.ok) {
        const qList = await quizzesResp.json().catch(() => []);
        if (Array.isArray(qList)) {
          const totalQuizzes = qList.length;
          const completedQuizzes = qList.filter(
            (q: { status?: string; totalItems?: number; correctItems?: number }) =>
              q.status === "completed" || Number(q.correctItems ?? 0) > 0
          ).length;
          const totalItems = qList.reduce(
            (sum: number, q: { totalItems?: number }) => sum + Number(q.totalItems ?? 0),
            0
          );
          const correctItems = qList.reduce(
            (sum: number, q: { correctItems?: number }) => sum + Number(q.correctItems ?? 0),
            0
          );
          setQuizTranscript({
            totalQuizzes,
            completedQuizzes,
            totalItems,
            correctItems,
            accuracyPercent: totalItems > 0 ? Math.round((correctItems * 100) / totalItems) : 0,
          });
        }
      }

      if (meResp && meResp.ok) {
        const me = await meResp.json().catch(() => null);
        if (me) {
          setLearnerName(String(me.fullName || me.name || me.username || "Học viên Studxy"));
          setLearnerEmail(String(me.email || ""));
        }
      } else if (dashData?.user) {
        setLearnerName(String(dashData.user.fullName || "Học viên Studxy"));
        setLearnerEmail(String(dashData.user.email || ""));
      } else if (typeof window !== "undefined") {
        const storedUser = window.localStorage.getItem("swms_active_user");
        if (storedUser) {
          try {
            const parsed = JSON.parse(storedUser);
            if (parsed?.fullName) setLearnerName(String(parsed.fullName));
            if (parsed?.email) setLearnerEmail(String(parsed.email));
          } catch {
            // ignore JSON error
          }
        }
      }

      let resolvedCourse: CertificateCourseData | null = null;
      if (courseResp && courseResp.ok) {
        const raw = await courseResp.json();
        const chaps = Array.isArray(raw.chapters) ? raw.chapters : [];
        const flatLessons = chaps.flatMap((c: { lessons?: unknown[] }) => c.lessons ?? []);
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
        resolvedCourse = {
          id: String(raw.courseId || raw.id || courseId),
          title: String(raw.title || "Khóa học"),
          description: String(raw.description || ""),
          category: String(raw.category || ""),
          progressPercent: progress,
          totalLessons: total,
          completedLessons: completed,
          totalEstimatedMinutes: Number(raw.totalEstimatedMinutes ?? 0) || undefined,
          flashcardCount: Number(raw.flashcardCount ?? 0),
          dueFlashcardCount: Number(raw.dueFlashcardCount ?? 0),
          masteredFlashcardCount: Number(raw.masteredFlashcardCount ?? 0),
          practiceTotalActivities: Number(raw.practiceTotalActivities ?? 0),
          practicePassedActivities: Number(raw.practicePassedActivities ?? 0),
          practiceAverageScore: Number(raw.practiceAverageScore ?? 0),
          quizCount: Number(raw.quizCount ?? 0),
          chapters: chaps,
          completedAt: raw.completedAt ?? raw.updatedAt ?? null,
        };
      } else if (dashData?.courses) {
        const found = (dashData.courses as Record<string, unknown>[]).find(
          (c) => (c.courseId || c.id) === courseId
        );
        if (found) {
          const total = Number(found.totalLessons ?? found.lessonCount ?? 0);
          const completed = Number(found.completedLessons ?? 0);
          resolvedCourse = {
            id: String(found.courseId || found.id || courseId),
            title: String(found.title || "Khóa học"),
            description: String(found.description || ""),
            category: String(found.category || ""),
            progressPercent: Number(
              found.progressPercent ??
                found.progress ??
                (total > 0 ? Math.round((completed * 100) / total) : 0)
            ),
            totalLessons: total,
            completedLessons: completed,
            totalEstimatedMinutes: Number(found.totalEstimatedMinutes ?? 0) || undefined,
            flashcardCount: Number(found.flashcardCount ?? 0),
            dueFlashcardCount: Number(found.dueFlashcardCount ?? 0),
            masteredFlashcardCount: Number(found.masteredFlashcardCount ?? 0),
            practiceTotalActivities: Number(found.practiceTotalActivities ?? 0),
            practicePassedActivities: Number(found.practicePassedActivities ?? 0),
            practiceAverageScore: Number(found.practiceAverageScore ?? 0),
            quizCount: Number(found.quizCount ?? 0),
          };
        }
      }

      if (!resolvedCourse && courseResp && !courseResp.ok) {
        setFetchError(`Không tải được dữ liệu khóa học (HTTP ${courseResp.status}).`);
      }

      setCourse(resolvedCourse);

      let realStudiedMins = 0;
      let accessDate: string | null = resolvedCourse?.completedAt ?? null;

      if (resumeResp && resumeResp.ok) {
        const rawResume = await resumeResp.json().catch(() => null);
        if (rawResume && typeof rawResume === "object") {
          const secs = Number(
            rawResume.totalTimeSpentSeconds ??
              rawResume.totalStudiedSeconds ??
              rawResume.timeSpentSeconds ??
              0
          );
          const mins = Number(
            rawResume.totalStudiedMinutes ??
              rawResume.totalTimeSpentMinutes ??
              rawResume.timeSpentMinutes ??
              0
          );
          if (secs > 0) realStudiedMins = Math.max(1, Math.round(secs / 60));
          else if (mins > 0) realStudiedMins = Math.round(mins);
          if (rawResume.lastAccessedAt || rawResume.updatedAt) {
            accessDate = String(rawResume.lastAccessedAt || rawResume.updatedAt);
          }
        }
      }

      if (realStudiedMins === 0 && resolvedCourse) {
        const totalEst = getCourseTotalMinutes(resolvedCourse);
        if (resolvedCourse.progressPercent > 0) {
          realStudiedMins = Math.max(
            15,
            Math.round((totalEst * resolvedCourse.progressPercent) / 100)
          );
        }
      }
      setStudiedMinutes(realStudiedMins);

      const dateObj = accessDate ? new Date(accessDate) : new Date();
      const validDate = Number.isNaN(dateObj.getTime()) ? new Date() : dateObj;
      setCompletedDateStr(
        validDate.toLocaleDateString("vi-VN", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        })
      );
    } catch {
      setFetchError("Không thể kết nối tới máy chủ.");
    } finally {
      setIsLoading(false);
    }
  }, [courseId, activeUserHeaderId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const certificateCode = useMemo(() => {
    const cleanId = (courseId || "COURSE")
      .replace(/[^a-zA-Z0-9]/g, "")
      .toUpperCase()
      .slice(0, 8);
    return `STX-${cleanId}-${(course?.progressPercent ?? 0) === 100 ? "100" : "PRO"}`;
  }, [courseId, course?.progressPercent]);

  if (isLoading) {
    return (
      <div className="max-w-5xl mx-auto px-4 sm:px-6 py-8 space-y-6">
        <div className="h-8 w-48 rounded bg-[var(--surface-elevated)] animate-pulse" />
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {[0, 1, 2].map((i) => (
            <div key={i} className="h-24 rounded-xl bg-[var(--surface-elevated)] animate-pulse" />
          ))}
        </div>
        <div className="h-96 rounded-2xl bg-[var(--surface-elevated)] animate-pulse" />
      </div>
    );
  }

  if (!course) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12 text-center space-y-4">
        {fetchError && (
          <div className="rounded-lg border border-red-500/30 bg-red-500/5 px-4 py-3 text-sm text-red-600 dark:text-red-400 inline-flex items-center gap-2">
            <AlertCircle className="w-4 h-4" />
            <span>{fetchError}</span>
          </div>
        )}
        <p className="text-sm text-[var(--text-secondary)]">Không tìm thấy thông tin khóa học.</p>
        <div className="flex items-center justify-center gap-3">
          <Button
            variant="secondary"
            size="sm"
            onClick={loadData}
            leftIcon={<RefreshCw className="w-3.5 h-3.5" />}
          >
            Thử lại
          </Button>
          <Link href="/app/courses" className="text-sm text-[#5E51E8] hover:underline">
            ← Về danh sách khóa học
          </Link>
        </div>
      </div>
    );
  }

  const domain = classifyCourseDomain(course);
  const theme = getCourseVisualTheme(course);
  const skillTags = extractCourseSkillTags(course);
  const totalCourseMinutes = getCourseTotalMinutes(course);
  const studiedHoursText =
    studiedMinutes >= 60
      ? `${(studiedMinutes / 60).toFixed(1).replace(/\.0$/, "")} giờ (${studiedMinutes} phút)`
      : `${studiedMinutes} phút`;

  const milestones = [
    {
      id: "starter",
      title: "Khởi động Lộ trình",
      desc: "Bắt đầu bài học đầu tiên trong khóa",
      unlocked: course.completedLessons > 0 || course.progressPercent > 0,
      icon: Flame,
    },
    {
      id: "halfway",
      title: "Kiên trì 50%",
      desc: "Hoàn thành tối thiểu một nửa chặng đường",
      unlocked: course.progressPercent >= 50,
      icon: Sparkles,
    },
    {
      id: "mastery",
      title: "Hoàn tất Lộ trình 100%",
      desc: "Chinh phục toàn bộ giáo trình & phòng thực hành",
      unlocked: course.progressPercent >= 100,
      icon: Trophy,
    },
  ];

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-24 md:pb-12 space-y-6 print:p-0 print:m-0 print:max-w-none">
      {/* Print-specific A4 stylesheet */}
      <style>{`
        @media print {
          @page {
            size: A4 landscape;
            margin: 10mm;
          }
          body {
            background: #ffffff !important;
            color: #0f172a !important;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
          }
          aside, nav, header, footer, [data-testid="app-sidebar"] {
            display: none !important;
          }
        }
      `}</style>

      {/* Top Navigation & Actions (Hidden in Print) */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 print:hidden">
        <div className="flex items-center gap-2 text-sm">
          <Link
            href={`/app/courses/${courseId}`}
            className="inline-flex items-center gap-1.5 text-[var(--text-secondary)] hover:text-[var(--text-primary)] font-medium transition"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Quay lại khóa học</span>
          </Link>
          <span className="text-[var(--text-tertiary)]">·</span>
          <span className="text-[var(--text-primary)] font-semibold truncate max-w-xs sm:max-w-md">
            Tổng kết &amp; Chứng nhận
          </span>
        </div>

        <div className="flex items-center gap-2.5">
          <Button
            variant="primary"
            onClick={() => window.print()}
            leftIcon={<Printer className="w-4 h-4" />}
            data-testid="print-certificate-btn"
          >
            <span>In / Xuất Chứng nhận PDF</span>
          </Button>
        </div>
      </div>

      {/* Summary Stats & Milestones (Hidden in Print) */}
      <div className="space-y-4 print:hidden">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="p-4 bg-[var(--surface-card)] flex items-center gap-3.5">
            <div className="w-11 h-11 rounded-xl bg-[#EEF0FD] dark:bg-[#1E1B4B] text-[#4F46E5] dark:text-[#A5B4FC] flex items-center justify-center shrink-0">
              <Award className="w-5 h-5" />
            </div>
            <div>
              <p className="text-xs text-[var(--text-tertiary)] font-medium">Tỷ lệ hoàn thành</p>
              <p className="text-xl font-bold text-[var(--text-primary)]">
                {course.progressPercent}%
              </p>
            </div>
          </Card>

          <Card className="p-4 bg-[var(--surface-card)] flex items-center gap-3.5">
            <div className="w-11 h-11 rounded-xl bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
              <BookOpen className="w-5 h-5" />
            </div>
            <div>
              <p className="text-xs text-[var(--text-tertiary)] font-medium">Số bài đã học</p>
              <p className="text-xl font-bold text-[var(--text-primary)]">
                {course.completedLessons} / {course.totalLessons} bài
              </p>
            </div>
          </Card>

          <Card className="p-4 bg-[var(--surface-card)] flex items-center gap-3.5">
            <div className="w-11 h-11 rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400 flex items-center justify-center shrink-0">
              <Clock className="w-5 h-5" />
            </div>
            <div>
              <p className="text-xs text-[var(--text-tertiary)] font-medium">
                Tổng thời gian học thật
              </p>
              <p className="text-lg font-bold text-[var(--text-primary)]">{studiedHoursText}</p>
            </div>
          </Card>
        </div>

        {/* Achievement Badges */}
        <Card className="p-5 bg-[var(--surface-card)]">
          <h2 className="text-xs font-bold uppercase tracking-wider text-[var(--text-tertiary)] mb-3">
            Huy hiệu cột mốc đạt được
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {milestones.map((m) => {
              const Icon = m.icon;
              return (
                <div
                  key={m.id}
                  className={`flex items-start gap-3 p-3.5 rounded-xl border transition ${
                    m.unlocked
                      ? "border-amber-400/50 bg-amber-50/50 dark:bg-amber-950/20"
                      : "border-[var(--border-calm)] bg-[var(--surface-elevated)]/40 opacity-60"
                  }`}
                >
                  <div
                    className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${
                      m.unlocked
                        ? "bg-amber-500 text-white shadow-xs"
                        : "bg-slate-200 dark:bg-slate-800 text-slate-500"
                    }`}
                  >
                    <Icon className="w-4 h-4" />
                  </div>
                  <div className="min-w-0">
                    <div className="flex items-center gap-1.5">
                      <p className="text-xs font-bold text-[var(--text-primary)]">{m.title}</p>
                      {m.unlocked && (
                        <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400 shrink-0" />
                      )}
                    </div>
                    <p className="text-[11px] text-[var(--text-secondary)] mt-0.5 leading-snug">
                      {m.desc}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </Card>
      </div>

      {/* Printable Certificate of Completion Canvas */}
      <div
        data-testid="certificate-canvas"
        className="relative rounded-2xl bg-gradient-to-br from-amber-50/70 via-white to-indigo-50/60 dark:from-slate-900 dark:via-slate-900 dark:to-indigo-950/50 p-4 sm:p-8 shadow-lg border-2 border-amber-500/40 print:shadow-none print:border-4 print:border-amber-600 print:rounded-none print:bg-white print:text-slate-900"
      >
        {/* Classic Double Ornamental Inner Frame */}
        <div className="relative border-2 border-amber-600/40 dark:border-amber-400/30 rounded-xl p-6 sm:p-10 text-center space-y-6 bg-white/85 dark:bg-slate-950/75 backdrop-blur-xs print:bg-white print:border-2 print:border-amber-700/60">
          {/* Corner Ornaments */}
          <div className="w-6 h-6 border-t-2 border-l-2 border-amber-600 absolute top-3 left-3" />
          <div className="w-6 h-6 border-t-2 border-r-2 border-amber-600 absolute top-3 right-3" />
          <div className="w-6 h-6 border-b-2 border-l-2 border-amber-600 absolute bottom-3 left-3" />
          <div className="w-6 h-6 border-b-2 border-r-2 border-amber-600 absolute bottom-3 right-3" />

          {/* Header Seal */}
          <div className="flex flex-col items-center gap-2">
            <div className="w-14 h-14 rounded-full bg-gradient-to-br from-amber-500 via-amber-600 to-yellow-700 text-white flex items-center justify-center shadow-md border-4 border-amber-100 dark:border-amber-900/60">
              <Award className="w-7 h-7" />
            </div>
            <p className="text-xs font-bold uppercase tracking-[0.25em] text-amber-700 dark:text-amber-400">
              Studxy Academic Learning Platform
            </p>
            <h1 className="text-2xl sm:text-4xl font-serif font-bold tracking-tight text-slate-900 dark:text-white print:text-slate-900">
              CHỨNG NHẬN HOÀN THÀNH
            </h1>
            <p className="text-xs uppercase tracking-widest text-slate-500 dark:text-slate-400">
              Certificate of Course Completion
            </p>
          </div>

          {/* Recipient */}
          <div className="space-y-2 py-2">
            <p className="text-xs sm:text-sm italic text-slate-600 dark:text-slate-300">
              Trân trọng trao tặng chứng nhận này cho học viên:
            </p>
            <p
              data-testid="certificate-learner-name"
              className="text-2xl sm:text-3xl font-bold text-[#4F46E5] dark:text-[#A5B4FC] print:text-indigo-800 border-b-2 border-amber-500/40 inline-block px-8 pb-1.5"
            >
              {learnerName}
            </p>
            {learnerEmail && (
              <p className="text-xs text-slate-500 dark:text-slate-400">{learnerEmail}</p>
            )}
          </div>

          {/* Course Details */}
          <div className="max-w-2xl mx-auto space-y-3">
            <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-300">
              {course.progressPercent >= 100
                ? "Đã hoàn thành xuất sắc 100% chương trình đào tạo và bài tập thực hành của khóa học:"
                : `Đã tham gia và hoàn thành ${course.progressPercent}% (${course.completedLessons}/${course.totalLessons} bài giảng) thuộc lộ trình:`}
            </p>
            <h2
              data-testid="certificate-course-title"
              className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white print:text-slate-900 leading-snug"
            >
              {course.title}
            </h2>

            <div className="flex flex-wrap items-center justify-center gap-2 pt-1">
              <Badge variant="neutral" size="sm">
                {domain.label}
              </Badge>
              <Badge variant="neutral" size="sm">
                {theme.levelLabel}
              </Badge>
              {skillTags.map((tag) => (
                <Badge key={tag} variant="brand" size="sm">
                  {tag}
                </Badge>
              ))}
            </div>
          </div>

          {/* 4-Dimensional Academic Transcript */}
          <div
            data-testid="certificate-academic-transcript"
            className="max-w-3xl mx-auto rounded-xl border border-slate-200 dark:border-slate-800 print:border-slate-300 bg-slate-50/70 dark:bg-slate-900/60 print:bg-slate-50 p-4 text-left space-y-3"
          >
            <p className="text-[11px] font-bold uppercase tracking-wider text-slate-500 dark:text-slate-400 print:text-slate-600 text-center">
              Bảng Điểm Học Thuật Toàn Diện (4 Chiều Năng Lực)
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
              <div className="rounded-lg border border-slate-200 dark:border-slate-700/80 print:border-slate-300 bg-white dark:bg-slate-950/60 print:bg-white p-3">
                <div className="flex items-center gap-1.5 text-[11px] font-semibold text-indigo-700 dark:text-indigo-300 print:text-indigo-700">
                  <BookOpen className="w-3.5 h-3.5 shrink-0" />
                  <span>Bài giảng Lý thuyết</span>
                </div>
                <p className="text-base font-bold text-slate-900 dark:text-white print:text-slate-900 mt-1">
                  {course.completedLessons}/{course.totalLessons} bài
                </p>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  Tiến độ {course.progressPercent}%
                </p>
              </div>

              <div className="rounded-lg border border-slate-200 dark:border-slate-700/80 print:border-slate-300 bg-white dark:bg-slate-950/60 print:bg-white p-3">
                <div className="flex items-center gap-1.5 text-[11px] font-semibold text-emerald-700 dark:text-emerald-300 print:text-emerald-700">
                  <FlaskConical className="w-3.5 h-3.5 shrink-0" />
                  <span>Practice Lab</span>
                </div>
                <p className="text-base font-bold text-slate-900 dark:text-white print:text-slate-900 mt-1">
                  {course.practicePassedActivities ?? 0}/{course.practiceTotalActivities ?? 0} bài đạt
                </p>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  Điểm TB: {(course.practiceAverageScore ?? 0) > 0 ? `${course.practiceAverageScore}%` : "Chưa chấm"}
                </p>
              </div>

              <div className="rounded-lg border border-slate-200 dark:border-slate-700/80 print:border-slate-300 bg-white dark:bg-slate-950/60 print:bg-white p-3">
                <div className="flex items-center gap-1.5 text-[11px] font-semibold text-sky-700 dark:text-sky-300 print:text-sky-700">
                  <ClipboardCheck className="w-3.5 h-3.5 shrink-0" />
                  <span>Kiểm tra &amp; Đề thi</span>
                </div>
                <p className="text-base font-bold text-slate-900 dark:text-white print:text-slate-900 mt-1">
                  {quizTranscript.completedQuizzes}/{quizTranscript.totalQuizzes || course.quizCount || 0} đề
                </p>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  Chính xác: {quizTranscript.correctItems}/{quizTranscript.totalItems} ({quizTranscript.accuracyPercent}%)
                </p>
              </div>

              <div className="rounded-lg border border-slate-200 dark:border-slate-700/80 print:border-slate-300 bg-white dark:bg-slate-950/60 print:bg-white p-3">
                <div className="flex items-center gap-1.5 text-[11px] font-semibold text-amber-700 dark:text-amber-300 print:text-amber-700">
                  <Brain className="w-3.5 h-3.5 shrink-0" />
                  <span>Trí nhớ FSRS v5</span>
                </div>
                <p className="text-base font-bold text-slate-900 dark:text-white print:text-slate-900 mt-1">
                  {course.masteredFlashcardCount ?? 0}/{course.flashcardCount ?? 0} thuộc
                </p>
                <p className="text-[11px] text-slate-500 dark:text-slate-400">
                  Cần ôn ngay: {course.dueFlashcardCount ?? 0} thẻ
                </p>
              </div>
            </div>
          </div>

          {/* Metadata Footer Row */}
          <div className="pt-6 mt-6 border-t border-slate-200 dark:border-slate-800 grid grid-cols-2 sm:grid-cols-4 gap-4 text-left max-w-3xl mx-auto">
            <div>
              <p className="text-[11px] uppercase tracking-wider text-slate-400 font-semibold">
                Ngày cấp
              </p>
              <p className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200 print:text-slate-900 mt-0.5">
                {completedDateStr}
              </p>
            </div>

            <div>
              <p className="text-[11px] uppercase tracking-wider text-slate-400 font-semibold">
                Thời lượng đào tạo
              </p>
              <p className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200 print:text-slate-900 mt-0.5">
                {theme.estimatedHoursLabel} ({totalCourseMinutes} phút)
              </p>
            </div>

            <div>
              <p className="text-[11px] uppercase tracking-wider text-slate-400 font-semibold">
                Thời gian học thực tế
              </p>
              <p className="text-xs sm:text-sm font-bold text-slate-800 dark:text-slate-200 print:text-slate-900 mt-0.5">
                {studiedHoursText}
              </p>
            </div>

            <div>
              <p className="text-[11px] uppercase tracking-wider text-slate-400 font-semibold">
                Mã chứng nhận
              </p>
              <p className="text-xs sm:text-sm font-mono font-bold text-amber-700 dark:text-amber-400 mt-0.5 flex items-center gap-1">
                <ShieldCheck className="w-3.5 h-3.5 shrink-0" />
                <span>{certificateCode}</span>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
