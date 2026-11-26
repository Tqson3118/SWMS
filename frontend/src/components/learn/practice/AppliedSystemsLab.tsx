"use client";

import React, { useCallback, useEffect, useMemo, useState } from "react";
import {
  AlertTriangle,
  BookmarkPlus,
  CheckCircle2,
  Code2,
  Database,
  Eye,
  EyeOff,
  Loader2,
  NotebookPen,
  Send,
  ShieldAlert,
  ShieldCheck,
  Sparkles,
  Terminal,
  XCircle,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import {
  APPLIED_TAB_LABELS_VI,
  resolvePracticeSelection,
  type AppliedTabMode,
} from "@/lib/practice-helpers";
import {
  evaluatePracticeWithAiOrOffline,
  safeParseConfigJson,
  type AiPracticeEvaluationResult,
  type RawDbPracticeActivity,
} from "@/lib/writein-practice";
import { saveWriteInFlashcardDeduplicated } from "./WriteInFsrsHelper";

export interface AppliedSystemsLabProps {
  courseId: string;
  courseTitle: string;
  lessonId?: string | null;
  lessonTitle: string;
  lessonMarkdown: string;
  onSaveToNotebook: (textToAppend: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  initialActivityType?: string | null;
  initialActivityKey?: string | null;
}

interface PromptCritiqueItem {
  id: string;
  difficulty?: string;
  title: string;
  contextVi: string;
  badPrompt: string;
  flawOptions: string[];
  correctFlawIndex: number;
  explanationVi: string;
  rewriteTaskVi: string;
  expectedKeywords: string[];
  sampleImprovedPrompt: string;
  sourceActivityId?: string;
  passingScore?: number;
}

interface SqlReasoningItem {
  id: string;
  difficulty?: string;
  title: string;
  schemaSql: string;
  sampleDataNoteVi: string;
  businessQuestionVi: string;
  buggyOrDraftSql: string;
  diagnosisOptions: string[];
  correctDiagnosisIndex: number;
  explanationVi: string;
  expectedKeywords: string[];
  sampleSolutionSql: string;
  sourceActivityId?: string;
  passingScore?: number;
}

interface ThreatSpotterItem {
  id: string;
  difficulty?: string;
  title: string;
  channel: string;
  artifactTitle: string;
  artifactBody: string;
  redFlags: string[];
  verdictOptions: string[];
  correctVerdictIndex: number;
  explanationVi: string;
  responseTaskVi: string;
  expectedKeywords: string[];
  sampleSafeAction: string;
  sourceActivityId?: string;
  passingScore?: number;
}

interface AppliedActivityBank {
  promptCritique: PromptCritiqueItem[];
  sqlReasoning: SqlReasoningItem[];
  threatSpotter: ThreatSpotterItem[];
}

interface AppliedItemState {
  selectedOptionIdx: number | null;
  diagnosisChecked: boolean;
  responseText: string;
  isEvaluating: boolean;
  evalResult: AiPracticeEvaluationResult | null;
  showSample: boolean;
  isSavingFsrs: boolean;
  fsrsSaved: boolean;
}

const DEFAULT_ITEM_STATE: AppliedItemState = {
  selectedOptionIdx: null,
  diagnosisChecked: false,
  responseText: "",
  isEvaluating: false,
  evalResult: null,
  showSample: false,
  isSavingFsrs: false,
  fsrsSaved: false,
};

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function clampIndex(idx: unknown, maxLen: number): number {
  const num = Number(idx);
  if (!Number.isFinite(num) || maxLen <= 0) return 0;
  return Math.max(0, Math.min(maxLen - 1, Math.trunc(num)));
}

function toStringArray(val: unknown): string[] {
  if (!Array.isArray(val)) return [];
  return val.map((v) => String(v ?? "").trim()).filter(Boolean);
}

function normalizeAppliedActivities(
  activities: RawDbPracticeActivity[]
): AppliedActivityBank {
  const bank: AppliedActivityBank = {
    promptCritique: [],
    sqlReasoning: [],
    threatSpotter: [],
  };

  for (const act of activities || []) {
    const rawType = (act.type || "").trim().toLowerCase();
    const cfg = safeParseConfigJson(act.configJson);
    if (!cfg) continue;

    const rawItems: Record<string, any>[] =
      Array.isArray(cfg.items) && cfg.items.length > 0
        ? cfg.items.filter((x: unknown): x is Record<string, any> => Boolean(x && typeof x === "object"))
        : [cfg];

    if (rawType === "prompt_critique" || rawType === "prompt-critique") {
      rawItems.forEach((raw, idx) => {
        const flawOptions = toStringArray(raw.flawOptions ?? raw.options);
        if (!raw.badPrompt && flawOptions.length === 0) return;
        bank.promptCritique.push({
          id: String(raw.id || `${act.id || "prompt"}-${idx}`),
          difficulty: raw.difficulty ? String(raw.difficulty) : undefined,
          title: String(raw.title || act.title || `Phản biện Prompt #${idx + 1}`),
          contextVi: String(raw.contextVi ?? raw.context ?? act.description ?? ""),
          badPrompt: String(raw.badPrompt ?? raw.prompt ?? ""),
          flawOptions,
          correctFlawIndex: clampIndex(raw.correctFlawIndex, flawOptions.length),
          explanationVi: String(raw.explanationVi ?? raw.explanation ?? ""),
          rewriteTaskVi: String(
            raw.rewriteTaskVi ??
              "Hãy viết lại Prompt hoàn chỉnh (nêu rõ vai trò, bối cảnh, ràng buộc, định dạng đầu ra và tiêu chí kiểm chứng)."
          ),
          expectedKeywords: toStringArray(raw.expectedKeywords),
          sampleImprovedPrompt: String(raw.sampleImprovedPrompt ?? raw.sampleAnswer ?? ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (rawType === "sql_reasoning" || rawType === "sql-reasoning") {
      rawItems.forEach((raw, idx) => {
        const diagnosisOptions = toStringArray(raw.diagnosisOptions ?? raw.options);
        if (!raw.businessQuestionVi && !raw.buggyOrDraftSql && diagnosisOptions.length === 0) return;
        bank.sqlReasoning.push({
          id: String(raw.id || `${act.id || "sql"}-${idx}`),
          difficulty: raw.difficulty ? String(raw.difficulty) : undefined,
          title: String(raw.title || act.title || `Chẩn đoán SQL #${idx + 1}`),
          schemaSql: String(raw.schemaSql ?? raw.schema ?? ""),
          sampleDataNoteVi: String(raw.sampleDataNoteVi ?? raw.sampleDataNote ?? ""),
          businessQuestionVi: String(raw.businessQuestionVi ?? raw.question ?? act.description ?? ""),
          buggyOrDraftSql: String(raw.buggyOrDraftSql ?? raw.buggySql ?? ""),
          diagnosisOptions,
          correctDiagnosisIndex: clampIndex(raw.correctDiagnosisIndex, diagnosisOptions.length),
          explanationVi: String(raw.explanationVi ?? raw.explanation ?? ""),
          expectedKeywords: toStringArray(raw.expectedKeywords),
          sampleSolutionSql: String(raw.sampleSolutionSql ?? raw.sampleAnswer ?? ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    } else if (rawType === "threat_spotter" || rawType === "threat-spotter") {
      rawItems.forEach((raw, idx) => {
        const verdictOptions = toStringArray(raw.verdictOptions ?? raw.options);
        if (!raw.artifactBody && verdictOptions.length === 0) return;
        bank.threatSpotter.push({
          id: String(raw.id || `${act.id || "threat"}-${idx}`),
          difficulty: raw.difficulty ? String(raw.difficulty) : undefined,
          title: String(raw.title || act.title || `Nhận diện mối nguy #${idx + 1}`),
          channel: String(raw.channel ?? "Cảnh báo số"),
          artifactTitle: String(raw.artifactTitle ?? raw.title ?? "Tình huống đáng ngờ"),
          artifactBody: String(raw.artifactBody ?? ""),
          redFlags: toStringArray(raw.redFlags),
          verdictOptions,
          correctVerdictIndex: clampIndex(raw.correctVerdictIndex, verdictOptions.length),
          explanationVi: String(raw.explanationVi ?? raw.explanation ?? ""),
          responseTaskVi: String(
            raw.responseTaskVi ??
              "Hãy đề xuất các bước xử lý an toàn cụ thể (ngừng tương tác, xác minh kênh chính thống, bảo vệ tài khoản/thiết bị)."
          ),
          expectedKeywords: toStringArray(raw.expectedKeywords),
          sampleSafeAction: String(raw.sampleSafeAction ?? raw.sampleAnswer ?? ""),
          sourceActivityId: act.id,
          passingScore: act.passingScore ?? 70,
        });
      });
    }
  }

  return bank;
}

export function AppliedSystemsLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  initialActivityType,
  initialActivityKey,
}: AppliedSystemsLabProps) {
  const resolvedTarget = useMemo(
    () => resolvePracticeSelection(initialActivityType, initialActivityKey, courseTitle, lessonTitle),
    [initialActivityType, initialActivityKey, courseTitle, lessonTitle]
  );

  const [dbActivities, setDbActivities] = useState<Array<RawDbPracticeActivity & { lessonId?: string | null }>>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [toast, setToast] = useState<string | null>(null);
  const [itemStates, setItemStates] = useState<Record<string, AppliedItemState>>({});
  const [selectedIndexByTab, setSelectedIndexByTab] = useState<Record<AppliedTabMode, number>>({
    prompt_critique: 0,
    sql_reasoning: 0,
    threat_spotter: 0,
  });

  const showToast = useCallback((msg: string) => {
    setToast(msg);
    setTimeout(() => {
      setToast((curr) => (curr === msg ? null : curr));
    }, 4000);
  }, []);

  useEffect(() => {
    let active = true;
    const validCourseId = courseId && UUID_REGEX.test(courseId.trim()) ? courseId.trim() : null;
    const validLessonId = lessonId && UUID_REGEX.test(lessonId.trim()) ? lessonId.trim() : null;

    if (!validCourseId && !validLessonId) {
      setDbActivities([]);
      return;
    }

    async function loadActivities() {
      setIsLoading(true);
      try {
        const headers = getClientRequestHeaders();
        const params = new URLSearchParams();
        if (validCourseId) params.set("courseId", validCourseId);
        if (validLessonId) params.set("lessonId", validLessonId);

        const res = await fetch(`/api/v1/practice/activities?${params.toString()}`, {
          headers,
        }).catch(() => null);

        if (active && res && res.ok) {
          const data = await res.json();
          if (Array.isArray(data)) {
            const filtered = validLessonId
              ? data.filter((a: any) => !a.lessonId || a.lessonId === validLessonId)
              : data;
            setDbActivities(filtered);
          }
        }
      } catch {
        // fallback cleanly to empty state
      } finally {
        if (active) setIsLoading(false);
      }
    }

    void loadActivities();
    return () => {
      active = false;
    };
  }, [courseId, lessonId]);

  useEffect(() => {
    setItemStates({});
    setSelectedIndexByTab({
      prompt_critique: 0,
      sql_reasoning: 0,
      threat_spotter: 0,
    });
  }, [courseId, lessonId]);

  const bank = useMemo(() => normalizeAppliedActivities(dbActivities), [dbActivities]);

  const availableTabs = useMemo<AppliedTabMode[]>(() => {
    const tabs: AppliedTabMode[] = [];
    if (bank.promptCritique.length > 0) tabs.push("prompt_critique");
    if (bank.sqlReasoning.length > 0) tabs.push("sql_reasoning");
    if (bank.threatSpotter.length > 0) tabs.push("threat_spotter");
    return tabs;
  }, [bank]);

  const [activeTab, setActiveTab] = useState<AppliedTabMode>(
    resolvedTarget?.appliedTab ?? "prompt_critique"
  );

  useEffect(() => {
    if (resolvedTarget?.labMode === "applied" && resolvedTarget.appliedTab) {
      setActiveTab(resolvedTarget.appliedTab);
    } else if (availableTabs.length > 0 && !availableTabs.includes(activeTab)) {
      setActiveTab(availableTabs[0]);
    }
  }, [resolvedTarget, availableTabs, activeTab]);

  // Select matching item if initialActivityKey was provided
  useEffect(() => {
    if (!initialActivityKey) return;
    const keyLower = initialActivityKey.trim().toLowerCase();
    const pIdx = bank.promptCritique.findIndex((i) => i.id.toLowerCase() === keyLower);
    const sIdx = bank.sqlReasoning.findIndex((i) => i.id.toLowerCase() === keyLower);
    const tIdx = bank.threatSpotter.findIndex((i) => i.id.toLowerCase() === keyLower);
    if (pIdx >= 0) {
      setActiveTab("prompt_critique");
      setSelectedIndexByTab((prev) => ({ ...prev, prompt_critique: pIdx }));
    } else if (sIdx >= 0) {
      setActiveTab("sql_reasoning");
      setSelectedIndexByTab((prev) => ({ ...prev, sql_reasoning: sIdx }));
    } else if (tIdx >= 0) {
      setActiveTab("threat_spotter");
      setSelectedIndexByTab((prev) => ({ ...prev, threat_spotter: tIdx }));
    }
  }, [initialActivityKey, bank]);

  const tabsToRender: AppliedTabMode[] =
    availableTabs.length > 0 ? availableTabs : ["prompt_critique", "sql_reasoning", "threat_spotter"];

  const getTabItemsCount = (tab: AppliedTabMode): number => {
    if (tab === "prompt_critique") return bank.promptCritique.length;
    if (tab === "sql_reasoning") return bank.sqlReasoning.length;
    return bank.threatSpotter.length;
  };

  const activePromptItem =
    bank.promptCritique[selectedIndexByTab.prompt_critique] ?? bank.promptCritique[0] ?? null;
  const activeSqlItem =
    bank.sqlReasoning[selectedIndexByTab.sql_reasoning] ?? bank.sqlReasoning[0] ?? null;
  const activeThreatItem =
    bank.threatSpotter[selectedIndexByTab.threat_spotter] ?? bank.threatSpotter[0] ?? null;

  const activeItemKey = useMemo(() => {
    if (activeTab === "prompt_critique" && activePromptItem) return `pc:${activePromptItem.id}`;
    if (activeTab === "sql_reasoning" && activeSqlItem) return `sql:${activeSqlItem.id}`;
    if (activeTab === "threat_spotter" && activeThreatItem) return `ts:${activeThreatItem.id}`;
    return `empty:${activeTab}`;
  }, [activeTab, activePromptItem, activeSqlItem, activeThreatItem]);

  const currentState: AppliedItemState = itemStates[activeItemKey] ?? DEFAULT_ITEM_STATE;

  const patchItemState = useCallback(
    (key: string, patch: Partial<AppliedItemState>) => {
      setItemStates((prev) => ({
        ...prev,
        [key]: { ...(prev[key] ?? DEFAULT_ITEM_STATE), ...patch },
      }));
    },
    []
  );

  const postPracticeAttempt = useCallback(
    async (params: {
      activityId?: string;
      activityType: AppliedTabMode;
      activityKey: string;
      activityTitle: string;
      score: number;
      passingScore?: number;
      details: Record<string, unknown>;
    }) => {
      const validCourseId = courseId && UUID_REGEX.test(courseId.trim()) ? courseId.trim() : null;
      if (!validCourseId) return;
      try {
        const headers = getClientRequestHeaders({ "Content-Type": "application/json" });
        const clampedScore = Math.max(0, Math.min(100, Math.round(params.score)));
        const passing = params.passingScore ?? 70;
        const res = await fetch("/api/v1/practice/attempts", {
          method: "POST",
          headers,
          body: JSON.stringify({
            activityId: params.activityId || null,
            courseId: validCourseId,
            lessonId: lessonId && UUID_REGEX.test(lessonId.trim()) ? lessonId.trim() : null,
            activityType: params.activityType,
            activityKey: params.activityKey,
            activityTitle: params.activityTitle,
            score: clampedScore,
            maxScore: 100,
            isPassed: clampedScore >= passing,
            detailJson: JSON.stringify(params.details),
          }),
        });
        if (res.ok) {
          invalidateDashboardCache();
          if (typeof window !== "undefined") {
            window.dispatchEvent(new Event("swms:data-mutated"));
          }
        }
      } catch {
        // non-blocking attempt persistence
      }
    },
    [courseId, lessonId]
  );

  const handleSubmitEval = async () => {
    const response = currentState.responseText.trim();
    if (!response || currentState.isEvaluating) return;

    let prompt = "";
    let expectedKeywords: string[] = [];
    let sampleAnswer = "";
    let activityId: string | undefined;
    let activityKey = activeItemKey;
    let activityTitle = lessonTitle;
    let passingScore = 70;
    let diagnosisCorrect: boolean | null = null;

    if (activeTab === "prompt_critique" && activePromptItem) {
      prompt = `Bối cảnh: ${activePromptItem.contextVi}\nPrompt gốc lỗi: ${activePromptItem.badPrompt}\nYêu cầu viết lại: ${activePromptItem.rewriteTaskVi}`;
      expectedKeywords = activePromptItem.expectedKeywords;
      sampleAnswer = activePromptItem.sampleImprovedPrompt;
      activityId = activePromptItem.sourceActivityId;
      activityKey = activePromptItem.id;
      activityTitle = activePromptItem.title;
      passingScore = activePromptItem.passingScore ?? 70;
      if (currentState.selectedOptionIdx !== null) {
        diagnosisCorrect = currentState.selectedOptionIdx === activePromptItem.correctFlawIndex;
      }
    } else if (activeTab === "sql_reasoning" && activeSqlItem) {
      prompt = `Schema: ${activeSqlItem.schemaSql}\nBài toán nghiệp vụ: ${activeSqlItem.businessQuestionVi}\nSQL dự thảo/lỗi: ${activeSqlItem.buggyOrDraftSql}`;
      expectedKeywords = activeSqlItem.expectedKeywords;
      sampleAnswer = activeSqlItem.sampleSolutionSql;
      activityId = activeSqlItem.sourceActivityId;
      activityKey = activeSqlItem.id;
      activityTitle = activeSqlItem.title;
      passingScore = activeSqlItem.passingScore ?? 70;
      if (currentState.selectedOptionIdx !== null) {
        diagnosisCorrect = currentState.selectedOptionIdx === activeSqlItem.correctDiagnosisIndex;
      }
    } else if (activeTab === "threat_spotter" && activeThreatItem) {
      prompt = `Kênh: ${activeThreatItem.channel}\nNội dung đáng ngờ (${activeThreatItem.artifactTitle}): ${activeThreatItem.artifactBody}\nNhiệm vụ xử lý: ${activeThreatItem.responseTaskVi}`;
      expectedKeywords = activeThreatItem.expectedKeywords;
      sampleAnswer = activeThreatItem.sampleSafeAction;
      activityId = activeThreatItem.sourceActivityId;
      activityKey = activeThreatItem.id;
      activityTitle = activeThreatItem.title;
      passingScore = activeThreatItem.passingScore ?? 70;
      if (currentState.selectedOptionIdx !== null) {
        diagnosisCorrect = currentState.selectedOptionIdx === activeThreatItem.correctVerdictIndex;
      }
    } else {
      return;
    }

    patchItemState(activeItemKey, { isEvaluating: true });
    const headers = getClientRequestHeaders();
    const evalResult = await evaluatePracticeWithAiOrOffline({
      mode: activeTab,
      lang: "vi-VN",
      prompt,
      userResponse: response,
      expectedKeywords,
      sampleAnswer,
      contextTitle: `${courseTitle} · ${activityTitle}`,
      minWords: 8,
      headers,
    });

    patchItemState(activeItemKey, {
      isEvaluating: false,
      evalResult,
      showSample: true,
    });

    await postPracticeAttempt({
      activityId,
      activityType: activeTab,
      activityKey,
      activityTitle,
      score: evalResult.score,
      passingScore,
      details: {
        gradingMode: evalResult.gradingMode,
        usedLiveEndpoint: evalResult.usedLiveEndpoint,
        diagnosisChecked: currentState.diagnosisChecked,
        diagnosisCorrect,
        selectedOptionIdx: currentState.selectedOptionIdx,
      },
    });
  };

  const handleSaveFsrs = async () => {
    if (currentState.isSavingFsrs || currentState.fsrsSaved) return;

    let front = "";
    let back = "";
    let tags: string[] = ["applied-lab", activeTab];

    if (activeTab === "prompt_critique" && activePromptItem) {
      const correctOpt = activePromptItem.flawOptions[activePromptItem.correctFlawIndex] ?? "";
      front = `[Prompt Critique] Khi xử lý tình huống "${activePromptItem.title}", lỗi chính trong prompt gốc là gì và cần sửa ra sao?`;
      back = `Lỗi chính: ${correctOpt}\n\nGiải thích: ${activePromptItem.explanationVi}\n\nPrompt chuẩn tham khảo:\n${activePromptItem.sampleImprovedPrompt}`;
      tags.push("ai-workflows", "prompt-engineering");
    } else if (activeTab === "sql_reasoning" && activeSqlItem) {
      const correctOpt = activeSqlItem.diagnosisOptions[activeSqlItem.correctDiagnosisIndex] ?? "";
      front = `[SQL Reasoning] ${activeSqlItem.businessQuestionVi} (Lỗi thường gặp ở truy vấn dự thảo là gì?)`;
      back = `Chẩn đoán: ${correctOpt}\n\nGiải thích: ${activeSqlItem.explanationVi}\n\nSQL chuẩn:\n${activeSqlItem.sampleSolutionSql}`;
      tags.push("data-sql", "sql-analytics");
    } else if (activeTab === "threat_spotter" && activeThreatItem) {
      const correctOpt = activeThreatItem.verdictOptions[activeThreatItem.correctVerdictIndex] ?? "";
      front = `[An toàn số · ${activeThreatItem.channel}] Dấu hiệu nhận diện và cách ứng phó với "${activeThreatItem.artifactTitle}"?`;
      back = `Kết luận: ${correctOpt}\n\nDấu hiệu cảnh báo: ${activeThreatItem.redFlags.join(" · ")}\n\nHành động an toàn:\n${activeThreatItem.sampleSafeAction}`;
      tags.push("cybersecurity", "an-toan-so");
    } else {
      return;
    }

    patchItemState(activeItemKey, { isSavingFsrs: true });
    const res = await saveWriteInFlashcardDeduplicated({
      front,
      back,
      lessonId,
      sourceTitle: `${courseTitle} · ${lessonTitle}`,
      tags,
      onCreateFlashcardQuick,
    });
    patchItemState(activeItemKey, {
      isSavingFsrs: false,
      fsrsSaved: res.ok,
    });

    if (res.ok) {
      showToast(
        res.deduplicated
          ? "Thẻ ghi nhớ FSRS này đã tồn tại (không tạo trùng)."
          : "Đã lưu tình huống thực hành vào thẻ FSRS v5!"
      );
    } else {
      showToast("Không thể lưu thẻ FSRS lúc này. Vui lòng thử lại.");
    }
  };

  const handleSaveAnalysisToNotebook = async () => {
    let md = "";
    if (activeTab === "prompt_critique" && activePromptItem) {
      md = `### 🤖 Phản biện Prompt: ${activePromptItem.title}\n- **Bối cảnh:** ${activePromptItem.contextVi}\n- **Bài viết lại của tôi:**\n\`\`\`text\n${currentState.responseText.trim() || "(chưa nhập)"}\n\`\`\`\n- **Prompt chuẩn:**\n\`\`\`text\n${activePromptItem.sampleImprovedPrompt}\n\`\`\``;
    } else if (activeTab === "sql_reasoning" && activeSqlItem) {
      md = `### 🗄️ Chẩn đoán SQL: ${activeSqlItem.title}\n- **Câu hỏi:** ${activeSqlItem.businessQuestionVi}\n- **Truy vấn của tôi:**\n\`\`\`sql\n${currentState.responseText.trim() || "(chưa nhập)"}\n\`\`\`\n- **SQL chuẩn:**\n\`\`\`sql\n${activeSqlItem.sampleSolutionSql}\n\`\`\``;
    } else if (activeTab === "threat_spotter" && activeThreatItem) {
      md = `### 🛡️ An toàn số (${activeThreatItem.channel}): ${activeThreatItem.artifactTitle}\n- **Dấu hiệu đỏ:** ${activeThreatItem.redFlags.join(", ")}\n- **Kế hoạch xử lý của tôi:** ${currentState.responseText.trim() || "(chưa nhập)"}\n- **Khuyến nghị an toàn:** ${activeThreatItem.sampleSafeAction}`;
    }
    if (!md) return;
    await onSaveToNotebook(md);
    showToast("Đã lưu bài thực hành vào Sổ tay bài học!");
  };

  const renderTabIcon = (tab: AppliedTabMode) => {
    if (tab === "prompt_critique") return <Sparkles className="w-3.5 h-3.5 shrink-0" />;
    if (tab === "sql_reasoning") return <Database className="w-3.5 h-3.5 shrink-0" />;
    return <ShieldAlert className="w-3.5 h-3.5 shrink-0" />;
  };

  const renderDiagnosisSection = (
    options: string[],
    correctIndex: number,
    explanationVi: string,
    headingLabel: string,
    extraAfterCheck?: React.ReactNode
  ) => {
    const isCorrect =
      currentState.diagnosisChecked && currentState.selectedOptionIdx === correctIndex;
    return (
      <div className="space-y-3 rounded-xl border border-border bg-bg-secondary/60 p-4">
        <p className="text-xs font-bold text-text-primary uppercase tracking-wider">
          {headingLabel}
        </p>
        <div className="space-y-2">
          {options.map((opt, idx) => {
            const isSelected = currentState.selectedOptionIdx === idx;
            const isOptCorrect = idx === correctIndex;
            let cardClass =
              "border-border bg-bg-primary hover:border-accent/60 text-text-primary";
            if (currentState.diagnosisChecked) {
              if (isOptCorrect) {
                cardClass =
                  "border-emerald-500 bg-emerald-50/80 dark:bg-emerald-950/40 text-emerald-900 dark:text-emerald-200";
              } else if (isSelected) {
                cardClass =
                  "border-rose-500 bg-rose-50/80 dark:bg-rose-950/40 text-rose-900 dark:text-rose-200";
              }
            } else if (isSelected) {
              cardClass = "border-accent bg-accent-subtle text-text-primary";
            }

            return (
              <label
                key={idx}
                className={`flex items-start gap-2.5 p-3 rounded-lg border text-xs cursor-pointer transition ${cardClass}`}
              >
                <input
                  type="radio"
                  name={`applied-diagnosis-${activeItemKey}`}
                  data-testid={`applied-diagnosis-option-${idx}`}
                  checked={isSelected}
                  onChange={() =>
                    patchItemState(activeItemKey, {
                      selectedOptionIdx: idx,
                      diagnosisChecked: false,
                    })
                  }
                  className="mt-0.5 w-4 h-4 accent-accent shrink-0 cursor-pointer"
                />
                <span className="leading-relaxed font-medium">{opt}</span>
              </label>
            );
          })}
        </div>

        <div className="flex flex-wrap items-center gap-2 pt-1">
          <button
            type="button"
            data-testid="applied-check-diagnosis-btn"
            disabled={currentState.selectedOptionIdx === null}
            onClick={() => patchItemState(activeItemKey, { diagnosisChecked: true })}
            className="inline-flex items-center gap-1.5 min-h-[38px] px-4 py-1.5 rounded-lg bg-accent hover:bg-accent-hover disabled:opacity-50 text-white text-xs font-semibold transition cursor-pointer"
          >
            <CheckCircle2 className="w-3.5 h-3.5" />
            <span>Kiểm tra chẩn đoán</span>
          </button>
        </div>

        {currentState.diagnosisChecked && (
          <div
            data-testid="applied-diagnosis-feedback"
            className={`rounded-lg border p-3.5 space-y-2 text-xs leading-relaxed ${
              isCorrect
                ? "border-emerald-300 dark:border-emerald-800 bg-emerald-50/90 dark:bg-emerald-950/40 text-emerald-950 dark:text-emerald-100"
                : "border-rose-300 dark:border-rose-800 bg-rose-50/90 dark:bg-rose-950/40 text-rose-950 dark:text-rose-100"
            }`}
          >
            <div className="flex items-center gap-1.5 font-bold">
              {isCorrect ? (
                <>
                  <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                  <span>Chẩn đoán chính xác!</span>
                </>
              ) : (
                <>
                  <XCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
                  <span>Chưa khớp đáp án chuẩn — xem giải thích bên dưới:</span>
                </>
              )}
            </div>
            <p>{explanationVi}</p>
            {extraAfterCheck}
          </div>
        )}
      </div>
    );
  };

  const renderWriteInAndEvalSection = (params: {
    taskPrompt: string;
    placeholder: string;
    sampleTitle: string;
    sampleBody: string;
    isCode?: boolean;
    passingScore?: number;
  }) => {
    const evalRes = currentState.evalResult;
    const passed = evalRes ? evalRes.score >= (params.passingScore ?? 70) : false;

    return (
      <div className="space-y-3 rounded-xl border border-border bg-bg-primary p-4">
        <label
          htmlFor={`applied-input-${activeItemKey}`}
          className="text-xs font-bold text-text-primary block leading-relaxed"
        >
          {params.taskPrompt}
        </label>

        <textarea
          id={`applied-input-${activeItemKey}`}
          data-testid="applied-response-input"
          rows={params.isCode ? 6 : 4}
          value={currentState.responseText}
          onChange={(e) =>
            patchItemState(activeItemKey, {
              responseText: e.target.value,
            })
          }
          placeholder={params.placeholder}
          className={`w-full p-3.5 rounded-xl border border-border bg-bg-secondary text-xs sm:text-sm text-text-primary leading-relaxed focus:outline-none focus:border-accent ${
            params.isCode ? "font-mono" : ""
          }`}
        />

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            data-testid="applied-submit-eval-btn"
            disabled={!currentState.responseText.trim() || currentState.isEvaluating}
            onClick={() => void handleSubmitEval()}
            className="inline-flex items-center gap-1.5 min-h-[40px] px-4 py-2 rounded-lg bg-accent hover:bg-accent-hover disabled:opacity-50 text-white text-xs font-semibold transition cursor-pointer shadow-xs"
          >
            {currentState.isEvaluating ? (
              <Loader2 className="w-3.5 h-3.5 animate-spin" />
            ) : (
              <Send className="w-3.5 h-3.5" />
            )}
            <span>{currentState.isEvaluating ? "Đang chấm..." : "Nộp bài & Chấm điểm"}</span>
          </button>

          <button
            type="button"
            onClick={() => patchItemState(activeItemKey, { showSample: !currentState.showSample })}
            className="inline-flex items-center gap-1.5 min-h-[40px] px-3.5 py-2 rounded-lg border border-border bg-bg-secondary text-xs font-medium text-text-primary hover:border-accent/50 transition cursor-pointer"
          >
            {currentState.showSample ? (
              <EyeOff className="w-3.5 h-3.5" />
            ) : (
              <Eye className="w-3.5 h-3.5" />
            )}
            <span>{currentState.showSample ? "Ẩn mẫu chuẩn" : "Xem mẫu tham khảo"}</span>
          </button>

          <button
            type="button"
            data-testid="applied-save-fsrs-btn"
            disabled={currentState.isSavingFsrs || currentState.fsrsSaved}
            onClick={() => void handleSaveFsrs()}
            className={`inline-flex items-center gap-1.5 min-h-[40px] px-3.5 py-2 rounded-lg border text-xs font-semibold transition cursor-pointer ${
              currentState.fsrsSaved
                ? "border-emerald-400 dark:border-emerald-700 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 cursor-default"
                : "border-border bg-bg-secondary text-text-primary hover:border-accent"
            }`}
          >
            <BookmarkPlus className="w-3.5 h-3.5" />
            <span>
              {currentState.isSavingFsrs
                ? "Đang lưu..."
                : currentState.fsrsSaved
                  ? "Đã lưu FSRS ✓"
                  : "Lưu vào FSRS"}
            </span>
          </button>

          <button
            type="button"
            onClick={() => void handleSaveAnalysisToNotebook()}
            className="inline-flex items-center gap-1.5 min-h-[40px] px-3.5 py-2 rounded-lg border border-border bg-bg-secondary text-xs font-medium text-text-secondary hover:text-text-primary transition cursor-pointer"
          >
            <NotebookPen className="w-3.5 h-3.5" />
            <span>Lưu Sổ tay</span>
          </button>
        </div>

        {currentState.showSample && params.sampleBody && (
          <div className="rounded-xl border border-emerald-200 dark:border-emerald-800 bg-emerald-50/70 dark:bg-emerald-950/30 p-3.5 space-y-1.5">
            <span className="text-xs font-bold text-emerald-800 dark:text-emerald-300 block">
              {params.sampleTitle}
            </span>
            <pre
              className={`text-xs text-emerald-950 dark:text-emerald-100 whitespace-pre-wrap leading-relaxed ${
                params.isCode ? "font-mono" : "font-sans"
              }`}
            >
              {params.sampleBody}
            </pre>
          </div>
        )}

        {evalRes && (
          <div
            data-testid="applied-eval-result"
            className="rounded-xl border border-border bg-bg-secondary p-4 space-y-3 text-xs"
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span
                data-testid="applied-grading-badge"
                className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full font-bold border ${
                  evalRes.usedLiveEndpoint
                    ? "bg-emerald-500/15 text-emerald-800 dark:text-emerald-200 border-emerald-500/35"
                    : "bg-amber-500/15 text-amber-800 dark:text-amber-200 border-amber-500/35"
                }`}
              >
                <Sparkles className="w-3.5 h-3.5" />
                <span>{evalRes.gradingLabelVi}</span>
              </span>

              <span
                className={`px-3 py-1 rounded-full font-extrabold text-white ${
                  passed ? "bg-emerald-600" : "bg-amber-600"
                }`}
              >
                Điểm: {evalRes.score}/100
              </span>
            </div>

            <p className="text-text-primary leading-relaxed">{evalRes.feedbackVi}</p>

            {(evalRes.matchedKeywords.length > 0 || evalRes.missingKeywords.length > 0) && (
              <div className="space-y-1.5">
                {evalRes.matchedKeywords.length > 0 && (
                  <div className="flex flex-wrap items-center gap-1.5">
                    <span className="font-semibold text-emerald-700 dark:text-emerald-300">
                      Ý đạt được:
                    </span>
                    {evalRes.matchedKeywords.map((kw) => (
                      <span
                        key={kw}
                        className="px-2 py-0.5 rounded bg-emerald-500/15 text-emerald-800 dark:text-emerald-200 font-medium"
                      >
                        ✓ {kw}
                      </span>
                    ))}
                  </div>
                )}
                {evalRes.missingKeywords.length > 0 && (
                  <div className="flex flex-wrap items-center gap-1.5">
                    <span className="font-semibold text-amber-700 dark:text-amber-300">
                      Cần bổ sung:
                    </span>
                    {evalRes.missingKeywords.map((kw) => (
                      <span
                        key={kw}
                        className="px-2 py-0.5 rounded bg-amber-500/15 text-amber-800 dark:text-amber-200 font-medium"
                      >
                        + {kw}
                      </span>
                    ))}
                  </div>
                )}
              </div>
            )}

            {evalRes.strengths.length > 0 && (
              <ul className="list-disc pl-4 space-y-1 text-text-secondary">
                {evalRes.strengths.map((s, i) => (
                  <li key={i}>{s}</li>
                ))}
              </ul>
            )}

            {evalRes.improvements.length > 0 && (
              <ul className="list-disc pl-4 space-y-1 text-amber-800 dark:text-amber-200">
                {evalRes.improvements.map((imp, i) => (
                  <li key={i}>{imp}</li>
                ))}
              </ul>
            )}
          </div>
        )}
      </div>
    );
  };

  return (
    <div data-testid="applied-systems-lab" className="space-y-4 font-sans">
      {/* Header + Dynamic Mode Tabs */}
      <div className="rounded-xl border border-border bg-bg-primary p-4 space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <span className="text-xs font-bold text-accent inline-flex items-center gap-1.5">
              <Terminal className="w-4 h-4" />
              <span>Phòng Lab Hệ Thống Ứng Dụng · AI, Dữ Liệu &amp; An Toàn Số</span>
            </span>
            <p className="text-xs text-text-secondary mt-0.5">
              Chẩn đoán lỗi sai thực tế, viết lại câu lệnh/kế hoạch và chấm phản hồi trung thực qua AI hoặc bộ từ khóa trên máy.
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {tabsToRender.map((tab) => {
            const isActive = activeTab === tab;
            const count = getTabItemsCount(tab);
            return (
              <button
                key={tab}
                type="button"
                data-testid={`applied-tab-${tab}`}
                onClick={() => setActiveTab(tab)}
                className={`inline-flex items-center gap-1.5 min-h-[38px] px-3.5 py-1.5 rounded-lg text-xs font-semibold transition cursor-pointer ${
                  isActive
                    ? "bg-accent text-white shadow-xs"
                    : "border border-border bg-bg-secondary text-text-secondary hover:text-text-primary"
                }`}
              >
                {renderTabIcon(tab)}
                <span>{APPLIED_TAB_LABELS_VI[tab]}</span>
                {count > 0 && (
                  <span
                    className={`ml-0.5 px-1.5 py-0.2 rounded-full text-[10px] font-bold ${
                      isActive ? "bg-white/20 text-white" : "bg-bg-primary text-text-secondary"
                    }`}
                  >
                    {count}
                  </span>
                )}
              </button>
            );
          })}
        </div>
      </div>

      {toast && (
        <div className="rounded-lg border border-emerald-300 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/40 px-3.5 py-2.5 text-xs font-medium text-emerald-800 dark:text-emerald-200 flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          <span>{toast}</span>
        </div>
      )}

      {isLoading ? (
        <div className="rounded-xl border border-border bg-bg-primary p-8 flex flex-col items-center justify-center gap-2">
          <Loader2 className="w-6 h-6 text-accent animate-spin" />
          <p className="text-xs text-text-secondary">Đang tải bài tập thực hành từ cơ sở dữ liệu...</p>
        </div>
      ) : activeTab === "prompt_critique" && activePromptItem ? (
        <div className="rounded-xl border border-border bg-bg-primary p-4 sm:p-5 space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-border">
            <div>
              <span className="text-[11px] font-semibold uppercase tracking-wider text-accent">
                Prompt Critique · Phản biện &amp; Tối ưu Prompt
              </span>
              <h3 className="text-sm sm:text-base font-bold text-text-primary mt-0.5">
                {activePromptItem.title}
              </h3>
            </div>
            {bank.promptCritique.length > 1 && (
              <div className="flex items-center gap-1.5">
                {bank.promptCritique.map((it, idx) => (
                  <button
                    key={it.id}
                    type="button"
                    onClick={() =>
                      setSelectedIndexByTab((prev) => ({ ...prev, prompt_critique: idx }))
                    }
                    className={`px-2.5 py-1 rounded-md text-xs font-semibold cursor-pointer transition ${
                      selectedIndexByTab.prompt_critique === idx
                        ? "bg-accent text-white"
                        : "bg-bg-secondary border border-border text-text-secondary hover:text-text-primary"
                    }`}
                  >
                    Bài #{idx + 1}
                  </button>
                ))}
              </div>
            )}
          </div>

          {activePromptItem.contextVi && (
            <div className="rounded-lg border border-border bg-bg-secondary/70 p-3 text-xs text-text-secondary leading-relaxed">
              <strong className="text-text-primary">Bối cảnh: </strong>
              {activePromptItem.contextVi}
            </div>
          )}

          <div className="space-y-1.5">
            <span className="text-xs font-bold text-rose-700 dark:text-rose-300 flex items-center gap-1.5">
              <AlertTriangle className="w-3.5 h-3.5" />
              <span>Prompt gốc có vấn đề:</span>
            </span>
            <pre className="p-3.5 rounded-xl border border-rose-200 dark:border-rose-900/70 bg-rose-50/50 dark:bg-rose-950/20 font-mono text-xs text-text-primary whitespace-pre-wrap leading-relaxed">
              {activePromptItem.badPrompt}
            </pre>
          </div>

          {activePromptItem.flawOptions.length > 0 &&
            renderDiagnosisSection(
              activePromptItem.flawOptions,
              activePromptItem.correctFlawIndex,
              activePromptItem.explanationVi,
              "Bước 1 · Chọn lỗ hổng nghiêm trọng nhất trong Prompt gốc:"
            )}

          {renderWriteInAndEvalSection({
            taskPrompt: `Bước 2 · ${activePromptItem.rewriteTaskVi}`,
            placeholder:
              "Viết lại Prompt hoàn chỉnh gồm Vai trò (Role), Bối cảnh, Ràng buộc (Constraints) và Định dạng đầu ra...",
            sampleTitle: "Prompt cải tiến mẫu chuẩn:",
            sampleBody: activePromptItem.sampleImprovedPrompt,
            isCode: true,
            passingScore: activePromptItem.passingScore,
          })}
        </div>
      ) : activeTab === "sql_reasoning" && activeSqlItem ? (
        <div className="rounded-xl border border-border bg-bg-primary p-4 sm:p-5 space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-border">
            <div>
              <span className="text-[11px] font-semibold uppercase tracking-wider text-accent">
                SQL Reasoning · Đọc Schema &amp; Gỡ Bẫy Truy Vấn
              </span>
              <h3 className="text-sm sm:text-base font-bold text-text-primary mt-0.5">
                {activeSqlItem.title}
              </h3>
            </div>
            {bank.sqlReasoning.length > 1 && (
              <div className="flex items-center gap-1.5">
                {bank.sqlReasoning.map((it, idx) => (
                  <button
                    key={it.id}
                    type="button"
                    onClick={() =>
                      setSelectedIndexByTab((prev) => ({ ...prev, sql_reasoning: idx }))
                    }
                    className={`px-2.5 py-1 rounded-md text-xs font-semibold cursor-pointer transition ${
                      selectedIndexByTab.sql_reasoning === idx
                        ? "bg-accent text-white"
                        : "bg-bg-secondary border border-border text-text-secondary hover:text-text-primary"
                    }`}
                  >
                    Bài #{idx + 1}
                  </button>
                ))}
              </div>
            )}
          </div>

          {activeSqlItem.schemaSql && (
            <div className="space-y-1.5">
              <span className="text-xs font-bold text-text-primary flex items-center gap-1.5">
                <Database className="w-3.5 h-3.5 text-accent" />
                <span>Cấu trúc bảng (Schema SQL):</span>
              </span>
              <pre className="p-3.5 rounded-xl border border-border bg-bg-secondary font-mono text-xs text-text-primary whitespace-pre-wrap leading-relaxed overflow-x-auto">
                {activeSqlItem.schemaSql}
              </pre>
            </div>
          )}

          {activeSqlItem.sampleDataNoteVi && (
            <div className="rounded-lg border border-border bg-bg-secondary/60 p-3 text-xs text-text-secondary leading-relaxed">
              <strong className="text-text-primary">Ghi chú dữ liệu: </strong>
              {activeSqlItem.sampleDataNoteVi}
            </div>
          )}

          <div className="rounded-lg border border-indigo-200 dark:border-indigo-900/60 bg-indigo-50/50 dark:bg-indigo-950/20 p-3.5 text-xs sm:text-sm text-text-primary leading-relaxed">
            <strong className="text-accent">Câu hỏi nghiệp vụ: </strong>
            {activeSqlItem.businessQuestionVi}
          </div>

          {activeSqlItem.buggyOrDraftSql && (
            <div className="space-y-1.5">
              <span className="text-xs font-bold text-amber-700 dark:text-amber-300 flex items-center gap-1.5">
                <Code2 className="w-3.5 h-3.5" />
                <span>Truy vấn dự thảo cần rà soát lỗi:</span>
              </span>
              <pre className="p-3.5 rounded-xl border border-amber-200 dark:border-amber-900/70 bg-amber-50/40 dark:bg-amber-950/20 font-mono text-xs text-text-primary whitespace-pre-wrap leading-relaxed overflow-x-auto">
                {activeSqlItem.buggyOrDraftSql}
              </pre>
            </div>
          )}

          {activeSqlItem.diagnosisOptions.length > 0 &&
            renderDiagnosisSection(
              activeSqlItem.diagnosisOptions,
              activeSqlItem.correctDiagnosisIndex,
              activeSqlItem.explanationVi,
              "Bước 1 · Chọn nguyên nhân gây sai số hoặc bẫy logic trong câu SQL trên:"
            )}

          {renderWriteInAndEvalSection({
            taskPrompt:
              "Bước 2 · Viết lại câu truy vấn SQL đúng (hoặc giải thích cách sửa từng bước bằng SQL):",
            placeholder: "SELECT ...\nFROM ...\nWHERE ...",
            sampleTitle: "Câu truy vấn SQL chuẩn đối chiếu:",
            sampleBody: activeSqlItem.sampleSolutionSql,
            isCode: true,
            passingScore: activeSqlItem.passingScore,
          })}
        </div>
      ) : activeTab === "threat_spotter" && activeThreatItem ? (
        <div className="rounded-xl border border-border bg-bg-primary p-4 sm:p-5 space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-border">
            <div className="space-y-1">
              <div className="flex flex-wrap items-center gap-2">
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-500/30">
                  Kênh: {activeThreatItem.channel}
                </span>
                <span className="text-[11px] font-semibold uppercase tracking-wider text-accent">
                  Threat Spotter · An toàn số thực chiến
                </span>
              </div>
              <h3 className="text-sm sm:text-base font-bold text-text-primary">
                {activeThreatItem.title}
              </h3>
            </div>
            {bank.threatSpotter.length > 1 && (
              <div className="flex items-center gap-1.5">
                {bank.threatSpotter.map((it, idx) => (
                  <button
                    key={it.id}
                    type="button"
                    onClick={() =>
                      setSelectedIndexByTab((prev) => ({ ...prev, threat_spotter: idx }))
                    }
                    className={`px-2.5 py-1 rounded-md text-xs font-semibold cursor-pointer transition ${
                      selectedIndexByTab.threat_spotter === idx
                        ? "bg-accent text-white"
                        : "bg-bg-secondary border border-border text-text-secondary hover:text-text-primary"
                    }`}
                  >
                    Tình huống #{idx + 1}
                  </button>
                ))}
              </div>
            )}
          </div>

          <div className="rounded-xl border border-amber-300/80 dark:border-amber-800/70 bg-amber-50/50 dark:bg-amber-950/20 p-4 space-y-2">
            <div className="flex items-center gap-2 text-xs font-bold text-amber-900 dark:text-amber-200">
              <ShieldAlert className="w-4 h-4 shrink-0" />
              <span>{activeThreatItem.artifactTitle}</span>
            </div>
            <p className="text-xs sm:text-sm text-text-primary whitespace-pre-wrap leading-relaxed font-mono bg-bg-primary/80 p-3 rounded-lg border border-border">
              {activeThreatItem.artifactBody}
            </p>
          </div>

          {activeThreatItem.verdictOptions.length > 0 &&
            renderDiagnosisSection(
              activeThreatItem.verdictOptions,
              activeThreatItem.correctVerdictIndex,
              activeThreatItem.explanationVi,
              "Bước 1 · Đánh giá mức độ rủi ro và bản chất tình huống trên:",
              activeThreatItem.redFlags.length > 0 ? (
                <div className="pt-2 space-y-1.5">
                  <span className="text-[11px] font-bold uppercase tracking-wider block">
                    Các dấu hiệu cảnh báo đỏ (Red Flags):
                  </span>
                  <div className="flex flex-wrap gap-1.5">
                    {activeThreatItem.redFlags.map((flag, i) => (
                      <span
                        key={i}
                        className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-rose-500/15 text-rose-800 dark:text-rose-200 border border-rose-500/30"
                      >
                        <AlertTriangle className="w-3 h-3 shrink-0" />
                        <span>{flag}</span>
                      </span>
                    ))}
                  </div>
                </div>
              ) : null
            )}

          {renderWriteInAndEvalSection({
            taskPrompt: `Bước 2 · ${activeThreatItem.responseTaskVi}`,
            placeholder:
              "Nêu các bước xử lý an toàn của bạn (ví dụ: không bấm link, không cài APK, gọi tổng đài chính thức, khóa thẻ/đổi Passkeys)...",
            sampleTitle: "Quy trình ứng phó an toàn khuyến nghị:",
            sampleBody: activeThreatItem.sampleSafeAction,
            isCode: false,
            passingScore: activeThreatItem.passingScore,
          })}
        </div>
      ) : (
        <div
          data-testid="applied-empty-state"
          className="rounded-xl border border-dashed border-border bg-bg-primary p-6 text-center space-y-2"
        >
          <ShieldCheck className="w-8 h-8 text-accent mx-auto" />
          <h3 className="text-sm font-bold text-text-primary">
            Chưa có bài tập {APPLIED_TAB_LABELS_VI[activeTab]} gắn riêng cho bài học này
          </h3>
          <p className="text-xs text-text-secondary max-w-lg mx-auto leading-relaxed">
            Phòng Lab Hệ Thống Ứng Dụng hỗ trợ 3 chế độ thực hành chuyên sâu: Phản biện Prompt (AI Workflows), Chẩn đoán &amp; Viết SQL (Data Analytics) và Nhận diện Lừa đảo Số (Cybersecurity). Hãy mở một bài học thuộc các lộ trình này hoặc chuyển sang tab thực hành khác phía trên.
          </p>
        </div>
      )}
    </div>
  );
}

export default AppliedSystemsLab;
