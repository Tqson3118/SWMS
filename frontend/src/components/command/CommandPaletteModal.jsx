"use client";
import React, { useEffect, useState } from "react";
import { useWorkspaceStore } from "@/stores/useWorkspaceStore";
import { Search, Zap, Plus, CheckCircle2, AlertCircle } from "lucide-react";
export function CommandPaletteModal({ onQuickCaptureCreated }) {
    const { commandPaletteOpen, setCommandPaletteOpen, setActiveMode, setActiveLesson, setActiveDocument, setSelectedTagFilterId } = useWorkspaceStore();
    const [query, setQuery] = useState("");
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);
    const [quickCaptureText, setQuickCaptureText] = useState("");
    const [captureSavedMsg, setCaptureSavedMsg] = useState(null);
    const [captureErrorMsg, setCaptureErrorMsg] = useState(null);
    useEffect(() => {
        const onKeyDown = (e) => {
            if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
                e.preventDefault();
                setCommandPaletteOpen(!commandPaletteOpen);
            }
            else if (e.key === "Escape" && commandPaletteOpen) {
                setCommandPaletteOpen(false);
            }
        };
        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [commandPaletteOpen, setCommandPaletteOpen]);
    useEffect(() => {
        if (!commandPaletteOpen)
            return;
        let active = true;
        setLoading(true);
        fetch(`/api/v1/workspace/search?q=${encodeURIComponent(query)}`)
            .then((r) => r.json())
            .then((data) => {
            if (active) {
                setResults(data.results || []);
                setLoading(false);
            }
        })
            .catch(() => {
            if (active)
                setLoading(false);
        });
        return () => {
            active = false;
        };
    }, [query, commandPaletteOpen]);
    if (!commandPaletteOpen)
        return null;
    const handleSelectItem = (item) => {
        if (item.type === "lesson") {
            setActiveLesson(item.id);
            setActiveMode("reading");
        }
        else if (item.type === "document") {
            setActiveDocument(item.id);
            setActiveMode("reading");
        }
        else if (item.type === "notebook") {
            setActiveMode("split");
        }
        else if (item.type === "flashcard") {
            setActiveMode("review");
        }
        else if (item.type === "tag") {
            setSelectedTagFilterId(item.id);
            setActiveMode("review");
        }
        setCommandPaletteOpen(false);
    };
    const handleQuickCaptureFlashcard = async () => {
        if (!quickCaptureText.trim())
            return;
        setCaptureErrorMsg(null);
        try {
            const resp = await fetch("/api/v1/flashcards", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    cardType: "cloze",
                    frontText: quickCaptureText.includes("{{c1::")
                        ? quickCaptureText
                        : `Ghi nhớ nhanh: {{c1::${quickCaptureText}}}`,
                    backText: "Được tạo nhanh từ Command Palette (Ctrl+K Quick Capture).",
                    tagNames: ["OS-Paging"]
                })
            });
            if (resp.ok) {
                setQuickCaptureText("");
                setCaptureSavedMsg("Đã lưu thẻ Cloze nhanh vào Hàng đợi Ôn tập FSRS!");
                onQuickCaptureCreated?.();
                setTimeout(() => setCaptureSavedMsg(null), 2500);
            }
            else {
                const err = await resp.json().catch(() => ({}));
                setCaptureErrorMsg(err.error || `Không thể lưu thẻ flashcard (Mã lỗi ${resp.status}).`);
                setTimeout(() => setCaptureErrorMsg(null), 3500);
            }
        }
        catch {
            setCaptureErrorMsg("Lỗi kết nối mạng khi lưu thẻ. Vui lòng thử lại!");
            setTimeout(() => setCaptureErrorMsg(null), 3500);
        }
    };
    return (<div data-testid="command-palette-modal" className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-xs flex items-start justify-center pt-20 px-4" onClick={() => setCommandPaletteOpen(false)}>
      <div className="w-full max-w-2xl rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xl overflow-hidden" onClick={(e) => e.stopPropagation()}>
        <div className="p-3.5 border-b border-slate-200 dark:border-slate-800 flex items-center gap-3">
          <Search className="w-4 h-4 text-slate-400 shrink-0"/>
          <input data-testid="command-palette-input" type="text" autoFocus value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Tìm bài học, PDF, ghi chú, thẻ FSRS (hỗ trợ tiếng Việt không dấu: 'bo nho ao', 'tlb')..." className="w-full bg-transparent text-sm text-slate-900 dark:text-slate-100 focus:outline-none"/>
          <button type="button" onClick={() => setCommandPaletteOpen(false)} className="px-2 py-0.5 text-xs rounded bg-slate-100 dark:bg-slate-800 text-slate-500 cursor-pointer">
            Esc
          </button>
        </div>

        {/* Quick Capture Bar */}
        <div className="px-4 py-2.5 bg-indigo-50/60 dark:bg-indigo-950/30 border-b border-slate-200 dark:border-slate-800 flex items-center gap-2">
          <span className="text-xs font-semibold text-indigo-700 dark:text-indigo-300 shrink-0 flex items-center gap-1">
            <Zap className="w-3.5 h-3.5"/>
            <span>Ghi nhanh:</span>
          </span>
          <input data-testid="quick-capture-input" type="text" value={quickCaptureText} onChange={(e) => setQuickCaptureText(e.target.value)} placeholder="Nhập ý tưởng hoặc mệnh đề cần tạo thẻ Cloze tức thì..." className="flex-1 text-xs bg-white dark:bg-slate-900 px-2.5 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 focus:outline-none"/>
          <button type="button" data-testid="quick-capture-save-btn" onClick={handleQuickCaptureFlashcard} className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold shrink-0 cursor-pointer">
            <Plus className="w-3.5 h-3.5"/>
            <span>Tạo thẻ Cloze</span>
          </button>
        </div>

        {captureSavedMsg && (<div className="px-4 py-2 bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 text-xs font-semibold flex items-center gap-1.5 animate-in fade-in">
            <CheckCircle2 className="w-3.5 h-3.5 shrink-0"/>
            <span>{captureSavedMsg}</span>
          </div>)}

        {captureErrorMsg && (<div data-testid="quick-capture-error-alert" className="px-4 py-2 bg-rose-500/15 text-rose-700 dark:text-rose-300 text-xs font-semibold flex items-center gap-1.5 animate-in fade-in">
            <AlertCircle className="w-3.5 h-3.5 shrink-0"/>
            <span>{captureErrorMsg}</span>
          </div>)}

        <div className="max-h-80 overflow-y-auto divide-y divide-slate-100 dark:divide-slate-800">
          {loading ? (<div className="p-6 text-center text-xs text-slate-500">Đang tìm kiếm...</div>) : results.length === 0 ? (<div className="p-6 text-center text-xs text-slate-500">
              Không tìm thấy kết quả phù hợp cho &ldquo;{query}&rdquo;.
            </div>) : (results.map((item) => (<button key={`${item.type}-${item.id}`} type="button" data-testid={`cmd-result-${item.type}`} onClick={() => handleSelectItem(item)} className="w-full text-left px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-800/70 transition flex items-start justify-between gap-3 cursor-pointer">
                <div className="min-w-0">
                  <div className="text-xs font-bold text-slate-900 dark:text-slate-100 truncate">
                    {item.title}
                  </div>
                  <div className="text-[11px] text-indigo-600 dark:text-indigo-400 font-medium">
                    {item.subtitle}
                  </div>
                  {item.snippet && (<div className="text-[11px] text-slate-500 dark:text-slate-400 truncate mt-0.5">
                      {item.snippet}
                    </div>)}
                </div>
                <span className="text-[10px] uppercase font-bold px-2 py-0.5 rounded bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 shrink-0">
                  {item.type}
                </span>
              </button>)))}
        </div>

        <div className="px-4 py-2 bg-slate-50 dark:bg-slate-950 border-t border-slate-200 dark:border-slate-800 flex items-center justify-between text-[11px] text-slate-500">
          <span>Mẹo: Gõ không dấu (&ldquo;bo nho ao&rdquo;, &ldquo;phan trang&rdquo;) nhờ bộ chuẩn hóa f_unaccent</span>
          <span>↵ Mở nhanh • Esc Đóng</span>
        </div>
      </div>
    </div>);
}
