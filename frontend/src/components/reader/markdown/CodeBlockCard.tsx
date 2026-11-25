"use client";

import React, { useState, useMemo } from "react";
import {
  Copy,
  Check,
  Workflow,
  Eye,
  Code2,
  Layers,
  GitFork,
  ArrowRight,
  ArrowLeftRight,
  Database,
  HelpCircle,
  PlayCircle,
  Sparkles,
  Activity,
} from "lucide-react";

export interface CodeBlockCardProps {
  lang: string;
  code: string;
  onOpenPractice?: () => void;
}

// ==========================================
// MERMAID PARSER TYPES & DATA STRUCTURES
// ==========================================

type MermaidDiagramType = "sequence" | "flowchart" | "class" | "er" | "other";

interface SequenceParticipant {
  id: string;
  label: string;
  isActor: boolean;
}

type SequenceItem =
  | {
      kind: "message";
      from: string;
      to: string;
      arrow: string;
      message: string;
      isDotted: boolean;
    }
  | {
      kind: "note";
      text: string;
      targets: string[];
    };

interface SequenceData {
  participants: SequenceParticipant[];
  items: SequenceItem[];
}

type FlowNodeShape =
  | "decision"
  | "terminal"
  | "database"
  | "subroutine"
  | "rounded"
  | "process"
  | "io";

interface FlowNode {
  id: string;
  label: string;
  shape: FlowNodeShape;
}

interface FlowEdge {
  from: string;
  to: string;
  label?: string;
  isDotted: boolean;
  isThick: boolean;
  isBidirectional: boolean;
}

interface FlowData {
  direction: "TD" | "LR" | "BT" | "RL";
  nodes: FlowNode[];
  edges: FlowEdge[];
}

interface ClassEntity {
  name: string;
  members: string[];
}

// ==========================================
// PARSER IMPLEMENTATION
// ==========================================

function detectDiagramType(code: string): MermaidDiagramType {
  const lines = code.split("\n");
  for (const rawLine of lines) {
    const line = rawLine.trim();
    if (!line || line.startsWith("%%")) continue;
    if (/^sequenceDiagram\b/i.test(line)) return "sequence";
    if (/^(?:flowchart|graph)\b/i.test(line)) return "flowchart";
    if (/^classDiagram\b/i.test(line)) return "class";
    if (/^erDiagram\b/i.test(line)) return "er";
    return "other";
  }
  return "other";
}

function cleanMermaidText(raw: string): string {
  if (!raw) return "";
  return raw
    .replace(/^["']|["']$/g, "")
    .replace(/<br\s*\/?>/gi, " • ")
    .trim();
}

function parseSequenceDiagram(code: string): SequenceData {
  const lines = code.split("\n");
  const participants = new Map<string, SequenceParticipant>();
  const items: SequenceItem[] = [];

  for (const rawLine of lines) {
    const line = rawLine.trim();
    if (
      !line ||
      line.startsWith("%%") ||
      /^sequenceDiagram\b/i.test(line) ||
      line === "autonumber"
    ) {
      continue;
    }

    // Explicit actor / participant declaration
    const partMatch = line.match(
      /^(actor|participant)\s+([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)(?:\s+as\s+(.+))?$/i
    );
    if (partMatch) {
      const isActor = partMatch[1].toLowerCase() === "actor";
      const id = partMatch[2];
      const label = partMatch[3] ? cleanMermaidText(partMatch[3]) : id;
      participants.set(id, { id, label, isActor });
      continue;
    }

    // Stage note: Note over A,B: Text | Note left of / right of A: Text
    const noteMatch = line.match(
      /^Note\s+(?:over|left of|right of)\s+([a-zA-Z0-9_,\s\-]+):\s*(.+)$/i
    );
    if (noteMatch) {
      const targets = noteMatch[1]
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);
      items.push({
        kind: "note",
        text: cleanMermaidText(noteMatch[2]),
        targets,
      });
      continue;
    }

    // Message interactions: From ->> To: Message
    // Arrows: -->>, ->>, --x, -x, -->, ->
    const msgMatch = line.match(
      /^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\s*(-->>|->>|--x|-x|-->|->)\s*([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\s*:\s*(.+)$/
    );
    if (msgMatch) {
      const from = msgMatch[1];
      const arrow = msgMatch[2];
      const to = msgMatch[3];
      const message = cleanMermaidText(msgMatch[4]);

      if (!participants.has(from)) {
        participants.set(from, { id: from, label: from, isActor: false });
      }
      if (!participants.has(to)) {
        participants.set(to, { id: to, label: to, isActor: false });
      }

      items.push({
        kind: "message",
        from,
        to,
        arrow,
        message,
        isDotted: arrow.includes("--"),
      });
    }
  }

  return {
    participants: Array.from(participants.values()),
    items,
  };
}

function parseFlowchart(code: string): FlowData {
  const lines = code.split("\n");
  let direction: FlowData["direction"] = "TD";
  const nodes = new Map<string, FlowNode>();
  const edges: FlowEdge[] = [];

  const firstLine = lines.find((l) =>
    /^(?:flowchart|graph)\s+([A-Za-z]+)/i.test(l.trim())
  );
  if (firstLine) {
    const dirMatch = firstLine.trim().match(/^(?:flowchart|graph)\s+([A-Za-z]+)/i);
    if (dirMatch) {
      const d = dirMatch[1].toUpperCase();
      if (d === "LR" || d === "TD" || d === "BT" || d === "RL") {
        direction = d as FlowData["direction"];
      } else if (d === "TB") {
        direction = "TD";
      }
    }
  }

  function parseNodeToken(token: string): FlowNode | null {
    let t = token.trim();
    if (!t) return null;

    // Strip classDef attachment like :::phaseStyle
    t = t.replace(/:::[a-zA-Z0-9_]+$/, "").trim();

    // 1. Terminal ([ ... ])
    let m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\(\[([\s\S]*?)\]\)$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "terminal" };

    // 2. Decision { ... }
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\{([\s\S]*?)\}$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "decision" };

    // 3. Database [( ... )]
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\[\(([\s\S]*?)\)\]$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "database" };

    // 4. Subroutine [[ ... ]]
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\[\[([\s\S]*?)\]\]$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "subroutine" };

    // 5. Parallelogram [/ ... /]
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\[\/([\s\S]*?)\/\]$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "io" };

    // 6. Rounded ( ... )
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\(([\s\S]*?)\)$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "rounded" };

    // 7. Process [ ... ]
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)\[([\s\S]*?)\]$/);
    if (m) return { id: m[1], label: cleanMermaidText(m[2]), shape: "process" };

    // 8. Plain ID without brackets
    m = t.match(/^([a-zA-Z0-9_]+(?:-[a-zA-Z0-9_]+)*)$/);
    if (m) return { id: m[1], label: m[1], shape: "process" };

    return null;
  }

  const edgeRegex =
    /\s*(<-->\|[^|]+\||<-->|==\s*[^=]+\s*==>|==>\|[^|]+\||==>|-\.\s*[^.]+\s*\.->|-\.->\|[^|]+\||-\.->|--\s*[^-]+\s*-->|-->\|[^|]+\||-->|---\|[^|]+\||---)\s*/g;

  for (const rawLine of lines) {
    const line = rawLine.trim();
    if (
      !line ||
      line.startsWith("%%") ||
      /^(?:flowchart|graph|subgraph|end|classDef|class|style|linkStyle)\b/i.test(line)
    ) {
      continue;
    }

    const matches = Array.from(line.matchAll(edgeRegex));

    if (matches.length > 0) {
      const nodeTokens: string[] = [];
      let lastIdx = 0;
      for (const m of matches) {
        if (m.index !== undefined) {
          const seg = line.slice(lastIdx, m.index).trim();
          if (seg) nodeTokens.push(seg);
          lastIdx = m.index + m[0].length;
        }
      }
      const lastSeg = line.slice(lastIdx).trim();
      if (lastSeg) nodeTokens.push(lastSeg);

      for (let i = 0; i < matches.length; i++) {
        const fromToken = nodeTokens[i];
        const toToken = nodeTokens[i + 1];
        if (!fromToken || !toToken) continue;

        const fromParsed = parseNodeToken(fromToken);
        const toParsed = parseNodeToken(toToken);

        if (fromParsed) {
          const existing = nodes.get(fromParsed.id);
          if (!existing || (fromParsed.label !== fromParsed.id && existing.label === existing.id)) {
            nodes.set(fromParsed.id, fromParsed);
          }
        }
        if (toParsed) {
          const existing = nodes.get(toParsed.id);
          if (!existing || (toParsed.label !== toParsed.id && existing.label === existing.id)) {
            nodes.set(toParsed.id, toParsed);
          }
        }

        const edgeStr = matches[i][0].trim();
        let edgeLabel = "";
        const isDotted = edgeStr.includes("-.");
        const isThick = edgeStr.includes("==");
        const isBidirectional = edgeStr.includes("<-->");

        const pipeMatch = edgeStr.match(/\|([^|]+)\|/);
        if (pipeMatch) {
          edgeLabel = pipeMatch[1].trim();
        } else {
          const textMatch = edgeStr.match(
            /--(.*?)(?:-->|---)|==(.*?)==>|-\.(.*?)\.->/
          );
          if (textMatch) {
            edgeLabel = (
              textMatch[1] ||
              textMatch[2] ||
              textMatch[3] ||
              ""
            ).trim();
          }
        }

        if (fromParsed && toParsed) {
          edges.push({
            from: fromParsed.id,
            to: toParsed.id,
            label: cleanMermaidText(edgeLabel),
            isDotted,
            isThick,
            isBidirectional,
          });
        }
      }
    } else {
      // Standalone node declaration
      const single = parseNodeToken(line);
      if (single) {
        const existing = nodes.get(single.id);
        if (!existing || (single.label !== single.id && existing.label === existing.id)) {
          nodes.set(single.id, single);
        }
      }
    }
  }

  return {
    direction,
    nodes: Array.from(nodes.values()),
    edges,
  };
}

function parseClassDiagram(code: string): ClassEntity[] {
  const lines = code.split("\n");
  const classes = new Map<string, string[]>();
  let currentClass = "";

  for (const rawLine of lines) {
    const line = rawLine.trim();
    if (!line || line.startsWith("%%") || /^classDiagram\b/i.test(line)) continue;

    const blockStart = line.match(/^class\s+([a-zA-Z0-9_]+)\s*\{/i);
    if (blockStart) {
      currentClass = blockStart[1];
      if (!classes.has(currentClass)) classes.set(currentClass, []);
      continue;
    }

    if (line === "}") {
      currentClass = "";
      continue;
    }

    if (currentClass) {
      classes.get(currentClass)?.push(line);
      continue;
    }

    const singleClass = line.match(/^class\s+([a-zA-Z0-9_]+)$/i);
    if (singleClass) {
      if (!classes.has(singleClass[1])) classes.set(singleClass[1], []);
    }
  }

  return Array.from(classes.entries()).map(([name, members]) => ({
    name,
    members,
  }));
}

// ==========================================
// MERMAID DIAGRAM CARD COMPONENT
// ==========================================

export function MermaidDiagramCard({ code }: { code: string }) {
  const [activeTab, setActiveTab] = useState<"diagram" | "code">("diagram");
  const [copied, setCopied] = useState(false);

  const diagramType = useMemo(() => detectDiagramType(code), [code]);

  const seqData = useMemo(() => {
    if (diagramType === "sequence") return parseSequenceDiagram(code);
    return null;
  }, [diagramType, code]);

  const flowData = useMemo(() => {
    if (diagramType === "flowchart") return parseFlowchart(code);
    return null;
  }, [diagramType, code]);

  const classData = useMemo(() => {
    if (diagramType === "class") return parseClassDiagram(code);
    return null;
  }, [diagramType, code]);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(true);
      setTimeout(() => setCopied(false), 1800);
    } catch {
      // Ignored in restricted environments
    }
  };

  const nodeMap = useMemo(() => {
    const map = new Map<string, FlowNode>();
    if (flowData) {
      for (const n of flowData.nodes) {
        map.set(n.id, n);
      }
    }
    return map;
  }, [flowData]);

  const participantMap = useMemo(() => {
    const map = new Map<string, SequenceParticipant>();
    if (seqData) {
      for (const p of seqData.participants) {
        map.set(p.id, p);
      }
    }
    return map;
  }, [seqData]);

  // Determine canRenderVisual
  const canRenderVisual = useMemo(() => {
    if (diagramType === "sequence") {
      return (seqData?.items.length ?? 0) > 0 || (seqData?.participants.length ?? 0) > 0;
    }
    if (diagramType === "flowchart") {
      return (flowData?.nodes.length ?? 0) >= 2 || (flowData?.edges.length ?? 0) >= 1;
    }
    if (diagramType === "class") {
      return (classData?.length ?? 0) > 0;
    }
    return false;
  }, [diagramType, seqData, flowData, classData]);

  const diagramTitle =
    diagramType === "sequence"
      ? "Sơ đồ Trình tự (Sequence Diagram)"
      : diagramType === "flowchart"
        ? `Sơ đồ Luồng (${flowData?.direction === "LR" ? "Ngang · LR" : "Dọc · TD"})`
        : diagramType === "class"
          ? "Sơ đồ Cấu trúc Lớp (Class Diagram)"
          : diagramType === "er"
            ? "Sơ đồ Thực thể (ER Diagram)"
            : "Sơ đồ Mermaid";

  return (
    <div className="my-5 rounded-2xl border border-indigo-200/90 dark:border-indigo-900/60 bg-white dark:bg-slate-900/95 overflow-hidden shadow-xs font-sans max-w-full">
      {/* Header bar */}
      <div className="px-3.5 sm:px-4 py-2.5 bg-slate-50 dark:bg-slate-900 border-b border-indigo-100 dark:border-slate-800 flex items-center justify-between gap-3 flex-wrap text-xs">
        <div className="flex items-center gap-2 min-w-0">
          <div className="w-6 h-6 rounded-lg bg-indigo-100 dark:bg-indigo-950/70 text-indigo-600 dark:text-indigo-400 flex items-center justify-center shrink-0">
            {diagramType === "sequence" ? (
              <Layers className="w-3.5 h-3.5" />
            ) : diagramType === "flowchart" ? (
              <Workflow className="w-3.5 h-3.5" />
            ) : (
              <GitFork className="w-3.5 h-3.5" />
            )}
          </div>
          <div className="min-w-0">
            <div className="font-semibold text-slate-900 dark:text-slate-100 truncate">
              {diagramTitle}
            </div>
            <div className="text-[10px] text-slate-500 dark:text-slate-400">
              {diagramType === "sequence" && seqData
                ? `${seqData.participants.length} đối tượng · ${seqData.items.filter((i) => i.kind === "message").length} tương tác`
                : diagramType === "flowchart" && flowData
                  ? `${flowData.nodes.length} nút · ${flowData.edges.length} liên kết bước`
                  : "Mã sơ đồ hệ thống & luồng dữ liệu"}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-1.5 shrink-0">
          <div className="flex items-center bg-slate-200/80 dark:bg-slate-800 p-0.5 rounded-lg text-[11px] font-medium">
            <button
              type="button"
              onClick={() => setActiveTab("diagram")}
              className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-md transition cursor-pointer ${
                activeTab === "diagram"
                  ? "bg-white dark:bg-indigo-600 text-indigo-700 dark:text-white shadow-xs font-semibold"
                  : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200"
              }`}
            >
              <Eye className="w-3 h-3" />
              <span>Sơ đồ trực quan</span>
            </button>
            <button
              type="button"
              onClick={() => setActiveTab("code")}
              className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-md transition cursor-pointer ${
                activeTab === "code"
                  ? "bg-white dark:bg-indigo-600 text-indigo-700 dark:text-white shadow-xs font-semibold"
                  : "text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200"
              }`}
            >
              <Code2 className="w-3 h-3" />
              <span>Mã nguồn</span>
            </button>
          </div>

          <button
            type="button"
            onClick={handleCopy}
            className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-slate-200/80 hover:bg-slate-300/80 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 text-[11px] font-medium transition cursor-pointer"
            title="Sao chép cú pháp Mermaid"
          >
            {copied ? (
              <>
                <Check className="w-3 h-3 text-emerald-600 dark:text-emerald-400" />
                <span className="text-emerald-600 dark:text-emerald-400 font-semibold">
                  Đã chép
                </span>
              </>
            ) : (
              <>
                <Copy className="w-3 h-3" />
                <span>Sao chép</span>
              </>
            )}
          </button>
        </div>
      </div>

      {/* Main Content Body */}
      {activeTab === "diagram" && canRenderVisual ? (
        <div className="p-3.5 sm:p-5 space-y-4">
          {/* 1. Sequence Diagram Visual Mode */}
          {diagramType === "sequence" && seqData && (
            <div className="space-y-4">
              {/* Participants Header Pills */}
              {seqData.participants.length > 0 && (
                <div>
                  <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">
                    Các tác nhân & hệ thống tham gia
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {seqData.participants.map((p) => (
                      <div
                        key={p.id}
                        className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-xs font-semibold shadow-2xs ${
                          p.isActor
                            ? "bg-amber-50/80 dark:bg-amber-950/40 border-amber-300 dark:border-amber-700/70 text-amber-900 dark:text-amber-200"
                            : "bg-indigo-50/70 dark:bg-indigo-950/40 border-indigo-200 dark:border-indigo-800/70 text-indigo-900 dark:text-indigo-200"
                        }`}
                      >
                        {p.isActor ? (
                          <Activity className="w-3.5 h-3.5 text-amber-600 dark:text-amber-400 shrink-0" />
                        ) : (
                          <Layers className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400 shrink-0" />
                        )}
                        <span>{p.label}</span>
                        {p.label !== p.id && (
                          <span className="text-[10px] opacity-60 font-mono font-normal">
                            ({p.id})
                          </span>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Interactions Timeline */}
              <div className="space-y-2.5 pt-1">
                <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1.5">
                  Trình tự các bước thực hiện
                </div>
                {seqData.items.map((item, idx) => {
                  if (item.kind === "note") {
                    return (
                      <div
                        key={`note-${idx}`}
                        className="p-3 rounded-xl bg-amber-500/10 dark:bg-amber-950/30 border border-amber-300/80 dark:border-amber-700/60 text-amber-950 dark:text-amber-200 text-xs sm:text-[13px] font-medium flex items-start gap-2.5 shadow-2xs"
                      >
                        <Sparkles className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
                        <div className="space-y-0.5 flex-1 min-w-0">
                          <div className="font-semibold">{item.text}</div>
                          {item.targets.length > 0 && (
                            <div className="text-[10px] text-amber-800/80 dark:text-amber-300/80">
                              Phạm vi liên quan:{" "}
                              {item.targets
                                .map((t) => participantMap.get(t)?.label || t)
                                .join(", ")}
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  }

                  const fromLabel = participantMap.get(item.from)?.label || item.from;
                  const toLabel = participantMap.get(item.to)?.label || item.to;

                  return (
                    <div
                      key={`msg-${idx}`}
                      className="p-3 rounded-xl border border-slate-200/90 dark:border-slate-800 bg-slate-50/70 dark:bg-slate-800/40 hover:bg-slate-100/60 dark:hover:bg-slate-800/70 transition shadow-2xs flex items-start gap-3"
                    >
                      <div className="w-5 h-5 rounded-full bg-indigo-100 dark:bg-indigo-900/60 text-indigo-700 dark:text-indigo-300 font-bold text-[11px] flex items-center justify-center shrink-0 mt-0.5">
                        {idx + 1}
                      </div>
                      <div className="flex-1 min-w-0 space-y-1.5">
                        <div className="flex items-center gap-1.5 flex-wrap text-xs">
                          <span className="font-semibold text-slate-800 dark:text-slate-200 px-2 py-0.5 rounded-md bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-2xs">
                            {fromLabel}
                          </span>
                          <span
                            className={`inline-flex items-center gap-1 text-[11px] font-medium px-1.5 py-0.5 rounded ${
                              item.isDotted
                                ? "text-sky-600 dark:text-sky-400 bg-sky-50 dark:bg-sky-950/40"
                                : "text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/40"
                            }`}
                          >
                            <ArrowRight className="w-3 h-3" />
                            <span>{item.isDotted ? "Phản hồi" : "Gửi yêu cầu"}</span>
                          </span>
                          <span className="font-semibold text-slate-800 dark:text-slate-200 px-2 py-0.5 rounded-md bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-2xs">
                            {toLabel}
                          </span>
                        </div>
                        <div className="text-xs sm:text-[13px] text-slate-900 dark:text-slate-100 font-medium leading-relaxed bg-white dark:bg-slate-900/80 p-2.5 rounded-lg border border-slate-200/80 dark:border-slate-700/80">
                          {item.message}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* 2. Flowchart Visual Mode */}
          {diagramType === "flowchart" && flowData && (
            <div className="space-y-4">
              {/* Nodes Overview */}
              <div>
                <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">
                  Các nút thành phần ({flowData.nodes.length} nút)
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2.5">
                  {flowData.nodes.map((node) => {
                    const isDecision = node.shape === "decision";
                    const isTerminal = node.shape === "terminal";
                    const isDb = node.shape === "database";
                    const isSub = node.shape === "subroutine";

                    return (
                      <div
                        key={node.id}
                        className={`p-3 rounded-xl border transition shadow-2xs flex items-start gap-2.5 ${
                          isDecision
                            ? "bg-amber-50/90 dark:bg-amber-950/40 border-amber-300 dark:border-amber-700/80 text-amber-950 dark:text-amber-100"
                            : isTerminal
                              ? "bg-emerald-50/90 dark:bg-emerald-950/40 border-emerald-300 dark:border-emerald-700/80 text-emerald-950 dark:text-emerald-100"
                              : isDb
                                ? "bg-purple-50/90 dark:bg-purple-950/40 border-purple-300 dark:border-purple-700/80 text-purple-950 dark:text-purple-100"
                                : isSub
                                  ? "bg-sky-50/90 dark:bg-sky-950/40 border-sky-300 dark:border-sky-700/80 text-sky-950 dark:text-sky-100"
                                  : "bg-white dark:bg-slate-800/90 border-slate-200 dark:border-slate-700 text-slate-900 dark:text-slate-100"
                        }`}
                      >
                        <div className="shrink-0 mt-0.5">
                          {isDecision ? (
                            <HelpCircle className="w-4 h-4 text-amber-600 dark:text-amber-400" />
                          ) : isTerminal ? (
                            <PlayCircle className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
                          ) : isDb ? (
                            <Database className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                          ) : (
                            <Workflow className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="text-xs sm:text-[13px] font-semibold leading-snug">
                            {node.label}
                          </div>
                          <div className="text-[10px] opacity-70 mt-0.5 font-mono">
                            {isDecision
                              ? "Phân nhánh điều kiện"
                              : isTerminal
                                ? "Điểm đầu / cuối"
                                : isDb
                                  ? "Cơ sở dữ liệu"
                                  : `Mã nút: ${node.id}`}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Edge Connections List */}
              {flowData.edges.length > 0 && (
                <div className="pt-2">
                  <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">
                    Các bước chuyển tiếp & Luồng logic ({flowData.edges.length} liên kết)
                  </div>
                  <div className="space-y-2">
                    {flowData.edges.map((edge, idx) => {
                      const fromNode = nodeMap.get(edge.from);
                      const toNode = nodeMap.get(edge.to);
                      const fromText = fromNode ? fromNode.label : edge.from;
                      const toText = toNode ? toNode.label : edge.to;

                      return (
                        <div
                          key={`edge-${idx}`}
                          className="p-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/80 dark:bg-slate-800/40 text-xs flex items-center justify-between gap-2.5 flex-wrap shadow-2xs"
                        >
                          <div className="flex items-center gap-2 min-w-0 flex-1">
                            <span className="font-semibold text-slate-800 dark:text-slate-200 px-2 py-1 rounded-md bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-2xs truncate max-w-[220px]">
                              {fromText}
                            </span>

                            <div className="inline-flex items-center gap-1.5 shrink-0 px-2 py-0.5 rounded-md bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 font-medium text-[11px]">
                              {edge.isBidirectional ? (
                                <ArrowLeftRight className="w-3.5 h-3.5 text-indigo-500" />
                              ) : (
                                <ArrowRight className="w-3.5 h-3.5 text-indigo-500" />
                              )}
                              {edge.label ? (
                                <span className="font-bold text-amber-700 dark:text-amber-300">
                                  {edge.label}
                                </span>
                              ) : edge.isDotted ? (
                                <span className="text-[10px] opacity-75">Nối phụ</span>
                              ) : edge.isThick ? (
                                <span className="text-[10px] opacity-75">Liên kết chính</span>
                              ) : null}
                            </div>

                            <span className="font-semibold text-slate-800 dark:text-slate-200 px-2 py-1 rounded-md bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-2xs truncate max-w-[220px]">
                              {toText}
                            </span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* 3. Class Diagram Visual Mode */}
          {diagramType === "class" && classData && classData.length > 0 && (
            <div className="space-y-3">
              <div className="text-[11px] font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-2">
                Các cấu trúc Class & Interface
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {classData.map((cls, idx) => (
                  <div
                    key={`cls-${idx}`}
                    className="p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-800 bg-white dark:bg-slate-800 shadow-2xs space-y-2"
                  >
                    <div className="flex items-center gap-2 font-bold text-sm text-indigo-700 dark:text-indigo-300 border-b border-indigo-100 dark:border-slate-700 pb-1.5">
                      <Layers className="w-4 h-4 text-indigo-500" />
                      <span>{cls.name}</span>
                    </div>
                    {cls.members.length > 0 ? (
                      <ul className="space-y-1 text-xs font-mono text-slate-700 dark:text-slate-300">
                        {cls.members.map((m, mIdx) => (
                          <li key={mIdx} className="truncate">
                            {m}
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="text-xs text-slate-400 italic">Không có thành viên công khai.</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      ) : activeTab === "diagram" ? (
        /* Fallback when visual parse has 0 nodes/edges or other unsupported diagram */
        <div className="p-4 sm:p-5 space-y-3">
          <div className="p-3.5 rounded-xl bg-indigo-50/70 dark:bg-indigo-950/30 border border-indigo-200 dark:border-indigo-800/60 text-xs text-slate-700 dark:text-slate-300 space-y-1.5">
            <div className="font-semibold text-indigo-900 dark:text-indigo-200 flex items-center gap-1.5">
              <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
              <span>Sơ đồ cấu trúc Mermaid</span>
            </div>
            <p className="leading-relaxed">
              Biểu đồ này có định dạng đặc thù. Bạn có thể xem ngay mã nguồn Mermaid chuẩn bên dưới
              hoặc chuyển tab &ldquo;Mã nguồn&rdquo; để sao chép cú pháp.
            </p>
          </div>
          <pre className="p-3.5 overflow-x-auto max-w-full text-xs font-mono rounded-xl bg-slate-950 text-slate-100 leading-relaxed border border-slate-800">
            <code>{code}</code>
          </pre>
        </div>
      ) : (
        /* Source Code Tab */
        <pre className="p-4 overflow-x-auto max-w-full text-[13px] leading-relaxed font-mono bg-slate-950 text-slate-100 border-t border-slate-800">
          <code>{highlightCode(code, "mermaid")}</code>
        </pre>
      )}
    </div>
  );
}

// ==========================================
// SYNTAX HIGHLIGHTING TOKENIZER
// ==========================================

const JS_KEYWORDS = new Set([
  "const", "let", "var", "function", "return", "if", "else", "for", "while", "do",
  "switch", "case", "break", "continue", "new", "this", "class", "extends", "import",
  "export", "from", "as", "default", "async", "await", "yield", "try", "catch",
  "finally", "throw", "typeof", "instanceof", "in", "of", "void", "delete", "interface",
  "type", "enum", "implements", "public", "private", "protected", "readonly", "static",
  "abstract", "true", "false", "null", "undefined"
]);

const CS_KEYWORDS = new Set([
  "using", "namespace", "class", "interface", "record", "struct", "enum", "public",
  "private", "protected", "internal", "static", "readonly", "async", "await", "var",
  "new", "return", "if", "else", "for", "foreach", "in", "while", "do", "switch",
  "case", "break", "continue", "try", "catch", "finally", "throw", "get", "set",
  "init", "override", "virtual", "abstract", "base", "this", "true", "false", "null",
  "typeof", "is", "as", "void", "int", "string", "bool", "double", "float", "decimal",
  "object", "long", "short", "byte", "char", "uint", "ulong", "Task"
]);

const PY_KEYWORDS = new Set([
  "def", "class", "return", "if", "elif", "else", "for", "while", "break", "continue",
  "try", "except", "finally", "raise", "import", "from", "as", "with", "async", "await",
  "yield", "lambda", "pass", "global", "nonlocal", "assert", "True", "False", "None",
  "and", "or", "not", "is", "in", "self"
]);

const SQL_KEYWORDS = new Set([
  "select", "from", "where", "insert", "into", "update", "delete", "join", "inner",
  "left", "right", "full", "outer", "cross", "on", "group", "by", "having", "order",
  "asc", "desc", "limit", "offset", "and", "or", "not", "in", "between", "like", "is",
  "null", "as", "create", "table", "alter", "drop", "index", "primary", "key", "foreign",
  "references", "constraint", "union", "all", "view", "trigger", "procedure", "function",
  "begin", "end", "transaction", "commit", "rollback", "case", "when", "then", "else",
  "exists", "count", "sum", "avg", "min", "max", "distinct", "values", "set"
]);

const SH_KEYWORDS = new Set([
  "if", "then", "else", "elif", "fi", "for", "while", "do", "done", "in", "case",
  "esac", "function", "return", "exit", "echo", "cd", "ls", "mkdir", "rm", "cp",
  "mv", "grep", "cat", "export", "source", "sudo", "curl", "wget", "npm", "npx",
  "node", "dotnet", "git", "docker"
]);

const DOCKER_KEYWORDS = new Set([
  "from", "run", "cmd", "label", "expose", "env", "add", "copy", "entrypoint",
  "volume", "user", "workdir", "arg", "onbuild", "stopsignal", "healthcheck", "shell"
]);

export function highlightCode(code: string, rawLang: string): React.ReactNode {
  if (!code) return null;
  const lang = (rawLang || "").toLowerCase().trim();

  const isJsTs = /^(js|javascript|ts|typescript|jsx|tsx)$/.test(lang);
  const isCs = /^(cs|csharp|dotnet)$/.test(lang);
  const isPy = /^(python|py)$/.test(lang);
  const isSql = /^(sql|pgsql|mysql|sqlite)$/.test(lang);
  const isHtml = /^(html|xml|svg)$/.test(lang);
  const isCss = /^(css|scss|less)$/.test(lang);
  const isSh = /^(bash|sh|powershell|shell|zsh)$/.test(lang);
  const isJson = lang === "json";
  const isYaml = /^(yaml|yml)$/.test(lang);
  const isDocker = /^(dockerfile|docker)$/.test(lang);

  if (
    !isJsTs && !isCs && !isPy && !isSql && !isHtml && !isCss && !isSh && !isJson && !isYaml && !isDocker
  ) {
    return code;
  }

  let tokenRegex: RegExp;

  if (isHtml) {
    tokenRegex = /(<!--[\s\S]*?-->)|("(\\.|[^"\\])*"|'(\\.|[^'\\])*')|(<\/?[a-zA-Z0-9_-]+)|([a-zA-Z0-9_-]+)(?=\s*=)|(\b\d+\b)|([^\s<"'>]+)/g;
  } else if (isPy) {
    tokenRegex = /(#.*$|"""[\s\S]*?"""|'''[\s\S]*?''')|("(\\.|[^"\\])*"|'(\\.|[^'\\])*')|(\b(?:0x[0-9a-fA-F]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)\b)|(\b[a-zA-Z_][a-zA-Z0-9_]*\b)|([^\s\w]+)/gm;
  } else if (isSh || isYaml || isDocker) {
    tokenRegex = /(#.*$)|("(\\.|[^"\\])*"|'(\\.|[^'\\])*')|(\b(?:0x[0-9a-fA-F]+|\d+(?:\.\d+)?)\b)|(\b[a-zA-Z_][a-zA-Z0-9_-]*\b)|([^\s\w]+)/gm;
  } else if (isSql) {
    tokenRegex = /(--.*$|\/\*[\s\S]*?\*\/)|('(\\.|[^'\\])*'|"(\\.|[^"\\])*")|(\b\d+(?:\.\d+)?\b)|(\b[a-zA-Z_][a-zA-Z0-9_]*\b)|([^\s\w]+)/gm;
  } else if (isCss) {
    tokenRegex = /(\/\*[\s\S]*?\*\/)|("(\\.|[^"\\])*"|'(\\.|[^'\\])*')|(#(?:[0-9a-fA-F]{3,8})\b|\b\d+(?:px|em|rem|%|vh|vw|ms|s)?\b)|(\b[a-zA-Z_-][a-zA-Z0-9_-]*\b)|([^\s\w]+)/gm;
  } else {
    // JS, TS, CS, JSON
    tokenRegex = /(\/\/.*$|\/\*[\s\S]*?\*\/)|("(\\.|[^"\\])*"|'(\\.|[^'\\])*'|`(\\.|[^`\\])*`)|(\b(?:0x[0-9a-fA-F]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)\b)|(<\/?[a-zA-Z0-9_-]+)|(\b[a-zA-Z_$][a-zA-Z0-9_$]*\b)|([^\s\w$]+)/gm;
  }

  const nodes: React.ReactNode[] = [];
  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = tokenRegex.exec(code)) !== null) {
    if (match.index > lastIndex) {
      nodes.push(code.slice(lastIndex, match.index));
    }
    lastIndex = tokenRegex.lastIndex;

    const [full] = match;
    const key = `t-${match.index}`;

    if (isHtml) {
      if (match[1]) {
        nodes.push(<span key={key} className="text-slate-400 dark:text-slate-500 italic">{full}</span>);
      } else if (match[2]) {
        nodes.push(<span key={key} className="text-emerald-400 dark:text-emerald-300">{full}</span>);
      } else if (match[3]) {
        nodes.push(<span key={key} className="text-rose-400 font-semibold">{full}</span>);
      } else if (match[4]) {
        nodes.push(<span key={key} className="text-yellow-300">{full}</span>);
      } else if (match[5]) {
        nodes.push(<span key={key} className="text-amber-300 dark:text-orange-300">{full}</span>);
      } else {
        nodes.push(full);
      }
      continue;
    }

    if (match[1]) {
      nodes.push(<span key={key} className="text-slate-400 dark:text-slate-500 italic">{full}</span>);
      continue;
    }

    if (match[2]) {
      if (isJson && full.endsWith(`":`)) {
        nodes.push(<span key={key} className="text-sky-300 font-medium">{full}</span>);
      } else {
        nodes.push(<span key={key} className="text-emerald-400 dark:text-emerald-300">{full}</span>);
      }
      continue;
    }

    if (match[3]) {
      nodes.push(<span key={key} className="text-amber-300 dark:text-orange-300">{full}</span>);
      continue;
    }

    if (match[4] && match[4].startsWith("<")) {
      nodes.push(<span key={key} className="text-rose-400 font-semibold">{full}</span>);
      continue;
    }

    const word = match[5] || match[4] || full;
    const lowerWord = word.toLowerCase();

    let isKw = false;
    if (isJsTs && JS_KEYWORDS.has(word)) isKw = true;
    else if (isCs && CS_KEYWORDS.has(word)) isKw = true;
    else if (isPy && PY_KEYWORDS.has(word)) isKw = true;
    else if (isSql && SQL_KEYWORDS.has(lowerWord)) isKw = true;
    else if (isSh && SH_KEYWORDS.has(word)) isKw = true;
    else if (isDocker && DOCKER_KEYWORDS.has(lowerWord)) isKw = true;

    if (isKw) {
      nodes.push(<span key={key} className="text-pink-400 dark:text-purple-400 font-medium">{full}</span>);
      continue;
    }

    if (/^[A-Z][a-zA-Z0-9_]*$/.test(word) && !isSql) {
      nodes.push(<span key={key} className="text-sky-300 dark:text-cyan-300">{full}</span>);
      continue;
    }

    nodes.push(full);
  }

  if (lastIndex < code.length) {
    nodes.push(code.slice(lastIndex));
  }

  return nodes;
}

// ==========================================
// MAIN CODEBLOCK CARD EXPORT
// ==========================================

export function CodeBlockCard({
  lang,
  code,
  onOpenPractice,
}: CodeBlockCardProps) {
  const [copied, setCopied] = useState(false);

  const normalizedLang = (lang || "").toLowerCase();

  // Special visual handler for Mermaid diagrams
  if (normalizedLang === "mermaid") {
    return <MermaidDiagramCard code={code} />;
  }

  // Interactive practice block handler
  if (normalizedLang.startsWith("practice-")) {
    let parsedTitle = "Bài tập thực hành tương tác";
    try {
      const parsed = JSON.parse(code) as { title?: string; topic?: string };
      if (parsed.title || parsed.topic) {
        parsedTitle = parsed.title || parsed.topic || parsedTitle;
      }
    } catch {
      // Ignore parse error for preview card
    }
    const kindLabel =
      normalizedLang === "practice-finance"
        ? "Biểu đồ Nến & Quản trị Rủi ro"
        : normalizedLang === "practice-english"
          ? "Luyện Nghe & Nói Tiếng Anh"
          : normalizedLang === "practice-code"
            ? "Bàn thực hành Code"
            : "Tình huống thực hành";

    return (
      <div className="my-4 rounded-xl border border-indigo-200 dark:border-indigo-800/70 bg-indigo-50/50 dark:bg-indigo-950/30 p-4 font-sans">
        <div className="text-[11px] font-semibold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
          {kindLabel}
        </div>
        <div className="text-sm font-semibold text-slate-900 dark:text-slate-100 mt-0.5">
          {parsedTitle}
        </div>
        <div className="mt-3 flex items-center justify-between gap-3 flex-wrap">
          <p className="text-xs text-slate-600 dark:text-slate-400">
            Bài tập thực hành tương tác chuyên sâu cho phần kiến thức này.
          </p>
          {onOpenPractice && (
            <button
              type="button"
              onClick={onOpenPractice}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white font-semibold text-xs shadow-xs transition-colors cursor-pointer"
            >
              <span>Bắt đầu bài tập ngay ↓</span>
            </button>
          )}
        </div>
      </div>
    );
  }

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(true);
      setTimeout(() => setCopied(false), 1800);
    } catch {
      // Clipboard blocked in headless context; ignore gracefully
    }
  };

  return (
    <div className="my-4 rounded-[12px] border border-slate-800 bg-slate-950 text-slate-100 overflow-hidden shadow-xs font-sans max-w-full">
      <div className="flex items-center justify-between px-3.5 py-2 bg-slate-900 border-b border-slate-800 text-[11px]">
        <span className="font-mono font-bold uppercase tracking-wider text-indigo-300">
          {lang || "code"}
        </span>
        <button
          type="button"
          onClick={handleCopy}
          className="inline-flex items-center gap-1 px-2.5 py-1 rounded-md bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold transition cursor-pointer"
        >
          {copied ? (
            <>
              <Check className="w-3 h-3 text-emerald-400" />
              <span className="text-emerald-300">Đã sao chép</span>
            </>
          ) : (
            <>
              <Copy className="w-3 h-3" />
              <span>Sao chép</span>
            </>
          )}
        </button>
      </div>
      <pre className="p-4 overflow-x-auto max-w-full text-[13px] leading-relaxed font-mono text-slate-100">
        <code>{highlightCode(code, lang)}</code>
      </pre>
    </div>
  );
}
