"use client";

import React, { useState, useEffect, useCallback } from "react";

export type PomodoroPhase = "focus" | "break";

export interface PomodoroTimerState {
  pomoRunning: boolean;
  pomoPhase: PomodoroPhase;
  pomoSecondsLeft: number;
  formattedPomoTime: string;
  togglePomo: () => void;
  resetPomo: () => void;
  setPomoRunning: React.Dispatch<React.SetStateAction<boolean>>;
  setPomoPhase: React.Dispatch<React.SetStateAction<PomodoroPhase>>;
  setPomoSecondsLeft: React.Dispatch<React.SetStateAction<number>>;
}

export function usePomodoroTimer(
  focusMinutes: number = 25,
  breakMinutes: number = 5
): PomodoroTimerState {
  const [pomoSecondsLeft, setPomoSecondsLeft] = useState<number>(focusMinutes * 60);
  const [pomoRunning, setPomoRunning] = useState<boolean>(false);
  const [pomoPhase, setPomoPhase] = useState<PomodoroPhase>("focus");

  useEffect(() => {
    if (!pomoRunning) return;
    const timer = setInterval(() => {
      setPomoSecondsLeft((prev) => {
        if (prev <= 1) {
          const nextPhase: PomodoroPhase = pomoPhase === "focus" ? "break" : "focus";
          setPomoPhase(nextPhase);
          setPomoRunning(false);
          return nextPhase === "focus" ? focusMinutes * 60 : breakMinutes * 60;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [pomoRunning, pomoPhase, focusMinutes, breakMinutes]);

  const togglePomo = useCallback(() => {
    setPomoRunning((prev) => !prev);
  }, []);

  const resetPomo = useCallback(() => {
    setPomoRunning(false);
    setPomoPhase("focus");
    setPomoSecondsLeft(focusMinutes * 60);
  }, [focusMinutes]);

  const formattedPomoTime = `${String(Math.floor(pomoSecondsLeft / 60)).padStart(2, "0")}:${String(
    pomoSecondsLeft % 60
  ).padStart(2, "0")}`;

  return {
    pomoRunning,
    pomoPhase,
    pomoSecondsLeft,
    formattedPomoTime,
    togglePomo,
    resetPomo,
    setPomoRunning,
    setPomoPhase,
    setPomoSecondsLeft,
  };
}
