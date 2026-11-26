"use client";

import React, { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  AlertCircle,
  AlertTriangle,
  Archive,
  Calendar,
  Check,
  CheckCircle2,
  Copy,
  Download,
  Edit3,
  Pin,
  RefreshCw,
  RotateCcw,
  Save,
  ShieldAlert,
  Sparkles,
  Trash2,
} from "lucide-react";
import { Button, Card, Badge, Dialog } from "@/components/ui";
import { useWorkspaceStore, getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import type { CourseDetail } from "./types";

export interface CourseSettingsTabProps {
  course: CourseDetail;
  courseId: string;
  onCourseUpdated: (updated: Partial<CourseDetail>) => void;
  onProgressReset: (resetCount: number) => void;
}

export function CourseSettingsTab({
  course,
  courseId,
  onCourseUpdated,
  onProgressReset,
}: CourseSettingsTabProps) {
  const router = useRouter();
  const { activeUserHeaderId } = useWorkspaceStore();

  // Daily goal & Target date
  const [dailyGoal, setDailyGoal] = useState<number>(course.dailyLessonGoal ?? 2);
  const initialDateStr = course.targetCompletionDate
    ? new Date(course.targetCompletionDate).toISOString().split("T")[0]
    : "";
  const [targetDate, setTargetDate] = useState<string>(initialDateStr);
  const [isSavingPreferences, setIsSavingPreferences] = useState(false);
  const [preferencesNotice, setPreferencesNotice] = useState<string | null>(null);

  // Sync local state when course prop changes
  useEffect(() => {
    setDailyGoal(course.dailyLessonGoal ?? 2);
    setTargetDate(
      course.targetCompletionDate
        ? new Date(course.targetCompletionDate).toISOString().split("T")[0]
        : ""
    );
  }, [course.dailyLessonGoal, course.targetCompletionDate]);

  const remainingLessons = Math.max(0, course.totalLessons - course.completedLessons);

  const forecast = useMemo(() => {
    if (remainingLessons === 0) return null;
    const safeGoal = Math.max(1, dailyGoal);
    const daysNeeded = Math.ceil(remainingLessons / safeGoal);
    const finishDate = new Date();
    finishDate.setDate(finishDate.getDate() + daysNeeded);
    const yyyy = finishDate.getFullYear();
    const mm = String(finishDate.getMonth() + 1).padStart(2, "0");
    const dd = String(finishDate.getDate()).padStart(2, "0");
    return {
      daysNeeded,
      isoDate: `${yyyy}-${mm}-${dd}`,
      displayDate: `${dd}/${mm}/${yyyy}`,
    };
  }, [remainingLessons, dailyGoal]);

  // Pin & Archive toggling
  const [isTogglingPin, setIsTogglingPin] = useState(false);
  const [isTogglingArchive, setIsTogglingArchive] = useState(false);

  // Cloning
  const [isCloning, setIsCloning] = useState(false);
  const [cloneError, setCloneError] = useState<string | null>(null);

  // Export Zip
  const [isExportingZip, setIsExportingZip] = useState(false);
  const [exportError, setExportError] = useState<string | null>(null);

  // Reset Progress Modal
  const [showResetModal, setShowResetModal] = useState(false);
  const [resetConfirmStep, setResetConfirmStep] = useState<1 | 2>(1);
  const [isResetting, setIsResetting] = useState(false);
  const [resetNotice, setResetNotice] = useState<string | null>(null);

  // Delete Course Modal
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteConfirmStep, setDeleteConfirmStep] = useState<1 | 2>(1);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const getEffectiveHeaders = (extra?: Record<string, string>) => {
    const effectiveUserId =
      activeUserHeaderId ??
      (typeof window !== "undefined" ? window.localStorage.getItem("swms_active_user_id") : null);
    return getClientRequestHeaders({
      ...(effectiveUserId ? { "X-Test-User-Id": effectiveUserId } : {}),
      ...(extra ?? {}),
    });
  };

  // 1. Save Study Pace Preferences (DB-persisted)
  const handleSavePreferences = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSavingPreferences) return;

    setIsSavingPreferences(true);
    setPreferencesNotice(null);
    try {
      const headers = getEffectiveHeaders({ "Content-Type": "application/json" });
      const payload: {
        dailyLessonGoal: number;
        targetCompletionDate?: string | null;
        clearTargetDate?: boolean;
      } = {
        dailyLessonGoal: Math.max(1, Math.min(20, dailyGoal)),
      };

      if (targetDate) {
        payload.targetCompletionDate = new Date(targetDate).toISOString();
        payload.clearTargetDate = false;
      } else {
        payload.clearTargetDate = true;
      }

      const resp = await fetch(`/api/v1/courses/${courseId}/preferences`, {
        method: "PUT",
        headers,
        credentials: "include",
        body: JSON.stringify(payload),
      });

      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setPreferencesNotice(err?.error || `Không thể lưu tùy chọn (HTTP ${resp.status})`);
        return;
      }

      const data = await resp.json();
      onCourseUpdated({
        dailyLessonGoal: data.dailyLessonGoal,
        targetCompletionDate: data.targetCompletionDate,
      });
      invalidateDashboardCache();
      setPreferencesNotice("Đã lưu mục tiêu học tập vào cơ sở dữ liệu thành công!");
    } catch {
      setPreferencesNotice("Lỗi kết nối khi lưu mục tiêu học tập.");
    } finally {
      setIsSavingPreferences(false);
    }
  };

  // 2. Toggle Pin
  const handleTogglePin = async () => {
    if (isTogglingPin) return;
    setIsTogglingPin(true);
    try {
      const nextPin = !course.isPinned;
      const headers = getEffectiveHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${courseId}/preferences`, {
        method: "PUT",
        headers,
        credentials: "include",
        body: JSON.stringify({ isPinned: nextPin }),
      });
      if (resp.ok) {
        onCourseUpdated({ isPinned: nextPin });
        invalidateDashboardCache();
      }
    } finally {
      setIsTogglingPin(false);
    }
  };

  // 3. Toggle Archive
  const handleToggleArchive = async () => {
    if (isTogglingArchive) return;
    setIsTogglingArchive(true);
    try {
      const nextArchive = !course.isArchived;
      const headers = getEffectiveHeaders({ "Content-Type": "application/json" });
      const resp = await fetch(`/api/v1/courses/${courseId}/preferences`, {
        method: "PUT",
        headers,
        credentials: "include",
        body: JSON.stringify({ isArchived: nextArchive }),
      });
      if (resp.ok) {
        onCourseUpdated({ isArchived: nextArchive });
        invalidateDashboardCache();
      }
    } finally {
      setIsTogglingArchive(false);
    }
  };

  // 4. Clone Course
  const handleCloneCourse = async () => {
    if (isCloning) return;
    setIsCloning(true);
    setCloneError(null);
    try {
      const headers = getEffectiveHeaders();
      const resp = await fetch(`/api/v1/courses/${courseId}/clone`, {
        method: "POST",
        headers,
        credentials: "include",
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setCloneError(err?.error || `Không thể nhân bản khóa học (HTTP ${resp.status})`);
        return;
      }
      const clonedCourse = await resp.json();
      invalidateDashboardCache();
      const targetId = clonedCourse.id || clonedCourse.courseId;
      router.push(`/app/courses/${targetId}`);
    } catch {
      setCloneError("Lỗi kết nối khi nhân bản khóa học.");
    } finally {
      setIsCloning(false);
    }
  };

  // 5. Export ZIP
  const handleExportZip = async () => {
    if (isExportingZip) return;
    setIsExportingZip(true);
    setExportError(null);
    try {
      const headers = getEffectiveHeaders();
      const resp = await fetch(`/api/v1/courses/${courseId}/export-zip`, {
        headers,
        credentials: "include",
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setExportError(err?.error || `Không xuất được gói dữ liệu (HTTP ${resp.status})`);
        return;
      }
      const blob = await resp.blob();
      const fileName = `${course.slug || "course"}.swm.zip`;
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      setExportError("Lỗi kết nối khi tải xuống gói khóa học.");
    } finally {
      setIsExportingZip(false);
    }
  };

  // 6. Reset Progress
  const handleExecuteResetProgress = async () => {
    if (isResetting) return;
    setIsResetting(true);
    setResetNotice(null);
    try {
      const headers = getEffectiveHeaders();
      const resp = await fetch(`/api/v1/courses/${courseId}/reset-progress`, {
        method: "POST",
        headers,
        credentials: "include",
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setResetNotice(err?.error || `Không thể đặt lại tiến độ (HTTP ${resp.status})`);
        return;
      }
      const data = await resp.json();
      const resetCount = Number(data.resetLessonsCount ?? 0);
      invalidateDashboardCache();
      onProgressReset(resetCount);
      setShowResetModal(false);
      setResetConfirmStep(1);
    } catch {
      setResetNotice("Lỗi kết nối khi đặt lại tiến độ.");
    } finally {
      setIsResetting(false);
    }
  };

  // 7. Delete Personal Course
  const handleExecuteDeleteCourse = async () => {
    if (isDeleting) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      const headers = getEffectiveHeaders();
      const resp = await fetch(`/api/v1/courses/${courseId}`, {
        method: "DELETE",
        headers,
        credentials: "include",
      });
      if (!resp.ok) {
        const err = await resp.json().catch(() => null);
        setDeleteError(err?.error || `Không thể xóa khóa học (HTTP ${resp.status})`);
        return;
      }
      invalidateDashboardCache();
      setShowDeleteModal(false);
      router.push("/app/courses");
    } catch {
      setDeleteError("Lỗi kết nối khi xóa khóa học.");
    } finally {
      setIsDeleting(false);
    }
  };

  const isSystemCourse = Boolean(course.isSystemCourse);
  const canDelete = Boolean(course.canDelete && !isSystemCourse);

  return (
    <div className="space-y-6">
      {/* 1. Lịch học & Mục tiêu hàng ngày (DB-persisted) */}
      <Card className="p-5 sm:p-6 bg-[var(--surface-card)] space-y-4">
        <div className="border-b border-[var(--border-calm)] pb-3">
          <h3 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
            <Calendar className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC]" />
            <span>Lịch học &amp; Mục tiêu hàng ngày</span>
          </h3>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            Các thiết lập này được lưu và đồng bộ trên mọi thiết bị của bạn.
          </p>
        </div>

        <form onSubmit={handleSavePreferences} className="space-y-4 max-w-xl">
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-[var(--text-primary)] flex items-center justify-between">
              <span>Số bài học mục tiêu mỗi ngày:</span>
              <span className="text-[#5E51E8] dark:text-[#A5B4FC] font-bold">
                {dailyGoal} bài/ngày
              </span>
            </label>
            <div className="flex items-center gap-3">
              <input
                type="range"
                min={1}
                max={20}
                step={1}
                value={dailyGoal}
                onChange={(e) => setDailyGoal(Number(e.target.value))}
                className="flex-1 accent-[#5E51E8] cursor-pointer"
              />
              <input
                type="number"
                min={1}
                max={20}
                value={dailyGoal}
                onChange={(e) => setDailyGoal(Math.max(1, Math.min(20, Number(e.target.value))))}
                className="w-16 h-8 px-2 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-center text-xs font-bold text-[var(--text-primary)]"
              />
            </div>
            {/* 4 Preset Chips */}
            <div className="flex flex-wrap items-center gap-1.5 pt-1">
              {[
                { value: 1, label: "1 bài/ngày (Nhẹ nhàng)" },
                { value: 2, label: "2 bài/ngày (Chuẩn)" },
                { value: 3, label: "3 bài/ngày (Tăng tốc)" },
                { value: 5, label: "5 bài/ngày (Nước rút)" },
              ].map((preset) => {
                const isSelected = dailyGoal === preset.value;
                return (
                  <button
                    key={preset.value}
                    type="button"
                    onClick={() => setDailyGoal(preset.value)}
                    className={`h-7 px-2.5 rounded-lg border text-[11px] font-semibold transition cursor-pointer ${
                      isSelected
                        ? "border-[#5E51E8] bg-[#EEF0FD] text-[#4F46E5] dark:bg-[#1E1B4B] dark:border-[#818CF8] dark:text-[#A5B4FC]"
                        : "border-[var(--border-calm)] bg-[var(--surface-elevated)] text-[var(--text-secondary)] hover:text-[var(--text-primary)] hover:border-[#5E51E8]/40"
                    }`}
                  >
                    {preset.label}
                  </button>
                );
              })}
            </div>
            <p className="text-[11px] text-[var(--text-tertiary)]">
              Khuyến nghị: 2–4 bài/ngày để duy trì nhịp học bền vững mà không bị quá tải.
            </p>
          </div>

          {/* Dynamic Forecast Card */}
          <div className="p-3.5 rounded-xl border border-indigo-500/20 bg-indigo-500/5 dark:bg-indigo-950/25 space-y-2">
            {remainingLessons === 0 ? (
              <div className="flex items-center gap-2 text-xs font-semibold text-emerald-700 dark:text-emerald-300">
                <CheckCircle2 className="w-4 h-4 shrink-0" />
                <span>Chúc mừng! Bạn đã hoàn tất toàn bộ {course.totalLessons} bài học của khóa này.</span>
              </div>
            ) : forecast ? (
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2.5">
                <div className="flex items-start gap-2 text-xs text-[var(--text-primary)]">
                  <Sparkles className="w-4 h-4 text-[#5E51E8] dark:text-[#A5B4FC] shrink-0 mt-0.5" />
                  <p className="leading-relaxed">
                    Với nhịp <strong className="text-[#5E51E8] dark:text-[#A5B4FC]">{dailyGoal} bài/ngày</strong> cho{" "}
                    <strong>{remainingLessons}</strong> bài còn lại → Dự kiến hoàn thành sau{" "}
                    <strong>~{forecast.daysNeeded} ngày</strong> ({forecast.displayDate})
                  </p>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setTargetDate(forecast.isoDate)}
                  className="shrink-0 text-xs whitespace-nowrap"
                >
                  Áp dụng ngày dự kiến
                </Button>
              </div>
            ) : null}
          </div>

          <div className="space-y-1.5 pt-2 border-t border-[var(--border-calm)]/40">
            <label className="text-xs font-semibold text-[var(--text-primary)] block">
              Ngày đích dự kiến hoàn thành khóa học:
            </label>
            <div className="flex items-center gap-2">
              <input
                type="date"
                value={targetDate}
                onChange={(e) => setTargetDate(e.target.value)}
                className="h-9 px-3 rounded-lg border border-[var(--border-calm)] bg-[var(--surface-elevated)] text-xs text-[var(--text-primary)]"
              />
              {targetDate && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setTargetDate("")}
                  className="text-xs"
                >
                  Xóa ngày đích
                </Button>
              )}
            </div>
            <p className="text-[11px] text-[var(--text-tertiary)]">
              Để trống nếu muốn tự do hoàn thành mà không đặt thời hạn chót.
            </p>
          </div>

          <div className="pt-2">
            <Button
              type="submit"
              variant="primary"
              size="sm"
              disabled={isSavingPreferences}
              leftIcon={<Save className="w-3.5 h-3.5" />}
            >
              <span>{isSavingPreferences ? "Đang lưu..." : "Lưu lịch học"}</span>
            </Button>
          </div>

          {preferencesNotice && (
            <p className="text-xs text-[#4F46E5] dark:text-[#A5B4FC] font-medium">
              {preferencesNotice}
            </p>
          )}
        </form>
      </Card>

      {/* 2. Ghim & Lưu trữ */}
      <Card className="p-5 sm:p-6 bg-[var(--surface-card)] space-y-4">
        <div className="border-b border-[var(--border-calm)] pb-3">
          <h3 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
            <Pin className="w-4 h-4 text-indigo-500" />
            <span>Hiển thị trong không gian học</span>
          </h3>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            Ghim để ưu tiên trên đầu trang chủ hoặc lưu trữ để ẩn các khóa tạm dừng học.
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] flex items-center justify-between gap-3">
            <div className="space-y-0.5">
              <p className="text-xs font-bold text-[var(--text-primary)]">
                {course.isPinned ? "Đang ghim lên đầu" : "Ghim khóa học"}
              </p>
              <p className="text-[11px] text-[var(--text-tertiary)]">
                Ưu tiên xuất hiện đầu tiên trong danh sách khóa học và trang tổng quan
              </p>
            </div>
            <Button
              type="button"
              variant={course.isPinned ? "primary" : "outline"}
              size="sm"
              disabled={isTogglingPin}
              onClick={handleTogglePin}
              leftIcon={<Pin className="w-3.5 h-3.5" />}
            >
              <span>{course.isPinned ? "Bỏ ghim" : "Ghim"}</span>
            </Button>
          </div>

          <div className="p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] flex items-center justify-between gap-3">
            <div className="space-y-0.5">
              <p className="text-xs font-bold text-[var(--text-primary)]">
                {course.isArchived ? "Đang lưu trữ" : "Lưu trữ khóa học"}
              </p>
              <p className="text-[11px] text-[var(--text-tertiary)]">
                Ẩn khỏi danh sách đang học chính mà không làm mất tiến độ
              </p>
            </div>
            <Button
              type="button"
              variant={course.isArchived ? "secondary" : "outline"}
              size="sm"
              disabled={isTogglingArchive}
              onClick={handleToggleArchive}
              leftIcon={<Archive className="w-3.5 h-3.5" />}
            >
              <span>{course.isArchived ? "Bỏ lưu trữ" : "Lưu trữ"}</span>
            </Button>
          </div>
        </div>
      </Card>

      {/* 3. Quản lý & Xuất dữ liệu */}
      <Card className="p-5 sm:p-6 bg-[var(--surface-card)] space-y-4">
        <div className="border-b border-[var(--border-calm)] pb-3">
          <h3 className="text-base font-bold text-[var(--text-primary)] flex items-center gap-2">
            <Copy className="w-4 h-4 text-emerald-500" />
            <span>Quản lý &amp; Xuất dữ liệu</span>
          </h3>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            Nhân bản khóa học cá nhân, tải gói dữ liệu offline hoặc đặt lại tiến độ học.
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {/* Clone */}
          <div className="p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] flex flex-col justify-between gap-3">
            <div className="space-y-1">
              <p className="text-xs font-bold text-[var(--text-primary)]">
                Nhân bản khóa học
              </p>
              <p className="text-[11px] text-[var(--text-tertiary)]">
                Tạo một bản sao cá nhân độc lập để tùy biến giáo trình và ghi chú riêng.
              </p>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={isCloning}
              onClick={handleCloneCourse}
              leftIcon={<Copy className="w-3.5 h-3.5 text-indigo-500" />}
            >
              <span>{isCloning ? "Đang nhân bản..." : "Tạo bản sao cá nhân"}</span>
            </Button>
          </div>

          {/* Export ZIP */}
          <div className="p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] flex flex-col justify-between gap-3">
            <div className="space-y-1">
              <p className="text-xs font-bold text-[var(--text-primary)]">
                Xuất gói dữ liệu (.swm.zip)
              </p>
              <p className="text-[11px] text-[var(--text-tertiary)]">
                Tải xuống toàn bộ tài liệu Markdown, mục lục và câu hỏi để lưu trữ ngoại tuyến.
              </p>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={isExportingZip}
              onClick={handleExportZip}
              leftIcon={<Download className="w-3.5 h-3.5 text-emerald-500" />}
            >
              <span>{isExportingZip ? "Đang tải xuống..." : "Tải gói .swm.zip"}</span>
            </Button>
          </div>

          {/* Reset Progress */}
          <div className="p-4 rounded-xl border border-[var(--border-calm)] bg-[var(--surface-elevated)] flex flex-col justify-between gap-3">
            <div className="space-y-1">
              <p className="text-xs font-bold text-[var(--text-primary)]">
                Đặt lại tiến độ học
              </p>
              <p className="text-[11px] text-[var(--text-tertiary)]">
                Xóa toàn bộ đánh dấu bài đã hoàn thành để bắt đầu học lại từ đầu.
              </p>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => {
                setResetConfirmStep(1);
                setShowResetModal(true);
              }}
              leftIcon={<RotateCcw className="w-3.5 h-3.5 text-amber-500" />}
            >
              <span>Đặt lại tiến độ</span>
            </Button>
          </div>
        </div>

        {cloneError && (
          <p className="text-xs text-red-500 font-medium">{cloneError}</p>
        )}
        {exportError && (
          <p className="text-xs text-red-500 font-medium">{exportError}</p>
        )}
      </Card>

      {/* 4. Khu vực nguy hiểm (Danger Zone) */}
      <Card
        className={`p-5 sm:p-6 bg-[var(--surface-card)] space-y-4 ${
          isSystemCourse ? "border-indigo-500/30" : "border-rose-500/30"
        }`}
      >
        <div className="border-b border-[var(--border-calm)] pb-3">
          <h3
            className={`text-base font-bold flex items-center gap-2 ${
              isSystemCourse
                ? "text-[var(--text-primary)]"
                : "text-rose-600 dark:text-rose-400"
            }`}
          >
            {isSystemCourse ? (
              <ShieldAlert className="w-4 h-4 text-indigo-500" />
            ) : (
              <AlertTriangle className="w-4 h-4 text-rose-500" />
            )}
            <span>
              {isSystemCourse ? "Bảo vệ giáo trình hệ thống" : "Khu vực nguy hiểm"}
            </span>
          </h3>
          <p className="text-xs text-[var(--text-secondary)] mt-0.5">
            {isSystemCourse
              ? "Khóa học này là tài nguyên cốt lõi thuộc thư viện chuẩn của Studxy."
              : "Các thao tác chỉnh sửa sâu hoặc xóa vĩnh viễn khóa học cá nhân."}
          </p>
        </div>

        {isSystemCourse ? (
          <div className="p-4 rounded-xl border border-indigo-500/20 bg-indigo-500/5 space-y-2">
            <p className="text-xs font-semibold text-[var(--text-primary)]">
              Đây là giáo trình chuẩn hệ thống của Studxy
            </p>
            <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
              Giáo trình hệ thống được bảo vệ để đảm bảo tính nhất quán dữ liệu cho tất cả học viên và không thể xóa. Nếu không muốn hiển thị khóa học này trên màn hình chính, bạn có thể bật tính năng <strong>Lưu trữ</strong> ở phía trên, hoặc dùng <strong>Nhân bản khóa học</strong> để tùy chỉnh nội dung riêng.
            </p>
          </div>
        ) : (
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-4 rounded-xl border border-rose-500/20 bg-rose-500/5">
            <div className="space-y-1">
              <p className="text-xs font-bold text-rose-600 dark:text-rose-400">
                Xóa vĩnh viễn khóa học này
              </p>
              <p className="text-xs text-[var(--text-secondary)]">
                Sau khi xóa, khóa học và mọi chương bài giảng, ghi chú liên quan sẽ bị xóa vĩnh viễn. Không thể khôi phục.
              </p>
            </div>
            <div className="flex items-center gap-2 shrink-0">
              <Link href={`/app/courses/${courseId}/edit`}>
                <Button
                  variant="outline"
                  size="sm"
                  leftIcon={<Edit3 className="w-3.5 h-3.5" />}
                >
                  <span>Chỉnh sửa khóa</span>
                </Button>
              </Link>
              {canDelete && (
                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => {
                    setDeleteConfirmStep(1);
                    setShowDeleteModal(true);
                  }}
                  leftIcon={<Trash2 className="w-3.5 h-3.5" />}
                >
                  <span>Xóa khóa học</span>
                </Button>
              )}
            </div>
          </div>
        )}
      </Card>

      {/* Reset Progress 2-Step Confirmation Modal */}
      <Dialog
        isOpen={showResetModal}
        onClose={() => {
          if (!isResetting) setShowResetModal(false);
        }}
        title="Đặt lại tiến độ học tập"
        maxWidth="md"
      >
        <div className="space-y-4">
          <p className="text-xs text-[var(--text-secondary)] leading-relaxed">
            {resetConfirmStep === 1
              ? "Bạn có chắc chắn muốn đặt lại toàn bộ tiến độ của khóa học này không? Tất cả các bài đã đánh dấu hoàn thành sẽ trở về trạng thái chưa học."
              : "Xác nhận lần 2: Thao tác này sẽ xóa toàn bộ lịch sử hoàn thành bài học của bạn trong khóa học này. Bạn có muốn tiếp tục?"}
          </p>

          {resetNotice && (
            <p className="text-xs text-red-500 font-medium">{resetNotice}</p>
          )}

          <div className="flex items-center justify-end gap-2 pt-2 border-t border-[var(--border-calm)]">
            <Button
              variant="outline"
              size="sm"
              disabled={isResetting}
              onClick={() => setShowResetModal(false)}
            >
              Hủy
            </Button>
            {resetConfirmStep === 1 ? (
              <Button
                variant="primary"
                size="sm"
                onClick={() => setResetConfirmStep(2)}
              >
                Tiếp tục bước 2
              </Button>
            ) : (
              <Button
                variant="danger"
                size="sm"
                disabled={isResetting}
                onClick={handleExecuteResetProgress}
              >
                {isResetting ? "Đang đặt lại..." : "Xác nhận đặt lại"}
              </Button>
            )}
          </div>
        </div>
      </Dialog>

      {/* Delete Course 2-Step Confirmation Modal */}
      <Dialog
        isOpen={showDeleteModal}
        onClose={() => {
          if (!isDeleting) setShowDeleteModal(false);
        }}
        title="Xóa khóa học cá nhân"
        maxWidth="md"
      >
        <div className="space-y-4">
          <div className="p-3 rounded-lg border border-red-500/30 bg-red-500/10 text-xs text-red-600 dark:text-red-400 space-y-1">
            <p className="font-bold flex items-center gap-1.5">
              <AlertTriangle className="w-4 h-4 shrink-0" />
              <span>Hành động không thể hoàn tác</span>
            </p>
            <p>
              {deleteConfirmStep === 1
                ? `Bạn đang chuẩn bị xóa khóa học "${course.title}". Toàn bộ nội dung chương, bài giảng và dữ liệu liên quan sẽ bị xóa hoàn toàn.`
                : `Xác nhận lần cuối: Bạn có chắc chắn muốn xóa vĩnh viễn khóa học "${course.title}" không?`}
            </p>
          </div>

          {deleteError && (
            <p className="text-xs text-red-500 font-medium">{deleteError}</p>
          )}

          <div className="flex items-center justify-end gap-2 pt-2 border-t border-[var(--border-calm)]">
            <Button
              variant="outline"
              size="sm"
              disabled={isDeleting}
              onClick={() => setShowDeleteModal(false)}
            >
              Hủy
            </Button>
            {deleteConfirmStep === 1 ? (
              <Button
                variant="danger"
                size="sm"
                onClick={() => setDeleteConfirmStep(2)}
              >
                Tôi hiểu rủi ro, tiếp tục
              </Button>
            ) : (
              <Button
                variant="danger"
                size="sm"
                disabled={isDeleting}
                onClick={handleExecuteDeleteCourse}
              >
                {isDeleting ? "Đang xóa..." : "Xóa vĩnh viễn khóa học"}
              </Button>
            )}
          </div>
        </div>
      </Dialog>
    </div>
  );
}
