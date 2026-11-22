"use client";

import { useState, useEffect, useRef, useCallback } from "react";

export interface UseReadingTelemetryOptions {
  courseId?: string | null;
  lessonId?: string | null;
  readerScrollRef: React.RefObject<HTMLDivElement | null>;
  getHeaders: (extra?: Record<string, string>) => Record<string, string>;
  isStandalone?: boolean;
}

export interface ReadingTelemetryResult {
  scrollPercent: number;
  lastScrollOffset: number;
  timeSpentSeconds: number;
  formattedTimeSpent: string;
  reportTelemetryNow: () => Promise<void>;
}

export function useReadingTelemetry({
  courseId,
  lessonId,
  readerScrollRef,
  getHeaders,
  isStandalone = false,
}: UseReadingTelemetryOptions): ReadingTelemetryResult {
  const [scrollPercent, setScrollPercent] = useState<number>(0);
  const [lastScrollOffset, setLastScrollOffset] = useState<number>(0);
  const [timeSpentSeconds, setTimeSpentSeconds] = useState<number>(0);

  const deltaAccumulatorRef = useRef<number>(0);
  const totalTimeSpentRef = useRef<number>(0);
  const lastSentOffsetRef = useRef<number>(-1);
  const currentScrollRef = useRef<{ offset: number; percent: number }>({ offset: 0, percent: 0 });
  const activeLessonIdRef = useRef<string | null | undefined>(lessonId);

  activeLessonIdRef.current = lessonId;

  // 1. Report telemetry payload to backend
  const sendTelemetry = useCallback(
    async (targetLessonId?: string | null) => {
      const activeId = targetLessonId || activeLessonIdRef.current;
      if (!activeId || isStandalone) return;

      const delta = deltaAccumulatorRef.current;
      const cur = currentScrollRef.current;

      // Don't spam if nothing changed and no time spent
      if (delta <= 0 && cur.offset === lastSentOffsetRef.current) {
        return;
      }

      deltaAccumulatorRef.current = 0;
      lastSentOffsetRef.current = cur.offset;

      try {
        const resp = await fetch(`/api/v1/lessons/${activeId}/telemetry`, {
          method: "POST",
          headers: getHeaders({ "Content-Type": "application/json" }),
          body: JSON.stringify({
            timeSpentDeltaSeconds: delta,
            scrollPercent: cur.percent,
            lastScrollOffset: Math.round(cur.offset),
          }),
        });
        if (resp.ok) {
          const data = await resp.json();
          if (typeof data.timeSpentSeconds === "number") {
            totalTimeSpentRef.current = data.timeSpentSeconds;
            setTimeSpentSeconds(data.timeSpentSeconds);
          }
        }
      } catch {
        // Ignore network telemetry errors gracefully
      }
    },
    [getHeaders, isStandalone]
  );

  // 2. Restore lastScrollOffset from GET /api/v1/courses/${courseId}/resume-state
  useEffect(() => {
    if (!courseId || !lessonId || isStandalone) {
      setScrollPercent(0);
      setLastScrollOffset(0);
      setTimeSpentSeconds(0);
      totalTimeSpentRef.current = 0;
      deltaAccumulatorRef.current = 0;
      currentScrollRef.current = { offset: 0, percent: 0 };
      lastSentOffsetRef.current = -1;
      return;
    }

    let isSubscribed = true;

    async function restoreResumeState() {
      try {
        const res = await fetch(`/api/v1/courses/${courseId}/resume-state`, {
          headers: getHeaders(),
        });
        if (!res.ok || !isSubscribed) return;
        const data = await res.json();

        let targetOffset = 0;
        let targetPercent = 0;
        let targetTime = 0;

        if (Array.isArray(data.lessonProgress)) {
          const match = data.lessonProgress.find((p: any) => p.lessonId === lessonId);
          if (match) {
            targetOffset = Number(match.lastScrollOffset ?? 0);
            targetPercent = Number(match.scrollPercent ?? 0);
            targetTime = Number(match.timeSpentSeconds ?? 0);
          }
        }

        if (targetOffset === 0 && data.lastViewedLessonId === lessonId) {
          targetOffset = Number(data.lastScrollOffset ?? 0);
          targetPercent = Number(data.scrollPercent ?? 0);
        }

        if (!isSubscribed) return;

        totalTimeSpentRef.current = targetTime;
        setTimeSpentSeconds(targetTime);
        setScrollPercent(targetPercent);
        setLastScrollOffset(targetOffset);
        currentScrollRef.current = { offset: targetOffset, percent: targetPercent };

        if (targetOffset > 0 && readerScrollRef.current) {
          // Allow DOM to settle before smooth scrolling
          setTimeout(() => {
            if (isSubscribed && readerScrollRef.current) {
              readerScrollRef.current.scrollTo({
                top: targetOffset,
                behavior: "smooth",
              });
            }
          }, 150);
        }
      } catch {
        // Fallback: start at top
      }
    }

    restoreResumeState();

    return () => {
      isSubscribed = false;
      // Send any pending delta when lesson changes
      sendTelemetry(lessonId);
    };
  }, [courseId, lessonId, isStandalone, getHeaders, readerScrollRef, sendTelemetry]);

  // 3. Track scroll changes in readerScrollRef
  useEffect(() => {
    const el = readerScrollRef.current;
    if (!el) return;

    const handleScroll = () => {
      const scrollTop = el.scrollTop;
      const maxScroll = Math.max(1, el.scrollHeight - el.clientHeight);
      const pct = Math.min(100, Math.max(0, Math.round((scrollTop / maxScroll) * 100)));

      currentScrollRef.current = { offset: scrollTop, percent: pct };
      setLastScrollOffset(scrollTop);
      setScrollPercent(pct);
    };

    el.addEventListener("scroll", handleScroll, { passive: true });
    return () => el.removeEventListener("scroll", handleScroll);
  }, [readerScrollRef, lessonId]);

  // 4. Timer: count real study time when tab is active
  useEffect(() => {
    if (!lessonId) return;

    const secondInterval = setInterval(() => {
      if (typeof document !== "undefined" && document.visibilityState === "visible") {
        deltaAccumulatorRef.current += 1;
        totalTimeSpentRef.current += 1;
        setTimeSpentSeconds(totalTimeSpentRef.current);
      }
    }, 1000);

    // Periodic flush every 15s
    const periodicFlush = setInterval(() => {
      sendTelemetry(lessonId);
    }, 15000);

    return () => {
      clearInterval(secondInterval);
      clearInterval(periodicFlush);
    };
  }, [lessonId, sendTelemetry]);

  // Format time display
  const mins = Math.floor(timeSpentSeconds / 60);
  const secs = timeSpentSeconds % 60;
  const formattedTimeSpent = mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;

  return {
    scrollPercent,
    lastScrollOffset,
    timeSpentSeconds,
    formattedTimeSpent,
    reportTelemetryNow: () => sendTelemetry(lessonId),
  };
}
