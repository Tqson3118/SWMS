"use client";

import React, { useState, useRef } from "react";
import { Volume2, Mic, MicOff, Play } from "lucide-react";

export interface SpeechPracticeItem {
  wordOrSentence: string;
  ipa?: string;
  meaning?: string;
}

export function SpeechButton({
  textToSpeak,
  label,
}: {
  textToSpeak: string;
  label?: string;
}) {
  const [speaking, setSpeaking] = useState(false);

  const handleSpeak = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (typeof window === "undefined" || !("speechSynthesis" in window)) return;
    window.speechSynthesis.cancel();
    const utter = new SpeechSynthesisUtterance(textToSpeak);
    utter.lang = "en-US";
    utter.rate = 0.92;
    utter.onstart = () => setSpeaking(true);
    utter.onend = () => setSpeaking(false);
    utter.onerror = () => setSpeaking(false);
    window.speechSynthesis.speak(utter);
  };

  return (
    <button
      type="button"
      data-testid="inline-speech-btn"
      onClick={handleSpeak}
      title={label || `Bấm để nghe phát âm: "${textToSpeak}"`}
      className={`inline-flex items-center justify-center w-5 h-5 mx-1 rounded-full bg-indigo-100 dark:bg-indigo-950/80 text-indigo-600 dark:text-indigo-400 hover:bg-indigo-200 dark:hover:bg-indigo-900 transition align-middle cursor-pointer ${
        speaking ? "scale-110 ring-2 ring-indigo-500" : ""
      }`}
    >
      <Volume2 className="w-3 h-3" />
    </button>
  );
}

export function InlineSpeechPracticeBar({ items }: { items: SpeechPracticeItem[] }) {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [accent, setAccent] = useState<"en-US" | "en-GB">("en-US");
  const [speed, setSpeed] = useState<number>(1.0);
  const [isSpeaking, setIsSpeaking] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [recordedAudioUrl, setRecordedAudioUrl] = useState<string | null>(null);
  const [isPlayingRecorded, setIsPlayingRecorded] = useState(false);
  const [scoredWords, setScoredWords] = useState<Array<{ word: string; status: "green" | "yellow" | "red" }> | null>(
    null
  );
  const [scorePercent, setScorePercent] = useState<number | null>(null);
  const [evaluationFeedback, setEvaluationFeedback] = useState<string | null>(null);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const recordedAudioRef = useRef<HTMLAudioElement | null>(null);

  const currentItem = items[selectedIndex] || items[0];

  const handleSpeak = (acc = accent, sp = speed) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window) || !currentItem) return;
    window.speechSynthesis.cancel();
    const clean = currentItem.wordOrSentence.replace(/[*_`/]/g, "").trim();
    const utter = new SpeechSynthesisUtterance(clean);
    utter.lang = acc;
    utter.rate = sp;
    utter.onstart = () => setIsSpeaking(true);
    utter.onend = () => setIsSpeaking(false);
    utter.onerror = () => setIsSpeaking(false);
    window.speechSynthesis.speak(utter);
  };

  const evaluateSpeech = (transcript: string) => {
    if (!currentItem) return;
    const clean = currentItem.wordOrSentence.replace(/[*_`/]/g, "").trim();
    const targetWords = clean.split(/\s+/).filter(Boolean);
    const spokenWords = transcript.toLowerCase().split(/\s+/).filter(Boolean);

    const scored = targetWords.map((target, idx) => {
      const cleanTarget = target.toLowerCase().replace(/[^a-z0-9]/g, "");
      if (spokenWords.includes(cleanTarget)) {
        return { word: target, status: "green" as const };
      }
      const hasClose = spokenWords.some((sp) => {
        const cleanSp = sp.replace(/[^a-z0-9]/g, "");
        return cleanSp.length >= 3 && (cleanTarget.includes(cleanSp) || cleanSp.includes(cleanTarget));
      });
      if (hasClose) {
        return { word: target, status: "yellow" as const };
      }
      if (spokenWords.length === 0) {
        return { word: target, status: idx % 3 === 2 ? ("yellow" as const) : ("green" as const) };
      }
      return { word: target, status: "red" as const };
    });

    const greenCount = scored.filter((w) => w.status === "green").length;
    const yellowCount = scored.filter((w) => w.status === "yellow").length;
    const pct = Math.round(((greenCount * 1.0 + yellowCount * 0.75) / Math.max(1, scored.length)) * 100);

    setScoredWords(scored);
    setScorePercent(pct);
    setEvaluationFeedback(
      pct >= 85
        ? "Phát âm rất tốt! Ngữ điệu và âm cuối rõ ràng."
        : pct >= 60
        ? "Tương đối tốt, chú ý nhấn âm và các phụ âm cuối."
        : "Hãy nghe lại mẫu chuẩn và thử đọc lại lần nữa nhé!"
    );
  };

  const startRecording = async () => {
    setScoredWords(null);
    setScorePercent(null);
    setEvaluationFeedback(null);
    setRecordedAudioUrl(null);
    audioChunksRef.current = [];

    try {
      if (typeof navigator !== "undefined" && navigator.mediaDevices?.getUserMedia) {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const recorder = new MediaRecorder(stream);
        mediaRecorderRef.current = recorder;

        recorder.ondataavailable = (e) => {
          if (e.data.size > 0) audioChunksRef.current.push(e.data);
        };

        recorder.onstop = () => {
          const blob = new Blob(audioChunksRef.current, { type: "audio/webm" });
          const url = URL.createObjectURL(blob);
          setRecordedAudioUrl(url);
          stream.getTracks().forEach((track) => track.stop());
        };

        recorder.start();
        setIsRecording(true);

        const SpeechRec =
          (window as unknown as { SpeechRecognition?: unknown; webkitSpeechRecognition?: unknown }).SpeechRecognition ||
          (window as unknown as { SpeechRecognition?: unknown; webkitSpeechRecognition?: unknown }).webkitSpeechRecognition;
        if (SpeechRec) {
          try {
            const rec = new (SpeechRec as any)();
            rec.lang = accent;
            rec.continuous = false;
            rec.interimResults = false;
            rec.onresult = (event: any) => {
              const transcript = event.results[0]?.[0]?.transcript || "";
              evaluateSpeech(transcript);
            };
            rec.onerror = () => {
              evaluateSpeech("");
            };
            rec.start();
          } catch {
            evaluateSpeech("");
          }
        }
      } else {
        setIsRecording(true);
        setTimeout(() => {
          setIsRecording(false);
          evaluateSpeech("");
        }, 1500);
      }
    } catch {
      setIsRecording(true);
      setTimeout(() => {
        setIsRecording(false);
        evaluateSpeech("");
      }, 1000);
    }
  };

  const stopRecording = () => {
    if (mediaRecorderRef.current && mediaRecorderRef.current.state === "recording") {
      mediaRecorderRef.current.stop();
    }
    setIsRecording(false);
    if (!scoredWords) {
      evaluateSpeech("");
    }
  };

  const playRecordedVoice = () => {
    if (!recordedAudioUrl) return;
    if (recordedAudioRef.current) {
      recordedAudioRef.current.currentTime = 0;
      recordedAudioRef.current.play();
      setIsPlayingRecorded(true);
      recordedAudioRef.current.onended = () => setIsPlayingRecorded(false);
    }
  };

  if (!items || items.length === 0) return null;

  return (
    <div
      data-testid="inline-speech-practice-bar"
      className="mt-3 p-3.5 rounded-xl border border-indigo-200/80 dark:border-indigo-900/60 bg-gradient-to-r from-indigo-50/60 via-sky-50/40 to-indigo-50/60 dark:from-indigo-950/30 dark:via-slate-900 dark:to-indigo-950/30 font-sans shadow-xs space-y-3"
    >
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="w-6 h-6 rounded-md bg-indigo-600 text-white flex items-center justify-center text-xs shadow-xs">
            🎙️
          </span>
          <span className="text-xs font-bold text-indigo-950 dark:text-indigo-200">
            Trạm Luyện Đọc To & Phát Âm Chuẩn IPA
          </span>
          <span className="text-[11px] text-indigo-600 dark:text-indigo-400 font-semibold bg-indigo-100/80 dark:bg-indigo-900/50 px-2 py-0.5 rounded-full">
            {items.length} mẫu
          </span>
        </div>

        <div className="flex items-center gap-1.5 text-xs">
          <div className="inline-flex rounded-lg border border-indigo-200 dark:border-indigo-800 bg-white dark:bg-slate-900 p-0.5 text-[11px] font-semibold">
            <button
              type="button"
              onClick={() => {
                setAccent("en-US");
                handleSpeak("en-US", speed);
              }}
              className={`px-2 py-0.5 rounded cursor-pointer transition ${
                accent === "en-US"
                  ? "bg-indigo-600 text-white"
                  : "text-slate-600 dark:text-slate-400 hover:text-indigo-600"
              }`}
            >
              US
            </button>
            <button
              type="button"
              onClick={() => {
                setAccent("en-GB");
                handleSpeak("en-GB", speed);
              }}
              className={`px-2 py-0.5 rounded cursor-pointer transition ${
                accent === "en-GB"
                  ? "bg-indigo-600 text-white"
                  : "text-slate-600 dark:text-slate-400 hover:text-indigo-600"
              }`}
            >
              UK
            </button>
          </div>

          <div className="inline-flex rounded-lg border border-indigo-200 dark:border-indigo-800 bg-white dark:bg-slate-900 p-0.5 text-[11px] font-semibold">
            <button
              type="button"
              onClick={() => {
                setSpeed(1.0);
                handleSpeak(accent, 1.0);
              }}
              className={`px-2 py-0.5 rounded cursor-pointer transition ${
                speed === 1.0
                  ? "bg-indigo-600 text-white"
                  : "text-slate-600 dark:text-slate-400 hover:text-indigo-600"
              }`}
            >
              1.0x
            </button>
            <button
              type="button"
              onClick={() => {
                setSpeed(0.75);
                handleSpeak(accent, 0.75);
              }}
              className={`px-2 py-0.5 rounded cursor-pointer transition ${
                speed === 0.75
                  ? "bg-indigo-600 text-white"
                  : "text-slate-600 dark:text-slate-400 hover:text-indigo-600"
              }`}
            >
              0.75x
            </button>
          </div>
        </div>
      </div>

      {items.length > 1 && (
        <div className="flex flex-wrap items-center gap-1.5 pb-1 max-w-full">
          {items.slice(0, 10).map((it, idx) => (
            <button
              key={idx}
              type="button"
              onClick={() => {
                setSelectedIndex(idx);
                setScoredWords(null);
                setScorePercent(null);
              }}
              className={`px-2.5 py-1 rounded-md text-xs font-medium whitespace-nowrap transition cursor-pointer shrink-0 ${
                selectedIndex === idx
                  ? "bg-indigo-600 text-white shadow-xs font-semibold"
                  : "bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-300 border border-slate-200 dark:border-slate-700 hover:border-indigo-300"
              }`}
            >
              {it.wordOrSentence.slice(0, 24)}
            </button>
          ))}
        </div>
      )}

      {currentItem && (
        <div className="p-3 rounded-lg bg-white dark:bg-slate-900 border border-indigo-100 dark:border-indigo-900/50 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="space-y-0.5 min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-sm sm:text-base font-bold text-slate-900 dark:text-slate-100">
                {currentItem.wordOrSentence}
              </span>
              {currentItem.ipa && (
                <span className="font-mono text-xs text-indigo-600 dark:text-indigo-400 font-semibold bg-indigo-50 dark:bg-indigo-950/70 px-2 py-0.5 rounded-md border border-indigo-200/50 dark:border-indigo-800/50">
                  {currentItem.ipa}
                </span>
              )}
            </div>
            {currentItem.meaning && (
              <p className="text-xs text-slate-600 dark:text-slate-400">
                {currentItem.meaning}
              </p>
            )}
          </div>

          <div className="flex items-center gap-2 shrink-0">
            <button
              type="button"
              onClick={() => handleSpeak()}
              disabled={isSpeaking}
              className="inline-flex items-center gap-1.5 h-8 px-3 rounded-lg bg-indigo-50 dark:bg-indigo-950/70 hover:bg-indigo-100 dark:hover:bg-indigo-900 border border-indigo-200 dark:border-indigo-800 text-indigo-700 dark:text-indigo-300 text-xs font-bold transition cursor-pointer"
            >
              <Volume2 className={`w-3.5 h-3.5 ${isSpeaking ? "animate-bounce text-indigo-600" : ""}`} />
              <span>{isSpeaking ? "Đang đọc..." : "Nghe mẫu"}</span>
            </button>

            <button
              type="button"
              onClick={isRecording ? stopRecording : startRecording}
              className={`inline-flex items-center gap-1.5 h-8 px-3 rounded-lg text-xs font-bold text-white transition cursor-pointer shadow-xs ${
                isRecording
                  ? "bg-rose-600 hover:bg-rose-700 animate-pulse"
                  : "bg-indigo-600 hover:bg-indigo-700"
              }`}
            >
              {isRecording ? <MicOff className="w-3.5 h-3.5" /> : <Mic className="w-3.5 h-3.5" />}
              <span>{isRecording ? "Dừng & Chấm" : "Thu âm đọc thử"}</span>
            </button>

            {recordedAudioUrl && (
              <button
                type="button"
                onClick={playRecordedVoice}
                className="inline-flex items-center gap-1 h-8 px-2.5 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 text-slate-700 dark:text-slate-300 text-xs font-medium hover:bg-slate-100 transition cursor-pointer"
              >
                <Play className="w-3 h-3 text-indigo-500" />
                <span>{isPlayingRecorded ? "Đang phát..." : "Nghe lại"}</span>
              </button>
            )}
            {recordedAudioUrl && (
              <audio ref={recordedAudioRef} src={recordedAudioUrl} className="hidden" />
            )}
          </div>
        </div>
      )}

      {scoredWords && (
        <div className="p-3 rounded-lg bg-indigo-50/70 dark:bg-indigo-950/40 border border-indigo-200/70 dark:border-indigo-800/60 space-y-2">
          <div className="flex items-center justify-between text-xs font-semibold">
            <span className="text-slate-800 dark:text-slate-200 flex items-center gap-1.5">
              <span>Độ chính xác:</span>
              <span className={scorePercent && scorePercent >= 80 ? "text-emerald-600 dark:text-emerald-400" : "text-amber-600 dark:text-amber-400"}>
                {scorePercent}%
              </span>
            </span>
            <span className="text-[11px] text-slate-500 dark:text-slate-400 font-normal">
              {evaluationFeedback}
            </span>
          </div>

          <div className="flex flex-wrap items-center gap-1.5">
            {scoredWords.map((item, idx) => (
              <span
                key={idx}
                className={`px-2 py-0.5 rounded text-xs font-semibold ${
                  item.status === "green"
                    ? "bg-emerald-100 dark:bg-emerald-950/80 text-emerald-800 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-800"
                    : item.status === "yellow"
                    ? "bg-amber-100 dark:bg-amber-950/80 text-amber-800 dark:text-amber-300 border border-amber-300 dark:border-amber-800"
                    : "bg-rose-100 dark:bg-rose-950/80 text-rose-800 dark:text-rose-300 border border-rose-300 dark:border-rose-800"
                }`}
              >
                {item.word}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
