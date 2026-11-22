"use client";

import React, { useState, useMemo, useCallback, useEffect } from "react";
import {
  Wallet,
  TrendingUp,
  TrendingDown,
  PieChart,
  ShieldAlert,
  AlertTriangle,
  CheckCircle2,
  Sparkles,
  BookOpen,
  Plus,
  Trash2,
  HelpCircle,
  RotateCcw,
  Target,
  ArrowRight,
  Info,
  Scale,
  CandlestickChart,
  ChevronRight,
  Flame,
  Snowflake,
  Sliders,
  DollarSign,
  Activity,
  Briefcase,
  FileText,
  BookmarkPlus,
  CheckSquare,
  Calculator,
  BarChart2,
  Layers,
  Award,
  RefreshCw,
  SlidersHorizontal,
} from "lucide-react";
import { getClientRequestHeaders } from "@/stores/useWorkspaceStore";
import { invalidateDashboardCache } from "@/lib/api-client";
import { resolvePracticeSelection } from "@/lib/practice-helpers";

export interface PracticeModuleProps {
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

export type FinancePracticeLabProps = PracticeModuleProps;

type ActiveTab = "budget" | "compound" | "debt" | "trading" | "cases" | "portfolio" | "behavioral";

// Định dạng tiền tệ VNĐ chuẩn
function formatVND(value: number): string {
  if (isNaN(value)) return "0 đ";
  return new Intl.NumberFormat("vi-VN").format(Math.round(value)) + " đ";
}

function formatCompactVND(value: number): string {
  if (isNaN(value)) return "0 đ";
  const abs = Math.abs(value);
  if (abs >= 1_000_000_000) {
    return (value / 1_000_000_000).toFixed(1).replace(/\.0$/, "") + " tỷ";
  }
  if (abs >= 1_000_000) {
    return (value / 1_000_000).toFixed(1).replace(/\.0$/, "") + " tr";
  }
  if (abs >= 1_000) {
    return (value / 1_000).toFixed(0) + " k";
  }
  return value.toLocaleString("vi-VN");
}

function stripDiacritics(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

function detectDefaultTab(courseTitle: string, lessonTitle: string, md: string): ActiveTab {
  const combined = stripDiacritics(`${courseTitle} ${lessonTitle} ${md.slice(0, 1000)}`);

  if (/\b(danh muc|portfolio|phan bo|asset allocation|rebalance)\b/.test(combined)) {
    return "portfolio";
  }
  if (/\b(fomo|loss aversion|anchoring|sunk cost|thien kien|tam ly|behavioral)\b/.test(combined)) {
    return "behavioral";
  }
  if (/\b(ngan sach|chi tieu|50\/30\/20|du phong|khan cap|dong tien|budget)\b/.test(combined)) {
    return "budget";
  }
  if (/\b(lai kep|lam phat|tich luy|compound|dca|muc tieu tai chinh|lai suat|huu tri)\b/.test(combined)) {
    return "compound";
  }
  if (/\b(tra no|no nan|qua cau tuyet|tuyet lo|snowball|avalanche|khoan no|tin dung|vay)\b/.test(combined)) {
    return "debt";
  }
  if (/\b(bieu do nen|nen nhat|candlestick|co phieu|chung khoan|trading|bieu do|rui ro|entry|stop-loss|take-profit|p\/e|roe)\b/.test(combined)) {
    return "trading";
  }

  return "trading";
}

// =============================================================================
// CÔNG CỤ 1: BẢNG CHIA NGÂN SÁCH (50/30/20) & QUỸ DỰ PHÒNG KHẨN CẤP
// =============================================================================
function BudgetLab({ onSaveToNotebook }: { onSaveToNotebook: (text: string) => Promise<void> }) {
  const [income, setIncome] = useState<number>(20_000_000);
  const [needs, setNeeds] = useState<number>(10_000_000);
  const [wants, setWants] = useState<number>(6_000_000);
  const [savings, setSavings] = useState<number>(4_000_000);
  const [targetMonths, setTargetMonths] = useState<number>(6);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [saveSuccess, setSaveSuccess] = useState<boolean>(false);

  // Áp dụng chuẩn 50/30/20 tức thì
  const applyStandardRule = () => {
    setNeeds(Math.round(income * 0.5));
    setWants(Math.round(income * 0.3));
    setSavings(Math.round(income * 0.2));
  };

  const totalExpense = needs + wants + savings;
  const balanceDiff = income - totalExpense;

  const needsPct = income > 0 ? (needs / income) * 100 : 0;
  const wantsPct = income > 0 ? (wants / income) * 100 : 0;
  const savingsPct = income > 0 ? (savings / income) * 100 : 0;

  const idealNeeds = income * 0.5;
  const idealWants = income * 0.3;
  const idealSavings = income * 0.2;

  // Quỹ dự phòng
  const emergencyFundTarget = needs * targetMonths;
  const monthsToGoal = savings > 0 ? Math.ceil(emergencyFundTarget / savings) : null;

  // Cảnh báo
  const isHighNeeds = needsPct > 60;
  const isLowSavings = savingsPct < 10;
  const isDeficit = balanceDiff < 0;

  const handleSaveToNotebook = async () => {
    setIsSaving(true);
    const md = `### 📊 Kế Hoạch Phân Bổ Ngân Sách 50/30/20 & Quỹ Dự Phòng

- **Thu nhập ròng hàng tháng:** ${formatVND(income)}
- **Cơ cấu phân bổ thực tế:**
  - 🏠 **Chi tiêu thiết yếu (Needs):** ${formatVND(needs)} (${needsPct.toFixed(1)}%) — *Chuẩn 50% (${formatVND(idealNeeds)})*
  - ☕ **Mong muốn cá nhân (Wants):** ${formatVND(wants)} (${wantsPct.toFixed(1)}%) — *Chuẩn 30% (${formatVND(idealWants)})*
  - 📈 **Tiết kiệm & Đầu tư (Savings):** ${formatVND(savings)} (${savingsPct.toFixed(1)}%) — *Chuẩn 20% (${formatVND(idealSavings)})*
- **Tình trạng dòng tiền:** ${
      balanceDiff >= 0
        ? `Thặng dư an toàn: +${formatVND(balanceDiff)}/tháng`
        : `⚠️ Thâm hụt ngân sách: ${formatVND(balanceDiff)}/tháng`
    }
- **Quỹ dự phòng khẩn cấp (${targetMonths} tháng thiết yếu):**
  - Số tiền mục tiêu: **${formatVND(emergencyFundTarget)}**
  - Thời gian dự kiến hoàn thành: **${
    monthsToGoal !== null ? `${monthsToGoal} tháng (với mức tích lũy ${formatVND(savings)}/tháng)` : "Cần tăng tiết kiệm"
  }**
${isHighNeeds ? "\n> ⚠️ **Lưu ý:** Chi phí thiết yếu đang vượt quá 60% thu nhập, cần tìm cách tối ưu chỗ ở hoặc gia tăng thu nhập." : ""}
${isLowSavings ? "\n> ⚠️ **Lưu ý:** Tỷ lệ tiết kiệm dưới 10% sẽ khiến bạn dễ tổn thương khi có biến cố tài chính bất ngờ." : ""}
`;
    try {
      await onSaveToNotebook(md);
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Tiêu đề & Giới thiệu ngắn */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-4">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <PieChart className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            Bảng Chia Ngân Sách (50/30/20) & Quỹ Dự Phòng Khẩn Cấp
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Quy tắc tài chính cá nhân kinh điển giúp kiểm soát dòng tiền, ngăn ngừa bội chi và tích lũy an toàn.
          </p>
        </div>
        <button
          type="button"
          onClick={applyStandardRule}
          className="min-h-[44px] px-3.5 py-2 text-xs font-semibold rounded-lg bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800 hover:bg-indigo-100 dark:hover:bg-indigo-900/60 transition-colors flex items-center gap-1.5"
        >
          <Sparkles className="w-4 h-4" />
          Áp dụng chuẩn 50/30/20 tự động
        </button>
      </div>

      {/* Thông báo cảnh báo nổi bật */}
      {isDeficit && (
        <div className="p-3.5 rounded-xl border border-rose-300 bg-rose-50 dark:bg-rose-950/40 dark:border-rose-800 text-rose-800 dark:text-rose-200 flex items-start gap-2.5 text-xs">
          <AlertTriangle className="w-5 h-5 flex-shrink-0 text-rose-600 dark:text-rose-400 mt-0.5" />
          <div>
            <span className="font-bold">BÁO ĐỘNG DÒNG TIỀN ÂM:</span> Bạn đang chi tiêu vượt thu nhập{" "}
            <strong>{formatVND(Math.abs(balanceDiff))}</strong> mỗi tháng. Bạn đang phải vay nợ hoặc ăn mòn vào tiền tiết kiệm cũ!
          </div>
        </div>
      )}

      {isHighNeeds && !isDeficit && (
        <div className="p-3.5 rounded-xl border border-amber-300 bg-amber-50 dark:bg-amber-950/40 dark:border-amber-800 text-amber-900 dark:text-amber-200 flex items-start gap-2.5 text-xs">
          <ShieldAlert className="w-5 h-5 flex-shrink-0 text-amber-600 dark:text-amber-400 mt-0.5" />
          <div>
            <span className="font-bold">CẢNH BÁO CHI PHÍ CỐ ĐỊNH CAO:</span> Chi tiêu thiết yếu chiếm{" "}
            <strong>{needsPct.toFixed(1)}%</strong> (&gt; 60% thu nhập). Áp lực tài chính rất lớn nếu gián đoạn công việc.
          </div>
        </div>
      )}

      {isLowSavings && !isDeficit && (
        <div className="p-3.5 rounded-xl border border-amber-300 bg-amber-50 dark:bg-amber-950/40 dark:border-amber-800 text-amber-900 dark:text-amber-200 flex items-start gap-2.5 text-xs">
          <Info className="w-5 h-5 flex-shrink-0 text-amber-600 dark:text-amber-400 mt-0.5" />
          <div>
            <span className="font-bold">CẢNH BÁO TÍCH LŨY MỎNG:</span> Tỷ lệ tiết kiệm & đầu tư chỉ đạt{" "}
            <strong>{savingsPct.toFixed(1)}%</strong> (&lt; 10%). Bạn nên cắt giảm các khoản muốn mua sắm không thiết yếu.
          </div>
        </div>
      )}

      {/* Grid: Điều khiển nhập liệu và Trực quan hóa */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Cột trái: Nhập liệu thanh trượt & số tiền */}
        <div className="lg:col-span-6 space-y-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-900/40 p-4">
          <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
            <Sliders className="w-4 h-4 text-indigo-500" />
            Nhập Dữ Liệu Thu Chi Hàng Tháng
          </h3>

          {/* Thu nhập ròng */}
          <div className="space-y-1.5">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-income" className="font-medium text-zinc-700 dark:text-zinc-300">
                Thu nhập ròng / tháng (VNĐ)
              </label>
              <span className="font-bold text-indigo-600 dark:text-indigo-400">{formatVND(income)}</span>
            </div>
            <input
              id="input-income"
              type="range"
              min={5_000_000}
              max={100_000_000}
              step={500_000}
              value={income}
              onChange={(e) => setIncome(Number(e.target.value))}
              className="w-full accent-indigo-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Thiết yếu */}
          <div className="space-y-1.5 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-needs" className="font-medium text-blue-700 dark:text-blue-300 flex items-center gap-1">
                <span>🏠 Chi tiêu thiết yếu (50%)</span>
                <span className="text-[11px] text-zinc-600 dark:text-zinc-400">(nhà, ăn, điện nước, xăng xe)</span>
              </label>
              <span className="font-bold text-blue-600 dark:text-blue-400">
                {formatVND(needs)} ({needsPct.toFixed(1)}%)
              </span>
            </div>
            <input
              id="input-needs"
              type="range"
              min={0}
              max={income * 1.2}
              step={200_000}
              value={needs}
              onChange={(e) => setNeeds(Number(e.target.value))}
              className="w-full accent-blue-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Mong muốn */}
          <div className="space-y-1.5 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-wants" className="font-medium text-purple-700 dark:text-purple-300 flex items-center gap-1">
                <span>☕ Mong muốn cá nhân (30%)</span>
                <span className="text-[11px] text-zinc-600 dark:text-zinc-400">(ăn ngoài, mua sắm, giải trí)</span>
              </label>
              <span className="font-bold text-purple-600 dark:text-purple-400">
                {formatVND(wants)} ({wantsPct.toFixed(1)}%)
              </span>
            </div>
            <input
              id="input-wants"
              type="range"
              min={0}
              max={income * 1.2}
              step={200_000}
              value={wants}
              onChange={(e) => setWants(Number(e.target.value))}
              className="w-full accent-purple-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Tiết kiệm & Đầu tư */}
          <div className="space-y-1.5 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-savings" className="font-medium text-emerald-700 dark:text-emerald-300 flex items-center gap-1">
                <span>📈 Tiết kiệm & Đầu tư (20%)</span>
                <span className="text-[11px] text-zinc-600 dark:text-zinc-400">(tích lũy tương lai, trả nợ gốc)</span>
              </label>
              <span className="font-bold text-emerald-600 dark:text-emerald-400">
                {formatVND(savings)} ({savingsPct.toFixed(1)}%)
              </span>
            </div>
            <input
              id="input-savings"
              type="range"
              min={0}
              max={income * 1.2}
              step={200_000}
              value={savings}
              onChange={(e) => setSavings(Number(e.target.value))}
              className="w-full accent-emerald-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Số tháng quỹ dự phòng */}
          <div className="space-y-1.5 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-target-months" className="font-medium text-zinc-700 dark:text-zinc-300">
                Mục tiêu Quỹ Dự Phòng Khẩn Cấp (Số tháng chi phí)
              </label>
              <span className="font-bold text-amber-600 dark:text-amber-400">{targetMonths} tháng</span>
            </div>
            <input
              id="input-target-months"
              type="range"
              min={3}
              max={12}
              step={1}
              value={targetMonths}
              onChange={(e) => setTargetMonths(Number(e.target.value))}
              className="w-full accent-amber-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
            <div className="flex justify-between text-[10px] text-zinc-600 dark:text-zinc-400">
              <span>3 tháng (công chức ổn định)</span>
              <span>6 tháng (chuẩn khuyến nghị)</span>
              <span>12 tháng (freelancer / rủi ro cao)</span>
            </div>
          </div>
        </div>

        {/* Cột phải: Thanh tỷ lệ phân bổ & Bảng đối chiếu */}
        <div className="lg:col-span-6 space-y-4">
          {/* Thanh phân bổ thời gian thực */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-3">
            <div className="flex justify-between items-center text-xs">
              <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                Tỷ Lệ Phân Bổ Ngân Sách Thực Tế
              </span>
              <span className="text-zinc-600 dark:text-zinc-400">
                Tổng đã chia: <strong>{formatVND(totalExpense)}</strong> / {formatVND(income)}
              </span>
            </div>

            {/* Thanh tiến trình phân đoạn */}
            <div className="h-6 w-full rounded-lg bg-zinc-100 dark:bg-zinc-800 flex overflow-hidden p-0.5 gap-0.5">
              <div
                className="bg-blue-500 rounded-l transition-all duration-300 flex items-center justify-center text-[10px] text-white font-bold overflow-hidden"
                style={{ width: `${Math.min(100, Math.max(0, needsPct))}%` }}
                title={`Thiết yếu: ${needsPct.toFixed(1)}%`}
              >
                {needsPct >= 12 ? `${needsPct.toFixed(0)}%` : ""}
              </div>
              <div
                className="bg-purple-500 transition-all duration-300 flex items-center justify-center text-[10px] text-white font-bold overflow-hidden"
                style={{ width: `${Math.min(100 - needsPct, Math.max(0, wantsPct))}%` }}
                title={`Mong muốn: ${wantsPct.toFixed(1)}%`}
              >
                {wantsPct >= 12 ? `${wantsPct.toFixed(0)}%` : ""}
              </div>
              <div
                className="bg-emerald-500 rounded-r transition-all duration-300 flex items-center justify-center text-[10px] text-white font-bold overflow-hidden"
                style={{ width: `${Math.min(100 - needsPct - wantsPct, Math.max(0, savingsPct))}%` }}
                title={`Tiết kiệm: ${savingsPct.toFixed(1)}%`}
              >
                {savingsPct >= 12 ? `${savingsPct.toFixed(0)}%` : ""}
              </div>
            </div>

            {/* Chú giải màu sắc */}
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs pt-1">
              <div className="flex items-center gap-1.5">
                <span className="w-3 h-3 rounded-sm bg-blue-500 inline-block" />
                <span className="text-zinc-700 dark:text-zinc-300">Thiết yếu ({needsPct.toFixed(1)}%)</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="w-3 h-3 rounded-sm bg-purple-500 inline-block" />
                <span className="text-zinc-700 dark:text-zinc-300">Mong muốn ({wantsPct.toFixed(1)}%)</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="w-3 h-3 rounded-sm bg-emerald-500 inline-block" />
                <span className="text-zinc-700 dark:text-zinc-300">Tiết kiệm ({savingsPct.toFixed(1)}%)</span>
              </div>
            </div>
          </div>

          {/* Bảng so sánh với chuẩn 50/30/20 */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-3">
            <h4 className="text-xs font-bold text-zinc-900 dark:text-zinc-100 uppercase tracking-wider">
              Đối Chiếu Chuẩn 50/30/20
            </h4>
            <div className="space-y-2 text-xs">
              {/* Row Thiết yếu */}
              <div className="flex items-center justify-between p-2 rounded-lg bg-zinc-50 dark:bg-zinc-800/40">
                <div>
                  <div className="font-semibold text-zinc-800 dark:text-zinc-200">50% Thiết yếu (Chuẩn: {formatVND(idealNeeds)})</div>
                  <div className="text-[11px] text-zinc-600 dark:text-zinc-400">
                    Thực tế: {formatVND(needs)} ({needsPct.toFixed(1)}%)
                  </div>
                </div>
                <span
                  className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                    needs <= idealNeeds
                      ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300"
                      : needs <= idealNeeds * 1.2
                      ? "bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300"
                      : "bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300"
                  }`}
                >
                  {needs <= idealNeeds ? "Đạt chuẩn" : `Vượt +${formatVND(needs - idealNeeds)}`}
                </span>
              </div>

              {/* Row Mong muốn */}
              <div className="flex items-center justify-between p-2 rounded-lg bg-zinc-50 dark:bg-zinc-800/40">
                <div>
                  <div className="font-semibold text-zinc-800 dark:text-zinc-200">30% Mong muốn (Chuẩn: {formatVND(idealWants)})</div>
                  <div className="text-[11px] text-zinc-600 dark:text-zinc-400">
                    Thực tế: {formatVND(wants)} ({wantsPct.toFixed(1)}%)
                  </div>
                </div>
                <span
                  className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                    wants <= idealWants
                      ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300"
                      : "bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300"
                  }`}
                >
                  {wants <= idealWants ? "Tốt" : `Vượt +${formatVND(wants - idealWants)}`}
                </span>
              </div>

              {/* Row Tiết kiệm */}
              <div className="flex items-center justify-between p-2 rounded-lg bg-zinc-50 dark:bg-zinc-800/40">
                <div>
                  <div className="font-semibold text-zinc-800 dark:text-zinc-200">20% Tiết kiệm & Đầu tư (Chuẩn: {formatVND(idealSavings)})</div>
                  <div className="text-[11px] text-zinc-600 dark:text-zinc-400">
                    Thực tế: {formatVND(savings)} ({savingsPct.toFixed(1)}%)
                  </div>
                </div>
                <span
                  className={`px-2 py-0.5 rounded text-[11px] font-semibold ${
                    savings >= idealSavings
                      ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300"
                      : savings >= idealSavings * 0.5
                      ? "bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300"
                      : "bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300"
                  }`}
                >
                  {savings >= idealSavings ? "Đạt chuẩn" : `Thiếu -${formatVND(idealSavings - savings)}`}
                </span>
              </div>
            </div>
          </div>

          {/* Thẻ Quỹ dự phòng khẩn cấp */}
          <div className="rounded-xl border border-amber-200 dark:border-amber-900/60 bg-amber-50/50 dark:bg-amber-950/20 p-4 space-y-2">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-amber-900 dark:text-amber-200 flex items-center gap-1.5">
                <Target className="w-4 h-4 text-amber-600 dark:text-amber-400" />
                Quỹ Dự Phòng Khẩn Cấp ({targetMonths} tháng thiết yếu)
              </span>
              <span className="text-sm font-extrabold text-amber-700 dark:text-amber-300">
                {formatVND(emergencyFundTarget)}
              </span>
            </div>
            <p className="text-xs text-zinc-700 dark:text-zinc-300">
              {savings > 0 ? (
                <>
                  Với mức tiết kiệm <strong>{formatVND(savings)}/tháng</strong>, bạn cần{" "}
                  <strong className="text-indigo-600 dark:text-indigo-400">{monthsToGoal} tháng</strong> (khoảng{" "}
                  {(Number(monthsToGoal) / 12).toFixed(1)} năm) để xây dựng tấm lá chắn tài chính này.
                </>
              ) : (
                <span className="text-rose-600 dark:text-rose-400 font-semibold">
                  Hiện bạn chưa có khoản tiết kiệm nào để tích lũy cho quỹ dự phòng!
                </span>
              )}
            </p>
          </div>

          {/* Nút lưu vào Sổ tay */}
          <button
            type="button"
            disabled={isSaving}
            onClick={handleSaveToNotebook}
            className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-indigo-600 text-white font-semibold text-xs hover:bg-indigo-700 active:scale-[0.99] transition-all flex items-center justify-center gap-2 shadow-sm disabled:opacity-50"
          >
            {isSaving ? (
              <Activity className="w-4 h-4 animate-spin" />
            ) : saveSuccess ? (
              <CheckCircle2 className="w-4 h-4 text-emerald-300" />
            ) : (
              <BookOpen className="w-4 h-4" />
            )}
            {saveSuccess ? "Đã lưu kế hoạch vào Sổ tay!" : "Lưu kế hoạch ngân sách vào Sổ tay"}
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 2: MÁY TÍNH LÃI KÉP, LẠM PHÁT & MỤC TIÊU TÀI CHÍNH
// =============================================================================
function CompoundInterestLab({
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: {
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}) {
  const [showYearlyTable, setShowYearlyTable] = useState(false);
  const [fireChoice, setFireChoice] = useState<number | null>(null);
  const [fireAnswerChecked, setFireAnswerChecked] = useState(false);
  const [initialCapital, setInitialCapital] = useState<number>(50_000_000);
  const [monthlyDeposit, setMonthlyDeposit] = useState<number>(5_000_000);
  const [annualRate, setAnnualRate] = useState<number>(10);
  const [inflationRate, setInflationRate] = useState<number>(4);
  const [years, setYears] = useState<number>(15);
  const [hoveredYear, setHoveredYear] = useState<number | null>(null);

  const [isSaving, setIsSaving] = useState(false);
  const [isCardCreating, setIsCardCreating] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  // Tính toán chuỗi tăng trưởng theo từng năm
  const yearlyData = useMemo(() => {
    const list: Array<{
      year: number;
      principal: number;
      nominalFV: number;
      savingsFV: number;
      interest: number;
      realFV: number;
    }> = [];

    const monthlyRate = annualRate / 100 / 12;

    for (let y = 1; y <= years; y++) {
      const m = y * 12;
      const principal = initialCapital + monthlyDeposit * m;

      let nominalFV = principal;
      if (monthlyRate > 0) {
        nominalFV =
          initialCapital * Math.pow(1 + monthlyRate, m) +
          monthlyDeposit * ((Math.pow(1 + monthlyRate, m) - 1) / monthlyRate);
      }

      const savingsRate = 0.06 / 12;
      const savingsFV =
        initialCapital * Math.pow(1 + savingsRate, m) +
        monthlyDeposit * ((Math.pow(1 + savingsRate, m) - 1) / savingsRate);

      const interest = Math.max(0, nominalFV - principal);
      const realFV = nominalFV / Math.pow(1 + inflationRate / 100, y);

      list.push({
        year: y,
        principal,
        nominalFV,
        savingsFV,
        interest,
        realFV,
      });
    }

    return list;
  }, [initialCapital, monthlyDeposit, annualRate, inflationRate, years]);

  const finalYear = yearlyData[yearlyData.length - 1] || {
    principal: initialCapital,
    nominalFV: initialCapital,
    savingsFV: initialCapital,
    interest: 0,
    realFV: initialCapital,
    year: years,
  };

  const interestMultiplier = finalYear.principal > 0 ? finalYear.interest / finalYear.principal : 0;
  const rule72 = annualRate > 0 ? (72 / annualRate).toFixed(1) : "—";

  // Dữ liệu cho SVG chart
  const svgW = 720;
  const svgH = 260;
  const padL = 60;
  const padR = 24;
  const padT = 20;
  const padB = 36;
  const plotW = svgW - padL - padR;
  const plotH = svgH - padT - padB;

  const maxVal = Math.max(
    1,
    ...yearlyData.map((d) => d.nominalFV),
    finalYear.nominalFV * 1.05
  );

  const scaleY = (val: number) => padT + plotH - (val / maxVal) * plotH;
  const scaleX = (index: number) => padL + (index / Math.max(1, yearlyData.length - 1)) * plotW;

  // Đường Path SVG cho Sức mua thực tế & Gửi tiết kiệm & Đầu tư DCA
  const realPathD = useMemo(() => {
    if (yearlyData.length === 0) return "";
    return yearlyData
      .map((d, i) => `${i === 0 ? "M" : "L"} ${scaleX(i).toFixed(1)} ${scaleY(d.realFV).toFixed(1)}`)
      .join(" ");
  }, [yearlyData, maxVal]);

  const savingsPathD = useMemo(() => {
    if (yearlyData.length === 0) return "";
    return yearlyData
      .map((d, i) => `${i === 0 ? "M" : "L"} ${scaleX(i).toFixed(1)} ${scaleY(d.savingsFV).toFixed(1)}`)
      .join(" ");
  }, [yearlyData, maxVal]);

  const nominalPathD = useMemo(() => {
    if (yearlyData.length === 0) return "";
    return yearlyData
      .map((d, i) => `${i === 0 ? "M" : "L"} ${scaleX(i).toFixed(1)} ${scaleY(d.nominalFV).toFixed(1)}`)
      .join(" ");
  }, [yearlyData, maxVal]);

  const handleSaveToNotebook = async () => {
    setIsSaving(true);
    const md = `### 📈 Kế Hoạch Lãi Kép & Mục Tiêu Tài Chính (${years} Năm)

- **Vốn ban đầu:** ${formatVND(initialCapital)}
- **Góp thêm đều đặn:** ${formatVND(monthlyDeposit)}/tháng
- **Lãi suất kỳ vọng:** ${annualRate}%/năm · **Lạm phát ước tính:** ${inflationRate}%/năm
- **Quy tắc 72:** Tài sản tự nhân đôi sau khoảng **${rule72} năm**.

#### Kết quả dự phóng sau ${years} năm:
- 💰 **Tổng tài sản danh nghĩa:** **${formatVND(finalYear.nominalFV)}**
- 💵 **Tổng tiền vốn gốc bỏ ra:** ${formatVND(finalYear.principal)}
- 🚀 **Tiền lãi kép sinh lời:** **${formatVND(finalYear.interest)}** (Lãi gấp **${interestMultiplier.toFixed(2)} lần** tiền gốc)
- 🛒 **Sức mua thực tế (trừ lạm phát):** **${formatVND(finalYear.realFV)}**
`;
    try {
      await onSaveToNotebook(md);
      setFeedback("Đã lưu kế hoạch lãi kép vào Sổ tay!");
      setTimeout(() => setFeedback(null), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCreateFlashcard = async () => {
    if (!onCreateFlashcardQuick) return;
    setIsCardCreating(true);
    try {
      await onCreateFlashcardQuick(
        "Quy tắc 72 trong đầu tư tài chính là gì và dùng để làm gì?",
        "Quy tắc 72 dùng để ước lượng nhanh số năm cần thiết để một khoản đầu tư nhân đôi tài sản ở một mức lãi suất kép cố định.\n\nCông thức: Số năm nhân đôi ≈ 72 / Lãi suất (%/năm).\nVí dụ: Lãi 10%/năm => 72 / 10 = 7.2 năm tài sản sẽ tăng gấp đôi."
      );
      setFeedback("Đã tạo Flashcard Quy tắc 72 thành công!");
      setTimeout(() => setFeedback(null), 3000);
    } finally {
      setIsCardCreating(false);
    }
  };

  const activeFocusYear = hoveredYear !== null ? yearlyData[hoveredYear] : finalYear;

  return (
    <div className="space-y-6">
      {/* Tiêu đề & Giới thiệu */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-4">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            Máy Tính Lãi Kép, Lạm Phát & Mục Tiêu Tài Chính
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Sức mạnh kỳ quan thứ 8: Xem tiền lãi tự sinh sôi và đối chiếu sức mua thực tế sau khi trừ lạm phát qua từng năm.
          </p>
        </div>
        <div className="flex items-center gap-2">
          {onCreateFlashcardQuick && (
            <button
              type="button"
              disabled={isCardCreating}
              onClick={handleCreateFlashcard}
              className="min-h-[44px] px-3 py-1.5 text-xs font-semibold rounded-lg bg-amber-50 dark:bg-amber-950/60 text-amber-700 dark:text-amber-300 border border-amber-200 dark:border-amber-800 hover:bg-amber-100 transition-colors flex items-center gap-1.5"
            >
              <Sparkles className="w-4 h-4" />
              Tạo thẻ Flashcard Quy tắc 72
            </button>
          )}
        </div>
      </div>

      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
          <span>{feedback}</span>
        </div>
      )}

      {/* Grid điều khiển & Biểu đồ */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Sliders điều khiển */}
        <div className="lg:col-span-5 space-y-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-900/40 p-4">
          <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
            <Sliders className="w-4 h-4 text-emerald-500" />
            Thông Số Đầu Tư Định Kỳ
          </h3>

          {/* Vốn ban đầu */}
          <div className="space-y-1">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-initial-capital" className="font-medium text-zinc-700 dark:text-zinc-300">
                Vốn ban đầu (VNĐ)
              </label>
              <span className="font-bold text-zinc-900 dark:text-zinc-100">{formatVND(initialCapital)}</span>
            </div>
            <input
              id="input-initial-capital"
              type="range"
              min={0}
              max={500_000_000}
              step={5_000_000}
              value={initialCapital}
              onChange={(e) => setInitialCapital(Number(e.target.value))}
              className="w-full accent-emerald-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Góp thêm hàng tháng */}
          <div className="space-y-1 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-monthly-deposit" className="font-medium text-zinc-700 dark:text-zinc-300">
                Góp thêm mỗi tháng (VNĐ)
              </label>
              <span className="font-bold text-emerald-600 dark:text-emerald-400">{formatVND(monthlyDeposit)}</span>
            </div>
            <input
              id="input-monthly-deposit"
              type="range"
              min={500_000}
              max={30_000_000}
              step={500_000}
              value={monthlyDeposit}
              onChange={(e) => setMonthlyDeposit(Number(e.target.value))}
              className="w-full accent-emerald-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Lãi suất kỳ vọng */}
          <div className="space-y-1 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-annual-rate" className="font-medium text-zinc-700 dark:text-zinc-300">
                Lãi suất kỳ vọng (%/năm)
              </label>
              <span className="font-bold text-indigo-600 dark:text-indigo-400">{annualRate}% / năm</span>
            </div>
            <input
              id="input-annual-rate"
              type="range"
              min={2}
              max={25}
              step={0.5}
              value={annualRate}
              onChange={(e) => setAnnualRate(Number(e.target.value))}
              className="w-full accent-indigo-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
            <div className="flex justify-between text-[10px] text-zinc-600 dark:text-zinc-400">
              <span>Gửi tiết kiệm (~5%)</span>
              <span>Trái phiếu (~8%)</span>
              <span>Cổ phiếu/ETF (~12-15%)</span>
            </div>
          </div>

          {/* Lạm phát ước tính */}
          <div className="space-y-1 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-inflation-rate" className="font-medium text-zinc-700 dark:text-zinc-300">
                Lạm phát ước tính (%/năm)
              </label>
              <span className="font-bold text-amber-600 dark:text-amber-400">{inflationRate}% / năm</span>
            </div>
            <input
              id="input-inflation-rate"
              type="range"
              min={0}
              max={10}
              step={0.5}
              value={inflationRate}
              onChange={(e) => setInflationRate(Number(e.target.value))}
              className="w-full accent-amber-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Thời gian */}
          <div className="space-y-1 pt-2 border-t border-zinc-200 dark:border-zinc-800">
            <div className="flex justify-between items-center text-xs">
              <label htmlFor="input-years" className="font-medium text-zinc-700 dark:text-zinc-300">
                Thời gian đầu tư (Năm)
              </label>
              <span className="font-bold text-zinc-900 dark:text-zinc-100">{years} năm</span>
            </div>
            <input
              id="input-years"
              type="range"
              min={1}
              max={35}
              step={1}
              value={years}
              onChange={(e) => setYears(Number(e.target.value))}
              className="w-full accent-emerald-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
            />
          </div>

          {/* Thẻ Quy tắc 72 */}
          <div className="p-3 rounded-lg bg-indigo-50/60 dark:bg-indigo-950/30 border border-indigo-200 dark:border-indigo-900/50 text-xs space-y-1">
            <div className="font-bold text-indigo-900 dark:text-indigo-200 flex items-center gap-1.5">
              <Sparkles className="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
              Quy tắc 72: Nhân đôi tài sản
            </div>
            <p className="text-zinc-700 dark:text-zinc-300">
              Ở mức sinh lời <strong>{annualRate}%/năm</strong>, tài sản của bạn sẽ tự nhân đôi sau khoảng{" "}
              <strong className="text-indigo-700 dark:text-indigo-300">{rule72} năm</strong> (72 ÷ {annualRate}).
            </p>
          </div>
        </div>

        {/* Biểu đồ SVG & Bảng tóm tắt kết quả */}
        <div className="lg:col-span-7 space-y-4">
          {/* 4 Thẻ chỉ số chính */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] uppercase font-bold text-zinc-600 dark:text-zinc-400">Tổng Danh Nghĩa</span>
              <div className="text-sm font-extrabold text-emerald-600 dark:text-emerald-400 mt-0.5 truncate">
                {formatCompactVND(finalYear.nominalFV)}
              </div>
              <span className="text-[10px] text-zinc-600 dark:text-zinc-400">Sau {years} năm</span>
            </div>
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] uppercase font-bold text-zinc-600 dark:text-zinc-400">Tiền Vốn Gốc</span>
              <div className="text-sm font-extrabold text-blue-600 dark:text-blue-400 mt-0.5 truncate">
                {formatCompactVND(finalYear.principal)}
              </div>
              <span className="text-[10px] text-zinc-600 dark:text-zinc-400">Tự bỏ ra</span>
            </div>
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] uppercase font-bold text-zinc-600 dark:text-zinc-400">Lãi Kép Sinh Ra</span>
              <div className="text-sm font-extrabold text-purple-600 dark:text-purple-400 mt-0.5 truncate">
                {formatCompactVND(finalYear.interest)}
              </div>
              <span className="text-[10px] text-emerald-600 font-semibold">Gấp {interestMultiplier.toFixed(1)}x gốc</span>
            </div>
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] uppercase font-bold text-zinc-600 dark:text-zinc-400">Sức Mua Thực</span>
              <div className="text-sm font-extrabold text-amber-600 dark:text-amber-400 mt-0.5 truncate">
                {formatCompactVND(finalYear.realFV)}
              </div>
              <span className="text-[10px] text-zinc-600 dark:text-zinc-400">Trừ lạm phát {inflationRate}%</span>
            </div>
          </div>

          {/* Biểu đồ SVG Tăng trưởng 3 lớp */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-2">
            <div className="flex flex-wrap items-center justify-between text-xs">
              <span className="font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
                Biểu Đồ Tăng Trưởng Tài Sản & Sức Mua
              </span>
              {/* Chú giải 4 đường/lớp */}
              <div className="flex flex-wrap items-center gap-3 text-[11px]">
                <div className="flex items-center gap-1">
                  <span className="w-2.5 h-2.5 rounded-sm bg-blue-500 inline-block" />
                  <span className="text-zinc-600 dark:text-zinc-400">Vốn tự bỏ ra</span>
                </div>
                <div className="flex items-center gap-1">
                  <span className="w-3 h-0.5 bg-blue-600 inline-block" />
                  <span className="text-zinc-600 dark:text-zinc-400">Tiết kiệm 6%</span>
                </div>
                <div className="flex items-center gap-1">
                  <span className="w-2.5 h-2.5 rounded-sm bg-emerald-500 inline-block" />
                  <span className="text-zinc-600 dark:text-zinc-400">Đầu tư DCA</span>
                </div>
                <div className="flex items-center gap-1">
                  <span className="w-3 h-0.5 border-t-2 border-dashed border-amber-500 inline-block" />
                  <span className="text-zinc-600 dark:text-zinc-400">Sức mua thực</span>
                </div>
              </div>
            </div>

            {/* SVG Canvas co giãn viewBox */}
            <div className="w-full bg-zinc-50 dark:bg-zinc-950/50 rounded-lg p-2 border border-zinc-100 dark:border-zinc-800/80">
              <svg viewBox={`0 0 ${svgW} ${svgH}`} className="w-full h-auto block select-none">
                {/* Lưới ngang Grid lines */}
                {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
                  const val = maxVal * ratio;
                  const y = padT + plotH - ratio * plotH;
                  return (
                    <g key={ratio}>
                      <line
                        x1={padL}
                        y1={y}
                        x2={svgW - padR}
                        y2={y}
                        stroke="currentColor"
                        strokeDasharray="3 3"
                        className="text-zinc-200 dark:text-zinc-800"
                        strokeWidth="1"
                      />
                      <text
                        x={padL - 6}
                        y={y + 3}
                        fontSize="9"
                        textAnchor="end"
                        className="fill-zinc-400 dark:fill-zinc-500 font-mono"
                      >
                        {formatCompactVND(val)}
                      </text>
                    </g>
                  );
                })}

                {/* Các cột xếp tầng: Vốn gốc + Tiền lãi */}
                {yearlyData.map((d, i) => {
                  const x = scaleX(i);
                  const barW = Math.max(4, Math.min(22, (plotW / yearlyData.length) * 0.7));
                  const principalH = (d.principal / maxVal) * plotH;
                  const totalH = (d.nominalFV / maxVal) * plotH;
                  const interestH = Math.max(0, totalH - principalH);

                  const isHovered = hoveredYear === i;

                  return (
                    <g
                      key={d.year}
                      onMouseEnter={() => setHoveredYear(i)}
                      onMouseLeave={() => setHoveredYear(null)}
                      className="cursor-pointer transition-opacity"
                    >
                      {/* Vốn gốc (Xanh dương) */}
                      <rect
                        x={x - barW / 2}
                        y={padT + plotH - principalH}
                        width={barW}
                        height={principalH}
                        fill="#3b82f6"
                        opacity={isHovered ? 1 : 0.85}
                        rx="1"
                      />
                      {/* Lãi kép sinh sôi (Xanh lá) */}
                      <rect
                        x={x - barW / 2}
                        y={padT + plotH - totalH}
                        width={barW}
                        height={interestH}
                        fill="#10b981"
                        opacity={isHovered ? 1 : 0.85}
                        rx="1"
                      />
                      {/* Nhãn năm dưới trục X */}
                      {(yearlyData.length <= 15 || d.year % 5 === 0 || d.year === 1 || d.year === years) && (
                        <text
                          x={x}
                          y={svgH - 12}
                          fontSize="9"
                          textAnchor="middle"
                          className="fill-zinc-400 dark:fill-zinc-500 font-mono"
                        >
                          Năm {d.year}
                        </text>
                      )}
                    </g>
                  );
                })}

                {/* Đường biểu diễn Sức mua thực tế (Trừ lạm phát) */}
                <path
                  d={realPathD}
                  fill="none"
                  stroke="#f59e0b"
                  strokeWidth="2.2"
                  strokeDasharray="4 3"
                />

                {/* Các chấm tròn mốc sức mua thực */}
                {yearlyData.map((d, i) => {
                  const x = scaleX(i);
                  const y = scaleY(d.realFV);
                  const isKeyYear = d.year % 5 === 0 || d.year === years;
                  if (!isKeyYear && hoveredYear !== i) return null;
                  return (
                    <circle
                      key={`dot-${d.year}`}
                      cx={x}
                      cy={y}
                      r={hoveredYear === i ? 4.5 : 3}
                      fill="#f59e0b"
                      stroke="#ffffff"
                      strokeWidth="1.5"
                    />
                  );
                })}
              </svg>
            </div>

            {/* Thông tin chi tiết mốc năm được chọn / hover */}
            <div className="p-3 rounded-lg bg-zinc-50 dark:bg-zinc-800/50 border border-zinc-200 dark:border-zinc-800 text-xs flex flex-wrap items-center justify-between gap-2">
              <span className="font-bold text-zinc-900 dark:text-zinc-100">
                Chi tiết mốc Năm {activeFocusYear.year}:
              </span>
              <div className="flex flex-wrap items-center gap-3">
                <span className="text-blue-600 dark:text-blue-400">
                  Gốc: <strong>{formatVND(activeFocusYear.principal)}</strong>
                </span>
                <span className="text-blue-800 dark:text-blue-300">
                  Tiết kiệm (6%): <strong>{formatVND(activeFocusYear.savingsFV || 0)}</strong>
                </span>
                <span className="text-emerald-600 dark:text-emerald-400">
                  Đầu tư DCA: <strong>{formatVND(activeFocusYear.nominalFV)}</strong>
                </span>
                <span className="text-amber-600 dark:text-amber-400">
                  Sức mua thực: <strong>{formatVND(activeFocusYear.realFV)}</strong>
                </span>
              </div>
            </div>

            {/* Nút bật/tắt Bảng so sánh từng năm */}
            <div className="pt-2">
              <button
                type="button"
                onClick={() => setShowYearlyTable(!showYearlyTable)}
                className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline font-semibold inline-flex items-center gap-1.5 cursor-pointer"
              >
                <Layers className="w-3.5 h-3.5" />
                <span>{showYearlyTable ? "Thu gọn bảng số liệu từng năm" : "Xem bảng so sánh chi tiết từng năm (DCA vs Tiết kiệm vs Lạm phát)"}</span>
              </button>

              {showYearlyTable && (
                <div className="mt-3 overflow-x-auto rounded-xl border border-zinc-200 dark:border-zinc-800 text-xs">
                  <table className="w-full text-left border-collapse">
                    <thead className="bg-zinc-100 dark:bg-zinc-800 text-[11px] font-bold text-zinc-700 dark:text-zinc-300 uppercase">
                      <tr>
                        <th className="p-2.5">Năm</th>
                        <th className="p-2.5">Vốn tự bỏ</th>
                        <th className="p-2.5">Gửi Tiết Kiệm (6%)</th>
                        <th className="p-2.5 text-emerald-600 dark:text-emerald-400">Đầu tư DCA ({annualRate}%)</th>
                        <th className="p-2.5 text-amber-600 dark:text-amber-400">Sức mua thực ({inflationRate}%)</th>
                        <th className="p-2.5">Chênh lệch DCA vs TK</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800 font-mono text-[11px]">
                      {yearlyData.map((d) => {
                        const diff = d.nominalFV - (d.savingsFV || 0);
                        return (
                          <tr key={d.year} className="hover:bg-zinc-50 dark:hover:bg-zinc-800/60">
                            <td className="p-2.5 font-sans font-bold">Năm {d.year}</td>
                            <td className="p-2.5 text-zinc-600 dark:text-zinc-400">{formatCompactVND(d.principal)}</td>
                            <td className="p-2.5 text-blue-600 dark:text-blue-400">{formatCompactVND(d.savingsFV || 0)}</td>
                            <td className="p-2.5 font-bold text-emerald-600 dark:text-emerald-400">{formatCompactVND(d.nominalFV)}</td>
                            <td className="p-2.5 text-amber-600 dark:text-amber-400">{formatCompactVND(d.realFV)}</td>
                            <td className="p-2.5 font-bold text-emerald-700 dark:text-emerald-300">
                              +{formatCompactVND(diff)}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* BÀI TẬP THỰC HÀNH CÓ CHẤM ĐIỂM: MỤC TIÊU TỰ DO TÀI CHÍNH FIRE */}
            <div className="mt-4 p-4 rounded-xl border border-emerald-200 dark:border-emerald-800/70 bg-emerald-50/50 dark:bg-emerald-950/20 space-y-3">
              <div className="flex items-center justify-between gap-2">
                <span className="text-xs font-bold text-emerald-800 dark:text-emerald-300 flex items-center gap-1.5 uppercase tracking-wide">
                  <Award className="w-4 h-4 text-emerald-600" />
                  Bài Tập Thực Hành: Tính Mục Tiêu Độc Lập Tài Chính FIRE
                </span>
                <span className="text-[10px] px-2 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-900/60 text-emerald-700 dark:text-emerald-300 font-semibold">
                  Chấm điểm thật
                </span>
              </div>
              <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed">
                <strong>Tình huống:</strong> Bạn đặt mục tiêu tích lũy quỹ FIRE <strong>3 tỷ VNĐ</strong> sau <strong>15 năm</strong>, với vốn ban đầu là 50 triệu và lãi suất kép kỳ vọng <strong>12%/năm</strong>. Số tiền đóng góp định kỳ DCA mỗi tháng tối thiểu cần chuẩn bị ước tính là bao nhiêu?
              </p>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                {[
                  { id: 0, text: "A. Khoảng 3.5 – 4.5 triệu VNĐ/tháng", isCorrect: false },
                  { id: 1, text: "B. Khoảng 5.5 – 6.5 triệu VNĐ/tháng (Chính xác ~ 5.9 tr)", isCorrect: true },
                  { id: 2, text: "C. Khoảng 12.0 – 15.0 triệu VNĐ/tháng", isCorrect: false },
                  { id: 3, text: "D. Khoảng 20.0 – 25.0 triệu VNĐ/tháng", isCorrect: false },
                ].map((opt) => (
                  <button
                    key={opt.id}
                    type="button"
                    onClick={() => {
                      setFireChoice(opt.id);
                      setFireAnswerChecked(false);
                    }}
                    className={`p-2.5 rounded-lg border text-left text-xs transition cursor-pointer ${
                      fireChoice === opt.id
                        ? "border-emerald-500 bg-emerald-100/60 dark:bg-emerald-900/40 text-emerald-900 dark:text-emerald-100 font-medium"
                        : "border-zinc-200 dark:border-zinc-700 bg-white dark:bg-zinc-900 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-50"
                    }`}
                  >
                    {opt.text}
                  </button>
                ))}
              </div>
              <div className="flex items-center justify-between pt-1">
                <span className="text-[11px] text-zinc-500">
                  Dựa trên công thức giá trị tương lai dòng tiền niên kim (Annuity FV)
                </span>
                <button
                  type="button"
                  disabled={fireChoice === null}
                  onClick={async () => {
                    if (fireChoice === null) return;
                    setFireAnswerChecked(true);
                    const isCorrect = fireChoice === 1;
                    const score = isCorrect ? 100 : 40;
                    if (onRecordAttempt) {
                      await onRecordAttempt({
                        activityType: "compound_calculator",
                        activityKey: "fire-goal-dca-calc",
                        activityTitle: "Thực hành tính mục tiêu tự do tài chính FIRE",
                        score,
                        isPassed: isCorrect,
                        detailJson: JSON.stringify({
                          chosenOption: fireChoice,
                          isCorrect,
                          targetFund: 3_000_000_000,
                          years: 15,
                          rate: 12,
                        }),
                      });
                    }
                  }}
                  className="px-3.5 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-white font-semibold text-xs transition cursor-pointer"
                >
                  Kiểm tra & Ghi điểm
                </button>
              </div>

              {fireAnswerChecked && (
                <div className={`p-3 rounded-lg border text-xs leading-relaxed space-y-1 ${
                  fireChoice === 1
                    ? "bg-emerald-100/70 border-emerald-300 dark:bg-emerald-950/60 dark:border-emerald-800 text-emerald-900 dark:text-emerald-200"
                    : "bg-amber-100/70 border-amber-300 dark:bg-amber-950/60 dark:border-amber-800 text-amber-900 dark:text-amber-200"
                }`}>
                  <div className="font-bold flex items-center gap-1.5">
                    {fireChoice === 1 ? <CheckCircle2 className="w-4 h-4 text-emerald-600" /> : <AlertTriangle className="w-4 h-4 text-amber-600" />}
                    <span>{fireChoice === 1 ? "Chính xác! (100/100 điểm)" : "Chưa chính xác (40/100 điểm)"}</span>
                  </div>
                  <p>
                    <strong>Giải thích chi tiết:</strong> Với FV = 3.000.000.000 đ, PV = 50.000.000 đ, r = 12%/12 = 1%/tháng, n = 180 tháng. Áp dụng công thức dòng tiền đều: sau 15 năm vốn 50 triệu ban đầu tăng lên ~ 299 triệu; phần còn lại ~ 2.701 tỷ cần được bù đắp bởi khoản đóng góp định kỳ DCA khoảng <strong>5.88 triệu VNĐ/tháng</strong> (tổng vốn gốc tự bỏ khoảng 1.1 tỷ, còn lại 1.9 tỷ là lãi kép sinh sôi).
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Nút lưu Sổ tay */}
          <button
            type="button"
            disabled={isSaving}
            onClick={handleSaveToNotebook}
            className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-emerald-600 text-white font-semibold text-xs hover:bg-emerald-700 active:scale-[0.99] transition-all flex items-center justify-center gap-2 shadow-sm disabled:opacity-50"
          >
            {isSaving ? <Activity className="w-4 h-4 animate-spin" /> : <BookOpen className="w-4 h-4" />}
            Lưu kế hoạch lãi kép vào Sổ tay
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 3: LẬP LỘ TRÌNH TRẢ NỢ (QUẢ CẦU TUYẾT VS. TUYẾT LỞ)
// =============================================================================
interface DebtItem {
  id: string;
  name: string;
  balance: number;
  interestRate: number; // % / năm
  minPayment: number; // VNĐ / tháng
}

const DEFAULT_DEBTS: DebtItem[] = [
  {
    id: "credit-card",
    name: "Thẻ tín dụng lãi cao",
    balance: 25_000_000,
    interestRate: 28,
    minPayment: 1_500_000,
  },
  {
    id: "installment",
    name: "Trả góp điện thoại / xe",
    balance: 15_000_000,
    interestRate: 12,
    minPayment: 1_400_000,
  },
  {
    id: "personal-loan",
    name: "Vay cá nhân / người thân",
    balance: 50_000_000,
    interestRate: 6,
    minPayment: 2_000_000,
  },
];

function DebtPayoffLab({ onSaveToNotebook }: { onSaveToNotebook: (text: string) => Promise<void> }) {
  const [debts, setDebts] = useState<DebtItem[]>(DEFAULT_DEBTS);
  const [extraPayment, setExtraPayment] = useState<number>(2_000_000);
  const [isSaving, setIsSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  // Form thêm khoản nợ
  const [newDebtName, setNewDebtName] = useState("");
  const [newDebtBalance, setNewDebtBalance] = useState(10_000_000);
  const [newDebtRate, setNewDebtRate] = useState(15);
  const [newDebtMin, setNewDebtMin] = useState(1_000_000);
  const [showAddForm, setShowAddForm] = useState(false);

  const addDebt = () => {
    if (!newDebtName.trim()) return;
    const newItem: DebtItem = {
      id: "debt-" + Date.now(),
      name: newDebtName.trim(),
      balance: newDebtBalance,
      interestRate: newDebtRate,
      minPayment: newDebtMin,
    };
    setDebts([...debts, newItem]);
    setNewDebtName("");
    setShowAddForm(false);
  };

  const removeDebt = (id: string) => {
    setDebts(debts.filter((d) => d.id !== id));
  };

  const resetDefaultDebts = () => {
    setDebts(DEFAULT_DEBTS);
  };

  // Thuật toán mô phỏng trả nợ hàng tháng
  const simulatePayoff = useCallback(
    (strategy: "snowball" | "avalanche") => {
      if (debts.length === 0) {
        return { months: 0, totalInterest: 0, clearOrder: [] };
      }

      // Clone các khoản nợ
      let active = debts.map((d) => ({
        ...d,
        currentBalance: d.balance,
      }));

      let month = 0;
      let totalInterest = 0;
      const clearOrder: Array<{ name: string; month: number }> = [];

      // Giới hạn 360 tháng để phòng trường hợp thanh toán tối thiểu không đủ trả lãi
      while (active.some((d) => d.currentBalance > 0.01) && month < 360) {
        month++;

        // 1. Tính lãi hàng tháng cho các khoản còn nợ
        for (const d of active) {
          if (d.currentBalance > 0) {
            const monthlyRate = d.interestRate / 100 / 12;
            const interest = d.currentBalance * monthlyRate;
            d.currentBalance += interest;
            totalInterest += interest;
          }
        }

        // 2. Trả tối thiểu cho tất cả các khoản
        let freedUpCash = 0;
        for (const d of active) {
          if (d.currentBalance > 0) {
            const pay = Math.min(d.currentBalance, d.minPayment);
            d.currentBalance -= pay;
            if (d.currentBalance <= 0.01) {
              d.currentBalance = 0;
              clearOrder.push({ name: d.name, month });
              // Khi trả xong, tiền trả tối thiểu của khoản này được dồn sang khoản tiếp theo
              freedUpCash += d.minPayment - pay;
            }
          }
        }

        // 3. Dồn toàn bộ tiền trả thêm + tiền tối thiểu của các khoản đã trả xong vào khoản ưu tiên
        let availableExtra = extraPayment + freedUpCash;

        // Chọn khoản ưu tiên theo chiến lược
        const remainingDebts = active.filter((d) => d.currentBalance > 0.01);
        if (remainingDebts.length > 0) {
          if (strategy === "snowball") {
            // Sắp xếp dư nợ nhỏ nhất trước
            remainingDebts.sort((a, b) => a.currentBalance - b.currentBalance);
          } else {
            // Sắp xếp lãi suất cao nhất trước
            remainingDebts.sort((a, b) => b.interestRate - a.interestRate);
          }

          for (const target of remainingDebts) {
            if (availableExtra <= 0) break;
            const extraPay = Math.min(target.currentBalance, availableExtra);
            target.currentBalance -= extraPay;
            availableExtra -= extraPay;
            if (target.currentBalance <= 0.01) {
              target.currentBalance = 0;
              if (!clearOrder.some((c) => c.name === target.name)) {
                clearOrder.push({ name: target.name, month });
              }
            }
          }
        }
      }

      return {
        months: month,
        totalInterest: Math.round(totalInterest),
        clearOrder,
      };
    },
    [debts, extraPayment]
  );

  const snowballResult = useMemo(() => simulatePayoff("snowball"), [simulatePayoff]);
  const avalancheResult = useMemo(() => simulatePayoff("avalanche"), [simulatePayoff]);

  const interestDiff = Math.max(0, snowballResult.totalInterest - avalancheResult.totalInterest);
  const totalDebtBalance = debts.reduce((acc, d) => acc + d.balance, 0);

  const handleSaveToNotebook = async () => {
    setIsSaving(true);
    const md = `### 🎯 Lộ Trình Trả Nợ: Quả Cầu Tuyết vs Tuyết Lở

- **Tổng dư nợ hiện tại:** ${formatVND(totalDebtBalance)} (${debts.length} khoản nợ)
- **Số tiền trả thêm hàng tháng:** ${formatVND(extraPayment)}/tháng

#### So sánh 2 chiến lược:
1. 🏔️ **Tuyết lở (Debt Avalanche - Ưu tiên lãi cao):**
   - Thời gian sạch nợ: **${avalancheResult.months} tháng** (${(avalancheResult.months / 12).toFixed(1)} năm)
   - Tổng tiền lãi phải trả: **${formatVND(avalancheResult.totalInterest)}**
   - 💡 **Tiết kiệm thêm:** **${formatVND(interestDiff)}** tiền lãi so với Quả cầu tuyết.

2. ⛄ **Quả cầu tuyết (Debt Snowball - Ưu tiên dư nợ nhỏ):**
   - Thời gian sạch nợ: **${snowballResult.months} tháng**
   - Tổng tiền lãi phải trả: **${formatVND(snowballResult.totalInterest)}**
   - 🧠 **Lợi thế tâm lý:** Gạch bỏ khoản nợ đầu tiên nhanh nhất để tạo động lực kỷ luật dài hạn.

- **Thứ tự gạch nợ (Tuyết lở):** ${avalancheResult.clearOrder.map((c, i) => `${i + 1}. ${c.name} (tháng ${c.month})`).join(" ➔ ")}
`;
    try {
      await onSaveToNotebook(md);
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-4">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Scale className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            Lập Lộ Trình Trả Nợ (Quả Cầu Tuyết vs. Tuyết Lở)
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            So sánh giữa chiến thắng tâm lý (Snowball) và tối ưu toán học tài chính (Avalanche) để thoát nợ nhanh nhất.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={resetDefaultDebts}
            className="min-h-[44px] px-3 py-1.5 text-xs font-semibold rounded-lg bg-zinc-100 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-200 transition-colors flex items-center gap-1.5"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            Khôi phục 3 khoản mẫu
          </button>
          <button
            type="button"
            onClick={() => setShowAddForm(!showAddForm)}
            className="min-h-[44px] px-3 py-1.5 text-xs font-semibold rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 transition-colors flex items-center gap-1.5"
          >
            <Plus className="w-4 h-4" />
            Thêm khoản nợ mới
          </button>
        </div>
      </div>

      {/* Form thêm khoản nợ */}
      {showAddForm && (
        <div className="p-4 rounded-xl border border-indigo-200 dark:border-indigo-900 bg-indigo-50/50 dark:bg-indigo-950/20 space-y-3">
          <h3 className="text-xs font-bold text-indigo-900 dark:text-indigo-200 uppercase">
            Thêm Khoản Nợ Cần Trả
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
            <div>
              <label htmlFor="input-new-debt-name" className="text-[11px] font-medium text-zinc-700 dark:text-zinc-300">
                Tên khoản nợ
              </label>
              <input
                id="input-new-debt-name"
                type="text"
                value={newDebtName}
                placeholder="Vd: Vay tiêu dùng FE"
                onChange={(e) => setNewDebtName(e.target.value)}
                className="w-full mt-1 px-3 py-2 text-xs rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
              />
            </div>
            <div>
              <label htmlFor="input-new-debt-balance" className="text-[11px] font-medium text-zinc-700 dark:text-zinc-300">
                Dư nợ còn lại (VNĐ)
              </label>
              <input
                id="input-new-debt-balance"
                type="number"
                value={newDebtBalance}
                step={1_000_000}
                onChange={(e) => setNewDebtBalance(Number(e.target.value))}
                className="w-full mt-1 px-3 py-2 text-xs rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
              />
            </div>
            <div>
              <label htmlFor="input-new-debt-rate" className="text-[11px] font-medium text-zinc-700 dark:text-zinc-300">
                Lãi suất (%/năm)
              </label>
              <input
                id="input-new-debt-rate"
                type="number"
                value={newDebtRate}
                step={0.5}
                onChange={(e) => setNewDebtRate(Number(e.target.value))}
                className="w-full mt-1 px-3 py-2 text-xs rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
              />
            </div>
            <div>
              <label htmlFor="input-new-debt-min" className="text-[11px] font-medium text-zinc-700 dark:text-zinc-300">
                Trả tối thiểu/tháng
              </label>
              <input
                id="input-new-debt-min"
                type="number"
                value={newDebtMin}
                step={100_000}
                onChange={(e) => setNewDebtMin(Number(e.target.value))}
                className="w-full mt-1 px-3 py-2 text-xs rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
              />
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-1">
            <button
              type="button"
              onClick={() => setShowAddForm(false)}
              className="min-h-[44px] px-3 py-1.5 text-xs rounded-lg border border-zinc-300 dark:border-zinc-700 text-zinc-600 dark:text-zinc-400"
            >
              Hủy
            </button>
            <button
              type="button"
              onClick={addDebt}
              className="min-h-[44px] px-4 py-1.5 text-xs font-semibold rounded-lg bg-indigo-600 text-white hover:bg-indigo-700"
            >
              Lưu khoản nợ
            </button>
          </div>
        </div>
      )}

      {/* Grid danh sách nợ & so sánh kết quả */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Danh sách khoản nợ hiện có */}
        <div className="lg:col-span-5 space-y-4">
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-3">
            <div className="flex justify-between items-center text-xs">
              <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                Danh Sách Khoản Nợ ({debts.length})
              </span>
              <span className="font-bold text-rose-600 dark:text-rose-400">
                Tổng nợ: {formatVND(totalDebtBalance)}
              </span>
            </div>

            <div className="space-y-2">
              {debts.map((d) => (
                <div
                  key={d.id}
                  className="p-3 rounded-lg border border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-800/40 flex items-center justify-between gap-2"
                >
                  <div className="space-y-0.5">
                    <div className="font-semibold text-xs text-zinc-900 dark:text-zinc-100">{d.name}</div>
                    <div className="text-[11px] text-zinc-600 dark:text-zinc-400 flex flex-wrap gap-2">
                      <span>Dư nợ: <strong className="text-zinc-800 dark:text-zinc-200">{formatVND(d.balance)}</strong></span>
                      <span>·</span>
                      <span className="text-amber-600 dark:text-amber-400 font-semibold">Lãi: {d.interestRate}%/năm</span>
                      <span>·</span>
                      <span>Tối thiểu: {formatVND(d.minPayment)}/th</span>
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={() => removeDebt(d.id)}
                    className="p-2 text-zinc-400 hover:text-rose-600 transition-colors min-h-[44px] min-w-[44px] flex items-center justify-center"
                    title="Xóa khoản nợ này"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>

            {/* Điều chỉnh số tiền trả thêm hàng tháng */}
            <div className="pt-3 border-t border-zinc-200 dark:border-zinc-800 space-y-1.5">
              <div className="flex justify-between items-center text-xs">
                <label htmlFor="input-extra-payment" className="font-medium text-zinc-700 dark:text-zinc-300">
                  Số tiền có thể trả thêm mỗi tháng:
                </label>
                <span className="font-bold text-emerald-600 dark:text-emerald-400">
                  +{formatVND(extraPayment)}/tháng
                </span>
              </div>
              <input
                id="input-extra-payment"
                type="range"
                min={500_000}
                max={15_000_000}
                step={500_000}
                value={extraPayment}
                onChange={(e) => setExtraPayment(Number(e.target.value))}
                className="w-full accent-emerald-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
              />
              <p className="text-[10px] text-zinc-600 dark:text-zinc-400">
                Đây là khoản tiền sau khi bạn đã thắt chặt chi tiêu mong muốn để dồn tổng lực thanh toán dứt điểm nợ nần.
              </p>
            </div>
          </div>
        </div>

        {/* So sánh trực quan Snowball vs Avalanche */}
        <div className="lg:col-span-7 space-y-4">
          {/* Card so sánh 2 chiến lược */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {/* Avalanche */}
            <div className="rounded-xl border-2 border-indigo-500/40 bg-indigo-50/40 dark:bg-indigo-950/20 p-4 space-y-3 relative overflow-hidden">
              <div className="absolute top-2 right-2 px-2 py-0.5 rounded text-[10px] font-bold bg-indigo-600 text-white uppercase">
                Tiết kiệm tối đa
              </div>
              <div className="flex items-center gap-2 text-indigo-700 dark:text-indigo-300 font-bold text-sm">
                <Flame className="w-4 h-4 text-orange-500" />
                Tuyết Lở (Avalanche)
              </div>
              <p className="text-[11px] text-zinc-600 dark:text-zinc-400">
                Ưu tiên trả khoản nợ có <strong>lãi suất cao nhất</strong> trước để triệt tiêu tiền lãi ăn mòn.
              </p>

              <div className="space-y-1.5 pt-1 border-t border-indigo-200 dark:border-indigo-900">
                <div className="flex justify-between text-xs">
                  <span className="text-zinc-600 dark:text-zinc-400">Thời gian sạch nợ:</span>
                  <span className="font-bold text-zinc-900 dark:text-zinc-100">
                    {avalancheResult.months} tháng ({(avalancheResult.months / 12).toFixed(1)} năm)
                  </span>
                </div>
                <div className="flex justify-between text-xs">
                  <span className="text-zinc-600 dark:text-zinc-400">Tổng tiền lãi:</span>
                  <span className="font-extrabold text-indigo-600 dark:text-indigo-400">
                    {formatVND(avalancheResult.totalInterest)}
                  </span>
                </div>
              </div>
            </div>

            {/* Snowball */}
            <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-3 relative">
              <div className="absolute top-2 right-2 px-2 py-0.5 rounded text-[10px] font-bold bg-zinc-200 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300 uppercase">
                Thắng lợi tâm lý
              </div>
              <div className="flex items-center gap-2 text-zinc-900 dark:text-zinc-100 font-bold text-sm">
                <Snowflake className="w-4 h-4 text-blue-500" />
                Quả Cầu Tuyết (Snowball)
              </div>
              <p className="text-[11px] text-zinc-600 dark:text-zinc-400">
                Ưu tiên trả khoản nợ có <strong>số dư nhỏ nhất</strong> trước để nhanh gạch bỏ mục tiêu, tạo hưng phấn.
              </p>

              <div className="space-y-1.5 pt-1 border-t border-zinc-200 dark:border-zinc-800">
                <div className="flex justify-between text-xs">
                  <span className="text-zinc-600 dark:text-zinc-400">Thời gian sạch nợ:</span>
                  <span className="font-bold text-zinc-900 dark:text-zinc-100">
                    {snowballResult.months} tháng ({(snowballResult.months / 12).toFixed(1)} năm)
                  </span>
                </div>
                <div className="flex justify-between text-xs">
                  <span className="text-zinc-600 dark:text-zinc-400">Tổng tiền lãi:</span>
                  <span className="font-bold text-zinc-700 dark:text-zinc-300">
                    {formatVND(snowballResult.totalInterest)}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Banner kết quả so sánh chênh lệch tiền lãi */}
          <div className="p-4 rounded-xl border border-emerald-300 bg-emerald-50 dark:bg-emerald-950/40 dark:border-emerald-800 text-emerald-900 dark:text-emerald-100 space-y-1 text-xs">
            <div className="font-bold text-sm flex items-center gap-2 text-emerald-700 dark:text-emerald-300">
              <CheckCircle2 className="w-5 h-5 flex-shrink-0" />
              Chiến lược Tuyết Lở giúp bạn tiết kiệm được {formatVND(interestDiff)} tiền lãi!
            </div>
            <p className="text-zinc-700 dark:text-zinc-300 text-[11px]">
              Nếu bạn là người kỷ luật và muốn bảo vệ tiền túi tối đa: hãy chọn <strong>Tuyết lở</strong>.
              Nếu bạn dễ nản chí và cần cảm giác chiến thắng từng chặng nhỏ: hãy chọn <strong>Quả cầu tuyết</strong>.
            </p>
          </div>

          {/* Thứ tự trả nợ chi tiết */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-2">
            <h4 className="text-xs font-bold text-zinc-900 dark:text-zinc-100 uppercase tracking-wider">
              Lộ Trình Gạch Nợ Từng Bước (Theo Tuyết Lở)
            </h4>
            <div className="space-y-1.5 text-xs">
              {avalancheResult.clearOrder.map((item, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-2 rounded-lg bg-zinc-50 dark:bg-zinc-800/40"
                >
                  <div className="flex items-center gap-2">
                    <span className="w-5 h-5 rounded-full bg-indigo-600 text-white font-bold text-[10px] flex items-center justify-center">
                      {index + 1}
                    </span>
                    <span className="font-medium text-zinc-800 dark:text-zinc-200">{item.name}</span>
                  </div>
                  <span className="text-emerald-600 dark:text-emerald-400 font-semibold text-[11px]">
                    Sạch nợ ở tháng thứ {item.month}
                  </span>
                </div>
              ))}
            </div>
          </div>

          {/* Nút lưu Sổ tay */}
          <button
            type="button"
            disabled={isSaving}
            onClick={handleSaveToNotebook}
            className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-indigo-600 text-white font-semibold text-xs hover:bg-indigo-700 active:scale-[0.99] transition-all flex items-center justify-center gap-2 shadow-sm disabled:opacity-50"
          >
            {isSaving ? (
              <Activity className="w-4 h-4 animate-spin" />
            ) : saveSuccess ? (
              <CheckCircle2 className="w-4 h-4 text-emerald-300" />
            ) : (
              <BookOpen className="w-4 h-4" />
            )}
            {saveSuccess ? "Đã lưu lộ trình vào Sổ tay!" : "Lưu kế hoạch trả nợ vào Sổ tay"}
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 4: PHÒNG TẬP ĐỌC BIỂU ĐỒ NẾN & QUẢN TRỊ RỦI RO ĐẦU TƯ
// =============================================================================
interface Candle {
  label: string;
  o: number;
  h: number;
  l: number;
  c: number;
  v: number;
}

interface TradingScenario {
  id: string;
  title: string;
  ticker: string;
  timeframe: string;
  context: string;
  supportZone: [number, number];
  resistanceZone: [number, number];
  defaultEntry: number;
  defaultSl: number;
  defaultTp: number;
  bestAction: "long" | "short" | "wait";
  explanation: string;
  initialCandles: Candle[];
  futureCandles: Candle[];
}

const TRADING_SCENARIOS: TradingScenario[] = [
  {
    id: "pinbar-support",
    title: "Nến rút chân (Bullish Pin Bar) tại vùng Hỗ trợ cứng",
    ticker: "VN30 / BTC-USDT",
    timeframe: "H4",
    context:
      "Giá điều chỉnh 6 phiên liên tiếp về đúng vùng cầu mạnh 98.0 – 100.0 (đáy cũ tháng trước). Ở nến số 16 xuất hiện nến rút chân dưới dài với khối lượng giao dịch tăng vọt, cho thấy phe mua hấp thụ mạnh lực bán.",
    supportZone: [97.5, 100.5],
    resistanceZone: [111.0, 114.0],
    defaultEntry: 101.5,
    defaultSl: 97.0,
    defaultTp: 111.5,
    bestAction: "long",
    explanation:
      "Cây nến rút chân (Pin Bar) với râu dưới dài > 2x thân nến đóng cửa xanh ngay trên vùng hỗ trợ kèm Volume đột biến là tín hiệu từ chối giảm giá kinh điển. Đặt Stop Loss an toàn dưới râu nến (97.0) và Take Profit trước vùng kháng cự (111.5) cho tỷ lệ R:R ≈ 2.22.",
    initialCandles: [
      { label: "1", o: 112.0, h: 113.2, l: 110.4, c: 110.8, v: 42 },
      { label: "2", o: 110.8, h: 111.5, l: 109.1, c: 109.6, v: 38 },
      { label: "3", o: 109.6, h: 110.9, l: 108.8, c: 110.2, v: 35 },
      { label: "4", o: 110.2, h: 111.8, l: 109.5, c: 111.4, v: 46 },
      { label: "5", o: 111.4, h: 112.6, l: 109.8, c: 110.0, v: 50 },
      { label: "6", o: 110.0, h: 110.5, l: 107.8, c: 108.2, v: 56 },
      { label: "7", o: 108.2, h: 108.9, l: 106.5, c: 107.0, v: 48 },
      { label: "8", o: 107.0, h: 108.4, l: 106.2, c: 107.9, v: 39 },
      { label: "9", o: 107.9, h: 108.2, l: 105.0, c: 105.4, v: 53 },
      { label: "10", o: 105.4, h: 106.1, l: 103.6, c: 104.1, v: 61 },
      { label: "11", o: 104.1, h: 104.8, l: 102.5, c: 103.0, v: 49 },
      { label: "12", o: 103.0, h: 104.2, l: 101.8, c: 103.6, v: 44 },
      { label: "13", o: 103.6, h: 103.9, l: 101.2, c: 101.8, v: 52 },
      { label: "14", o: 101.8, h: 102.4, l: 100.1, c: 100.6, v: 58 },
      { label: "15", o: 100.6, h: 101.3, l: 98.9, c: 99.6, v: 67 },
      { label: "16", o: 99.6, h: 102.0, l: 97.4, c: 101.6, v: 94 },
    ],
    futureCandles: [
      { label: "17", o: 101.6, h: 104.5, l: 101.1, c: 104.0, v: 82 },
      { label: "18", o: 104.0, h: 106.8, l: 103.2, c: 106.3, v: 75 },
      { label: "19", o: 106.3, h: 107.5, l: 105.1, c: 105.8, v: 54 },
      { label: "20", o: 105.8, h: 109.9, l: 105.4, c: 109.4, v: 86 },
      { label: "21", o: 109.4, h: 112.4, l: 108.9, c: 111.8, v: 91 },
    ],
  },
  {
    id: "bull-trap",
    title: "Bẫy tăng giá (Bull Trap / False Breakout) tại Kháng cự",
    ticker: "FPT / ETH-USDT",
    timeframe: "D1",
    context:
      "Sau nhịp tăng mạnh, giá chọc thủng đỉnh kháng cự 118.0 ở nến 15 nhưng ngay lập tức xuất hiện nến nhấn chìm giảm (Bearish Engulfing) đóng cửa lọt ngược lại dưới kháng cự với thanh khoản bán lớn.",
    supportZone: [104.0, 106.5],
    resistanceZone: [117.5, 120.0],
    defaultEntry: 116.5,
    defaultSl: 121.5,
    defaultTp: 106.5,
    bestAction: "short",
    explanation:
      "Phá vỡ giả (Bull Trap) bẫy các lệnh mua đuổi ở đỉnh. Khi nến sau nhấn chìm hoàn toàn thân nến breakout và đóng dưới vùng 117.5, động lượng đảo chiều rất mạnh. Ưu tiên bán/đóng vị thế Long, chặn lỗ trên râu nến 121.5.",
    initialCandles: [
      { label: "1", o: 105.0, h: 107.0, l: 104.4, c: 106.5, v: 45 },
      { label: "2", o: 106.5, h: 108.5, l: 106.0, c: 108.1, v: 50 },
      { label: "3", o: 108.1, h: 109.2, l: 107.0, c: 107.5, v: 41 },
      { label: "4", o: 107.5, h: 110.4, l: 107.2, c: 109.9, v: 58 },
      { label: "5", o: 109.9, h: 112.2, l: 109.4, c: 111.6, v: 64 },
      { label: "6", o: 111.6, h: 113.0, l: 110.8, c: 111.2, v: 47 },
      { label: "7", o: 111.2, h: 114.5, l: 110.9, c: 114.0, v: 69 },
      { label: "8", o: 114.0, h: 116.8, l: 113.5, c: 116.1, v: 74 },
      { label: "9", o: 116.1, h: 118.4, l: 115.6, c: 117.3, v: 80 },
      { label: "10", o: 117.3, h: 118.2, l: 115.0, c: 115.6, v: 62 },
      { label: "11", o: 115.6, h: 117.1, l: 114.8, c: 116.8, v: 55 },
      { label: "12", o: 116.8, h: 118.5, l: 116.0, c: 117.6, v: 66 },
      { label: "13", o: 117.6, h: 118.9, l: 116.2, c: 116.9, v: 59 },
      { label: "14", o: 116.9, h: 118.6, l: 116.4, c: 118.1, v: 70 },
      { label: "15", o: 118.1, h: 121.0, l: 117.9, c: 119.5, v: 63 },
      { label: "16", o: 119.5, h: 120.2, l: 115.8, c: 116.2, v: 95 },
    ],
    futureCandles: [
      { label: "17", o: 116.2, h: 117.0, l: 113.2, c: 113.8, v: 88 },
      { label: "18", o: 113.8, h: 114.5, l: 111.0, c: 111.4, v: 79 },
      { label: "19", o: 111.4, h: 112.6, l: 109.5, c: 110.2, v: 65 },
      { label: "20", o: 110.2, h: 110.9, l: 107.1, c: 107.5, v: 83 },
      { label: "21", o: 107.5, h: 108.1, l: 105.6, c: 106.2, v: 90 },
    ],
  },
  {
    id: "volume-breakout",
    title: "Phá vỡ kháng cự bùng nổ khối lượng (Breakout with Volume)",
    ticker: "HPG / SSI",
    timeframe: "D1",
    context:
      "Cổ phiếu tích lũy nền giá chặt chẽ trong hơn 2 tuần quanh vùng 28.0. Ở phiên số 16, một cây nến Marubozu xanh đặc đâm thủng dứt khoát đỉnh kháng cự với thanh khoản gấp 2.8 lần trung bình 20 phiên.",
    supportZone: [26.8, 27.5],
    resistanceZone: [28.2, 28.8],
    defaultEntry: 29.0,
    defaultSl: 27.8,
    defaultTp: 32.0,
    bestAction: "long",
    explanation:
      "Breakout chuẩn chỉ với sự đồng thuận hoàn hảo giữa Giá và Khối lượng (Price & Volume). Khi dòng tiền tổ chức hấp thụ toàn bộ lượng cung ở đỉnh, xác suất tăng tiếp là rất cao. Đặt SL ngay dưới kháng cự cũ vừa biến thành hỗ trợ mới (27.8).",
    initialCandles: [
      { label: "1", o: 27.2, h: 27.6, l: 27.0, c: 27.4, v: 30 },
      { label: "2", o: 27.4, h: 27.8, l: 27.2, c: 27.5, v: 32 },
      { label: "3", o: 27.5, h: 27.9, l: 27.3, c: 27.7, v: 28 },
      { label: "4", o: 27.7, h: 28.1, l: 27.5, c: 27.8, v: 35 },
      { label: "5", o: 27.8, h: 28.2, l: 27.6, c: 27.9, v: 33 },
      { label: "6", o: 27.9, h: 28.3, l: 27.7, c: 28.0, v: 38 },
      { label: "7", o: 28.0, h: 28.4, l: 27.8, c: 28.1, v: 40 },
      { label: "8", o: 28.1, h: 28.3, l: 27.9, c: 28.0, v: 26 },
      { label: "9", o: 28.0, h: 28.2, l: 27.7, c: 27.9, v: 25 },
      { label: "10", o: 27.9, h: 28.1, l: 27.6, c: 27.8, v: 29 },
      { label: "11", o: 27.8, h: 28.2, l: 27.7, c: 28.0, v: 31 },
      { label: "12", o: 28.0, h: 28.3, l: 27.8, c: 28.1, v: 34 },
      { label: "13", o: 28.1, h: 28.4, l: 27.9, c: 28.2, v: 37 },
      { label: "14", o: 28.2, h: 28.5, l: 28.0, c: 28.3, v: 42 },
      { label: "15", o: 28.3, h: 28.6, l: 28.1, c: 28.4, v: 48 },
      { label: "16", o: 28.4, h: 29.5, l: 28.3, c: 29.3, v: 110 },
    ],
    futureCandles: [
      { label: "17", o: 29.3, h: 30.2, l: 29.0, c: 29.9, v: 95 },
      { label: "18", o: 29.9, h: 30.8, l: 29.6, c: 30.5, v: 88 },
      { label: "19", o: 30.5, h: 31.2, l: 30.1, c: 30.9, v: 80 },
      { label: "20", o: 30.9, h: 31.8, l: 30.6, c: 31.5, v: 84 },
      { label: "21", o: 31.5, h: 32.5, l: 31.2, c: 32.2, v: 92 },
    ],
  },
  {
    id: "choppy-wait",
    title: "Vùng đi ngang nhiễu loạn (Choppy Sideways) — Kiên nhẫn đứng ngoài",
    ticker: "MWG / VHM",
    timeframe: "H1",
    context:
      "Giá dao động giằng co giữa biên 100 – 106 với hàng loạt nến Doji râu hai đầu, khối lượng sụt giảm và không có xu hướng rõ ràng. Đặt lệnh giữa vùng nhiễu rất dễ bị quét cắt lỗ cả hai đầu.",
    supportZone: [99.0, 100.5],
    resistanceZone: [106.0, 107.5],
    defaultEntry: 103.0,
    defaultSl: 100.8,
    defaultTp: 106.2,
    bestAction: "wait",
    explanation:
      "Đứng ngoài cũng là một quyết định đầu tư khôn ngoan! Giữa biên sideways, khoảng cách tới kháng cự gần nhưng dễ bị quét SL hai đầu, tỷ lệ R:R kém hấp dẫn (< 1.5) và không có lợi thế xác suất. Hãy kiên nhẫn đợi thị trường phá vỡ biên rõ ràng.",
    initialCandles: [
      { label: "1", o: 102.0, h: 104.2, l: 101.2, c: 103.4, v: 42 },
      { label: "2", o: 103.4, h: 105.0, l: 102.1, c: 102.4, v: 39 },
      { label: "3", o: 102.4, h: 104.8, l: 101.5, c: 104.1, v: 44 },
      { label: "4", o: 104.1, h: 105.2, l: 102.0, c: 102.6, v: 36 },
      { label: "5", o: 102.6, h: 103.9, l: 100.9, c: 101.4, v: 35 },
      { label: "6", o: 101.4, h: 104.4, l: 101.0, c: 103.8, v: 40 },
      { label: "7", o: 103.8, h: 104.9, l: 101.9, c: 102.2, v: 34 },
      { label: "8", o: 102.2, h: 104.1, l: 101.2, c: 103.5, v: 31 },
      { label: "9", o: 103.5, h: 104.7, l: 101.8, c: 102.7, v: 33 },
      { label: "10", o: 102.7, h: 104.6, l: 101.4, c: 103.9, v: 37 },
      { label: "11", o: 103.9, h: 105.1, l: 102.2, c: 102.5, v: 29 },
      { label: "12", o: 102.5, h: 104.0, l: 101.1, c: 103.3, v: 28 },
      { label: "13", o: 103.3, h: 104.5, l: 101.7, c: 102.8, v: 30 },
      { label: "14", o: 102.8, h: 104.2, l: 101.4, c: 103.4, v: 27 },
      { label: "15", o: 103.4, h: 104.8, l: 101.8, c: 102.9, v: 29 },
      { label: "16", o: 102.9, h: 104.1, l: 101.9, c: 103.1, v: 25 },
    ],
    futureCandles: [
      { label: "17", o: 103.1, h: 105.0, l: 100.6, c: 102.4, v: 41 },
      { label: "18", o: 102.4, h: 104.6, l: 100.9, c: 103.8, v: 35 },
      { label: "19", o: 103.8, h: 104.9, l: 101.3, c: 102.0, v: 38 },
      { label: "20", o: 102.0, h: 104.5, l: 101.5, c: 103.6, v: 32 },
      { label: "21", o: 103.6, h: 104.7, l: 101.7, c: 102.8, v: 30 },
    ],
  },
];

function CandlestickRiskLab({
  onSaveToNotebook,
  onCreateFlashcardQuick,
}: {
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}) {
  const [activeScenarioId, setActiveScenarioId] = useState<string>("pinbar-support");
  const scenario = TRADING_SCENARIOS.find((s) => s.id === activeScenarioId) || TRADING_SCENARIOS[0];

  // Quyết định của người học
  const [userAction, setUserAction] = useState<"long" | "short" | "wait">("long");
  const [isRevealed, setIsRevealed] = useState<boolean>(false);

  // Quản trị rủi ro & Position Sizing
  const [portfolioCapital, setPortfolioCapital] = useState<number>(100_000_000); // 100 triệu VNĐ
  const [riskPercent, setRiskPercent] = useState<number>(2); // 2% rủi ro tối đa mỗi lệnh
  const [entryPrice, setEntryPrice] = useState<number>(scenario.defaultEntry);
  const [stopLoss, setStopLoss] = useState<number>(scenario.defaultSl);
  const [takeProfit, setTakeProfit] = useState<number>(scenario.defaultTp);

  // Câu hỏi Mini Quiz chỉ số tài chính
  const [quizAnswers, setQuizAnswers] = useState<{ [key: string]: number | null }>({
    pe: null,
    roe: null,
    de: null,
  });

  const [isSaving, setIsSaving] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  // Cập nhật giá mặc định khi đổi kịch bản
  const handleSelectScenario = (id: string) => {
    setActiveScenarioId(id);
    setIsRevealed(false);
    const target = TRADING_SCENARIOS.find((s) => s.id === id);
    if (target) {
      setEntryPrice(target.defaultEntry);
      setStopLoss(target.defaultSl);
      setTakeProfit(target.defaultTp);
      setUserAction(target.bestAction === "short" ? "short" : target.bestAction === "wait" ? "wait" : "long");
    }
  };

  // Tính toán Risk Management
  const riskAmount = portfolioCapital * (riskPercent / 100);
  const riskPerUnit = Math.abs(entryPrice - stopLoss);
  const rewardPerUnit = Math.abs(takeProfit - entryPrice);
  const rrRatio = riskPerUnit > 0 ? rewardPerUnit / riskPerUnit : 0;
  const maxUnits = riskPerUnit > 0 ? Math.floor(riskAmount / (riskPerUnit * 1000)) : 0; // quy đổi đơn vị cổ phiếu (giả sử đơn vị nghìn đồng)
  const totalPositionValue = maxUnits * entryPrice * 1000;

  // Nến hiển thị (16 nến đầu hoặc thêm 5 nến tương lai)
  const visibleCandles = isRevealed
    ? [...scenario.initialCandles, ...scenario.futureCandles]
    : scenario.initialCandles;

  // Tính toán bounds cho biểu đồ SVG
  const allPrices = visibleCandles.flatMap((c) => [c.h, c.l]);
  if (userAction !== "wait") {
    allPrices.push(entryPrice, stopLoss, takeProfit);
  }
  const minP = Math.min(...allPrices) * 0.985;
  const maxP = Math.max(...allPrices) * 1.015;
  const priceSpan = Math.max(1, maxP - minP);

  const maxVol = Math.max(1, ...visibleCandles.map((c) => c.v));

  const svgW = 720;
  const svgH = 280;
  const padL = 20;
  const padR = 60;
  const padT = 16;
  const padB = 40;
  const plotW = svgW - padL - padR;
  const plotH = svgH - padT - padB;

  const priceToY = (p: number) => padT + ((maxP - p) / priceSpan) * (plotH - 30);

  const isDecisionCorrect = userAction === scenario.bestAction;

  const handleSaveToNotebook = async () => {
    setIsSaving(true);
    const md = `### 📊 Phòng Tập Đọc Biểu Đồ & Quản Trị Rủi Ro Đầu Tư

- **Kịch bản phân tích:** ${scenario.title} (${scenario.ticker} - ${scenario.timeframe})
- **Quyết định vào lệnh:** **${userAction.toUpperCase()}** (Khuyến nghị kỹ thuật: **${scenario.bestAction.toUpperCase()}**)
- **Kết quả đánh giá:** ${isDecisionCorrect ? "✅ CHÍNH XÁC" : "⚠️ CẦN RÚT KINH NGHIỆM"}
- **Giải thích thị trường:** ${scenario.explanation}

#### Nguyên tắc Quản trị rủi ro & Vị thế:
- **Tổng vốn danh mục:** ${formatVND(portfolioCapital)}
- **Rủi ro tối đa cho phép/lệnh (2%):** ${formatVND(riskAmount)}
- **Tỷ lệ Lợi nhuận/Rủi ro (R:R):** 1 : **${rrRatio.toFixed(2)}** ${rrRatio >= 2.0 ? "(Chuẩn vàng >= 2.0)" : "(Thấp)"}
- **Vị thế tối đa cho phép mua:** **${maxUnits.toLocaleString("vi-VN")} cổ phiếu** (Giá trị vị thế: ${formatVND(totalPositionValue)})
- *Cam kết kỷ luật:* Nếu chạm Stop Loss (${stopLoss}), bạn chỉ mất đúng ${formatVND(riskAmount)} (2% vốn), bảo toàn vốn qua mọi chu kỳ suy thoái.
`;
    try {
      await onSaveToNotebook(md);
      setFeedback("Đã lưu phân tích quản trị rủi ro vào Sổ tay!");
      setTimeout(() => setFeedback(null), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCreateFlashcards = async () => {
    if (!onCreateFlashcardQuick) return;
    try {
      await onCreateFlashcardQuick(
        "Tỷ lệ Lợi nhuận / Rủi ro (Risk:Reward Ratio - R:R) tối thiểu nên là bao nhiêu trước khi vào lệnh?",
        "Tỷ lệ R:R khuyến nghị tối thiểu nên từ 1:2 trở lên.\n\nÝ nghĩa: Khoản lợi nhuận tiềm năng (tới Take-Profit) phải gấp ít nhất 2 lần khoản lỗ tối đa nếu chạm Stop-Loss. Khi đó, ngay cả khi bạn chỉ đúng 40% số lệnh, tài khoản vẫn tăng trưởng dương dài hạn."
      );
      setFeedback("Đã tạo Flashcard Quản trị Rủi ro R:R thành công!");
      setTimeout(() => setFeedback(null), 3000);
    } catch {
      // ignore
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-4">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <CandlestickChart className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            Phòng Tập Đọc Biểu Đồ Nến & Quản Trị Rủi Ro Đầu Tư
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Luyện đọc hành động giá (Price Action), tính Position Sizing kỷ luật và kiểm tra 3 chỉ số tài chính cốt lõi (P/E, ROE, D/E).
          </p>
        </div>
        {onCreateFlashcardQuick && (
          <button
            type="button"
            onClick={handleCreateFlashcards}
            className="min-h-[44px] px-3 py-1.5 text-xs font-semibold rounded-lg bg-indigo-50 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800 hover:bg-indigo-100 transition-colors flex items-center gap-1.5"
          >
            <Sparkles className="w-4 h-4" />
            Tạo Flashcard R:R & Chỉ Số Tài Chính
          </button>
        )}
      </div>

      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
          <span>{feedback}</span>
        </div>
      )}

      {/* Thanh chọn 4 kịch bản */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-2.5">
        {TRADING_SCENARIOS.map((s) => {
          const isActive = s.id === activeScenarioId;
          return (
            <button
              key={s.id}
              type="button"
              onClick={() => handleSelectScenario(s.id)}
              className={`p-3 text-left rounded-xl border transition-all min-h-[44px] flex flex-col justify-between ${
                isActive
                  ? "border-indigo-600 bg-indigo-50/60 dark:bg-indigo-950/40 text-indigo-900 dark:text-indigo-200 shadow-sm"
                  : "border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 text-zinc-700 dark:text-zinc-300 hover:border-zinc-300"
              }`}
            >
              <div className="font-bold text-xs truncate">{s.title}</div>
              <div className="text-[10px] text-zinc-600 dark:text-zinc-400 mt-1 flex items-center justify-between">
                <span>{s.ticker}</span>
                <span className="font-mono">{s.timeframe}</span>
              </div>
            </button>
          );
        })}
      </div>

      {/* Grid biểu đồ & Bảng tính vị thế */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Biểu đồ nến SVG */}
        <div className="lg:col-span-7 space-y-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-zinc-100 dark:border-zinc-800 pb-3">
            <div>
              <div className="text-xs font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
                <span>{scenario.title}</span>
                <span className="px-1.5 py-0.5 rounded text-[10px] bg-zinc-100 dark:bg-zinc-800 font-mono">
                  {scenario.ticker} · {scenario.timeframe}
                </span>
              </div>
              <p className="text-[11px] text-zinc-600 dark:text-zinc-400 mt-1">{scenario.context}</p>
            </div>
          </div>

          {/* SVG Candlestick Chart Canvas */}
          <div className="w-full bg-zinc-950 rounded-lg p-2 overflow-hidden border border-zinc-800">
            <svg viewBox={`0 0 ${svgW} ${svgH}`} className="w-full h-auto block select-none">
              {/* Resistance Zone (Vùng kháng cự - Đỏ mờ) */}
              <rect
                x={padL}
                y={priceToY(scenario.resistanceZone[1])}
                width={plotW}
                height={Math.max(4, priceToY(scenario.resistanceZone[0]) - priceToY(scenario.resistanceZone[1]))}
                fill="#ef4444"
                opacity={0.15}
              />
              <text
                x={padL + 6}
                y={priceToY(scenario.resistanceZone[1]) + 11}
                fontSize="9"
                fill="#f87171"
                fontWeight="600"
              >
                Kháng cự ({scenario.resistanceZone[0]} - {scenario.resistanceZone[1]})
              </text>

              {/* Support Zone (Vùng hỗ trợ - Xanh mờ) */}
              <rect
                x={padL}
                y={priceToY(scenario.supportZone[1])}
                width={plotW}
                height={Math.max(4, priceToY(scenario.supportZone[0]) - priceToY(scenario.supportZone[1]))}
                fill="#10b981"
                opacity={0.15}
              />
              <text
                x={padL + 6}
                y={priceToY(scenario.supportZone[0]) - 3}
                fontSize="9"
                fill="#34d399"
                fontWeight="600"
              >
                Hỗ trợ ({scenario.supportZone[0]} - {scenario.supportZone[1]})
              </text>

              {/* Đường kẻ Entry, Stop-Loss, Take-Profit */}
              {userAction !== "wait" && (
                <g>
                  {/* Đường Entry */}
                  <line
                    x1={padL}
                    y1={priceToY(entryPrice)}
                    x2={svgW - padR}
                    y2={priceToY(entryPrice)}
                    stroke="#3b82f6"
                    strokeWidth="1.2"
                    strokeDasharray="4 3"
                  />
                  <text x={svgW - padR + 4} y={priceToY(entryPrice) + 3} fontSize="9" fill="#60a5fa" fontWeight="bold">
                    Entry: {entryPrice}
                  </text>

                  {/* Đường Stop-Loss */}
                  <line
                    x1={padL}
                    y1={priceToY(stopLoss)}
                    x2={svgW - padR}
                    y2={priceToY(stopLoss)}
                    stroke="#ef4444"
                    strokeWidth="1.2"
                    strokeDasharray="4 3"
                  />
                  <text x={svgW - padR + 4} y={priceToY(stopLoss) + 3} fontSize="9" fill="#f87171" fontWeight="bold">
                    SL: {stopLoss}
                  </text>

                  {/* Đường Take-Profit */}
                  <line
                    x1={padL}
                    y1={priceToY(takeProfit)}
                    x2={svgW - padR}
                    y2={priceToY(takeProfit)}
                    stroke="#10b981"
                    strokeWidth="1.2"
                    strokeDasharray="4 3"
                  />
                  <text x={svgW - padR + 4} y={priceToY(takeProfit) + 3} fontSize="9" fill="#34d399" fontWeight="bold">
                    TP: {takeProfit}
                  </text>
                </g>
              )}

              {/* Đường ngăn cách giữa nến lịch sử và nến tương lai khi mở */}
              {isRevealed && (
                <line
                  x1={padL + (16 / visibleCandles.length) * plotW}
                  y1={padT}
                  x2={padL + (16 / visibleCandles.length) * plotW}
                  y2={svgH - 24}
                  stroke="#a855f7"
                  strokeWidth="1.2"
                  strokeDasharray="3 3"
                />
              )}

              {/* Render nến & Khối lượng */}
              {visibleCandles.map((c, i) => {
                const candleStep = plotW / visibleCandles.length;
                const cx = padL + i * candleStep + candleStep / 2;
                const bodyW = Math.max(3, candleStep * 0.65);

                const isGreen = c.c >= c.o;
                const candleColor = isGreen ? "#10b981" : "#ef4444";

                const topY = priceToY(Math.max(c.o, c.c));
                const bottomY = priceToY(Math.min(c.o, c.c));
                const bodyH = Math.max(2, bottomY - topY);

                const highY = priceToY(c.h);
                const lowY = priceToY(c.l);

                // Khối lượng Volume
                const volH = (c.v / maxVol) * 36;

                return (
                  <g key={i}>
                    {/* Râu nến (Wick) */}
                    <line x1={cx} y1={highY} x2={cx} y2={lowY} stroke={candleColor} strokeWidth="1.2" />

                    {/* Thân nến (Body) */}
                    <rect
                      x={cx - bodyW / 2}
                      y={topY}
                      width={bodyW}
                      height={bodyH}
                      fill={candleColor}
                      rx="1"
                    />

                    {/* Volume Bar phía dưới */}
                    <rect
                      x={cx - bodyW / 2}
                      y={svgH - 26 - volH}
                      width={bodyW}
                      height={volH}
                      fill={candleColor}
                      opacity={0.35}
                    />

                    {/* Nhãn phiên */}
                    {(i === 0 || i === 15 || i === visibleCandles.length - 1) && (
                      <text
                        x={cx}
                        y={svgH - 10}
                        fontSize="8.5"
                        textAnchor="middle"
                        fill="#71717a"
                        fontFamily="monospace"
                      >
                        P{c.label}
                      </text>
                    )}
                  </g>
                );
              })}
            </svg>
          </div>

          {/* Điều khiển hành động: Mua / Bán / Đứng ngoài & Mở 5 phiên tới */}
          <div className="space-y-3 pt-2">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex items-center gap-2">
                <span className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">
                  Quyết định của bạn:
                </span>
                <div className="flex rounded-lg border border-zinc-300 dark:border-zinc-700 p-0.5 bg-zinc-100 dark:bg-zinc-800">
                  <button
                    type="button"
                    onClick={() => setUserAction("long")}
                    className={`min-h-[44px] px-3 py-1 text-xs font-bold rounded-md transition-all ${
                      userAction === "long"
                        ? "bg-emerald-600 text-white shadow-sm"
                        : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900"
                    }`}
                  >
                    MUA (Long)
                  </button>
                  <button
                    type="button"
                    onClick={() => setUserAction("short")}
                    className={`min-h-[44px] px-3 py-1 text-xs font-bold rounded-md transition-all ${
                      userAction === "short"
                        ? "bg-rose-600 text-white shadow-sm"
                        : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900"
                    }`}
                  >
                    BÁN (Short)
                  </button>
                  <button
                    type="button"
                    onClick={() => setUserAction("wait")}
                    className={`min-h-[44px] px-3 py-1 text-xs font-bold rounded-md transition-all ${
                      userAction === "wait"
                        ? "bg-zinc-600 text-white shadow-sm"
                        : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900"
                    }`}
                  >
                    ĐỨNG NGOÀI (Wait)
                  </button>
                </div>
              </div>

              <button
                type="button"
                data-testid="practice-finance-reveal-btn"
                onClick={() => setIsRevealed(!isRevealed)}
                className={`min-h-[44px] px-4 py-2 rounded-xl text-xs font-bold transition-all flex items-center gap-2 ${
                  isRevealed
                    ? "bg-zinc-200 dark:bg-zinc-800 text-zinc-700 dark:text-zinc-300"
                    : "bg-indigo-600 text-white hover:bg-indigo-700 shadow-sm"
                }`}
              >
                <ArrowRight className="w-4 h-4" />
                {isRevealed ? "Ẩn 5 phiên tương lai" : "Chạy tiếp 5 phiên tới"}
              </button>
            </div>

            {/* Phân tích kết quả sau khi bấm "Chạy tiếp 5 phiên tới" */}
            {isRevealed && (
              <div
                data-testid="practice-finance-result"
                className={`p-4 rounded-xl border space-y-2 text-xs transition-all ${
                  isDecisionCorrect
                    ? "border-emerald-300 bg-emerald-50 dark:bg-emerald-950/40 dark:border-emerald-800 text-emerald-900 dark:text-emerald-100"
                    : "border-amber-300 bg-amber-50 dark:bg-amber-950/40 dark:border-amber-800 text-amber-900 dark:text-amber-100"
                }`}
              >
                <div className="font-bold text-sm flex items-center gap-2">
                  {isDecisionCorrect ? (
                    <>
                      <CheckCircle2 className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                      <span>XUẤT SẮC! Quyết định của bạn hoàn toàn chuẩn xác.</span>
                    </>
                  ) : (
                    <>
                      <AlertTriangle className="w-5 h-5 text-amber-600 dark:text-amber-400" />
                      <span>CHƯA TỐI ƯU: Kịch bản này nên ưu tiên {scenario.bestAction.toUpperCase()}.</span>
                    </>
                  )}
                </div>
                <p className="text-zinc-700 dark:text-zinc-300 leading-relaxed">{scenario.explanation}</p>
              </div>
            )}
          </div>
        </div>

        {/* Quản trị rủi ro & Position Sizing Calculator */}
        <div className="lg:col-span-5 space-y-4">
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-900/40 p-4 space-y-4">
            <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
              <ShieldAlert className="w-4 h-4 text-indigo-500" />
              Bộ Tính Vị Thế & Quản Trị Rủi Ro (Position Sizing)
            </h3>

            {/* Tổng vốn */}
            <div className="space-y-1">
              <div className="flex justify-between items-center text-xs">
                <label htmlFor="input-portfolio-capital" className="font-medium text-zinc-700 dark:text-zinc-300">
                  Tổng vốn đầu tư (VNĐ)
                </label>
                <span className="font-bold text-zinc-900 dark:text-zinc-100">{formatVND(portfolioCapital)}</span>
              </div>
              <input
                id="input-portfolio-capital"
                type="range"
                min={20_000_000}
                max={500_000_000}
                step={10_000_000}
                value={portfolioCapital}
                onChange={(e) => setPortfolioCapital(Number(e.target.value))}
                className="w-full accent-indigo-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
              />
            </div>

            {/* % Rủi ro tối đa */}
            <div className="space-y-1 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <div className="flex justify-between items-center text-xs">
                <label htmlFor="input-risk-percent" className="font-medium text-zinc-700 dark:text-zinc-300">
                  Rủi ro tối đa mỗi lệnh (%)
                </label>
                <span className="font-bold text-rose-600 dark:text-rose-400">{riskPercent}% (= {formatVND(riskAmount)})</span>
              </div>
              <input
                id="input-risk-percent"
                type="range"
                min={0.5}
                max={5}
                step={0.5}
                value={riskPercent}
                onChange={(e) => setRiskPercent(Number(e.target.value))}
                className="w-full accent-rose-600 cursor-pointer h-2 bg-zinc-200 dark:bg-zinc-700 rounded-lg min-h-[44px]"
              />
              <span className="text-[10px] text-zinc-600 dark:text-zinc-400">
                Quy tắc 2%: Không bao giờ mạo hiểm quá 2% tổng tài sản cho một quyết định giao dịch duy nhất.
              </span>
            </div>

            {/* Giá Entry / SL / TP */}
            <div className="grid grid-cols-3 gap-2 pt-2 border-t border-zinc-200 dark:border-zinc-800">
              <div>
                <label htmlFor="input-entry-price" className="text-[10px] font-semibold text-blue-600">
                  Giá Vào (Entry)
                </label>
                <input
                  id="input-entry-price"
                  type="number"
                  step={0.1}
                  value={entryPrice}
                  onChange={(e) => setEntryPrice(Number(e.target.value))}
                  className="w-full mt-0.5 px-2 py-1.5 text-xs rounded border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
                />
              </div>
              <div>
                <label htmlFor="input-stop-loss" className="text-[10px] font-semibold text-rose-600">
                  Cắt Lỗ (SL)
                </label>
                <input
                  id="input-stop-loss"
                  type="number"
                  step={0.1}
                  value={stopLoss}
                  onChange={(e) => setStopLoss(Number(e.target.value))}
                  className="w-full mt-0.5 px-2 py-1.5 text-xs rounded border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
                />
              </div>
              <div>
                <label htmlFor="input-take-profit" className="text-[10px] font-semibold text-emerald-600">
                  Chốt Lời (TP)
                </label>
                <input
                  id="input-take-profit"
                  type="number"
                  step={0.1}
                  value={takeProfit}
                  onChange={(e) => setTakeProfit(Number(e.target.value))}
                  className="w-full mt-0.5 px-2 py-1.5 text-xs rounded border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 min-h-[44px]"
                />
              </div>
            </div>

            {/* Kết quả tính toán vị thế */}
            <div className="p-3 rounded-lg bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 space-y-2 text-xs">
              <div className="flex justify-between items-center">
                <span className="text-zinc-600 dark:text-zinc-400">Tỷ lệ Lợi nhuận / Rủi ro (R:R):</span>
                <span
                  className={`font-extrabold px-2 py-0.5 rounded text-[11px] ${
                    rrRatio >= 2.0
                      ? "bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300"
                      : rrRatio >= 1.5
                      ? "bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300"
                      : "bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-300"
                  }`}
                >
                  1 : {rrRatio.toFixed(2)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-zinc-600 dark:text-zinc-400">Số lượng đơn vị mua tối đa:</span>
                <span className="font-extrabold text-indigo-600 dark:text-indigo-400">
                  {maxUnits.toLocaleString("vi-VN")} CP
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-zinc-600 dark:text-zinc-400">Giá trị giải ngân:</span>
                <span className="font-bold text-zinc-900 dark:text-zinc-100">
                  {formatVND(totalPositionValue)} ({portfolioCapital > 0 ? ((totalPositionValue / portfolioCapital) * 100).toFixed(1) : 0}% danh mục)
                </span>
              </div>
              <div className="pt-1.5 border-t border-zinc-100 dark:border-zinc-800 text-[11px] text-zinc-700 dark:text-zinc-300">
                🛡️ <em>Nếu lệnh dính Cắt lỗ (SL {stopLoss}), số tiền lỗ đúng bằng <strong>{formatVND(riskAmount)}</strong> ({riskPercent}% vốn), tuyệt đối không cháy tài khoản!</em>
              </div>
            </div>
          </div>

          {/* 3 Bài tập Mini đọc nhanh chỉ số doanh nghiệp */}
          <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4 space-y-3">
            <h4 className="text-xs font-bold text-zinc-900 dark:text-zinc-100 uppercase tracking-wider flex items-center gap-1.5">
              <Activity className="w-4 h-4 text-emerald-500" />
              Thử Thách Nhanh: 3 Chỉ Số Tài Chính Doanh Nghiệp
            </h4>

            {/* Quiz 1: P/E */}
            <div className="p-2.5 rounded-lg bg-zinc-50 dark:bg-zinc-800/40 space-y-1.5 text-xs">
              <div className="font-semibold text-zinc-900 dark:text-zinc-100">
                1. Cổ phiếu A có P/E = 6 trong khi trung bình ngành = 18. Có nên mua ngay?
              </div>
              <div className="grid grid-cols-2 gap-2">
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, pe: 0 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.pe === 0
                      ? "border-rose-500 bg-rose-50 dark:bg-rose-950/40 text-rose-800 dark:text-rose-200"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Mua ngay vì đang quá rẻ so với ngành.
                </button>
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, pe: 1 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.pe === 1
                      ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-800 dark:text-emerald-200 font-bold"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Cần thận trọng kiểm tra bẫy giá trị (Value Trap) do LN đột biến hoặc ngành suy thoái.
                </button>
              </div>
              {quizAnswers.pe !== null && (
                <p className="text-[11px] text-zinc-700 dark:text-zinc-300">
                  {quizAnswers.pe === 1
                    ? "✅ Đúng! P/E thấp có thể do lợi nhuận bất thường một lần bán tài sản, hoặc triển vọng tương lai rất xấu."
                    : "❌ Sai! Đừng nhìn P/E đơn lẻ, rất dễ dính bẫy giá trị (Value Trap)."}
                </p>
              )}
            </div>

            {/* Quiz 2: ROE */}
            <div className="p-2.5 rounded-lg bg-zinc-50 dark:bg-zinc-800/40 space-y-1.5 text-xs">
              <div className="font-semibold text-zinc-900 dark:text-zinc-100">
                2. Chỉ số ROE phản ánh điều gì cốt lõi nhất về năng lực ban lãnh đạo?
              </div>
              <div className="grid grid-cols-2 gap-2">
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, roe: 1 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.roe === 1
                      ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-800 dark:text-emerald-200 font-bold"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Hiệu quả sinh lời trên mỗi đồng vốn chủ sở hữu bỏ ra (&gt; 15% là xuất sắc).
                </button>
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, roe: 0 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.roe === 0
                      ? "border-rose-500 bg-rose-50 dark:bg-rose-950/40 text-rose-800 dark:text-rose-200"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Tốc độ tăng trưởng quy mô tổng tài sản của công ty.
                </button>
              </div>
              {quizAnswers.roe !== null && (
                <p className="text-[11px] text-zinc-700 dark:text-zinc-300">
                  {quizAnswers.roe === 1
                    ? "✅ Đúng! Warren Buffett luôn chọn các doanh nghiệp có ROE duy trì liên tục trên 15-20% trong 5 năm."
                    : "❌ Chưa đúng! Quy mô tài sản tăng có thể do vay nợ vô tội vạ, chỉ có ROE mới đo lường chất lượng sinh lời."}
                </p>
              )}
            </div>

            {/* Quiz 3: Debt/Equity */}
            <div className="p-2.5 rounded-lg bg-zinc-50 dark:bg-zinc-800/40 space-y-1.5 text-xs">
              <div className="font-semibold text-zinc-900 dark:text-zinc-100">
                3. Khi ngân hàng trung ương tăng lãi suất, công ty có tỷ lệ Nợ/Vốn (D/E &gt; 2.5) chịu rủi ro gì?
              </div>
              <div className="grid grid-cols-2 gap-2">
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, de: 1 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.de === 1
                      ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-800 dark:text-emerald-200 font-bold"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Chi phí lãi vay ăn mòn hết lợi nhuận ròng và áp lực đáo hạn đè nặng dòng tiền.
                </button>
                <button
                  type="button"
                  onClick={() => setQuizAnswers({ ...quizAnswers, de: 0 })}
                  className={`p-2 rounded text-left text-[11px] border min-h-[44px] ${
                    quizAnswers.de === 0
                      ? "border-rose-500 bg-rose-50 dark:bg-rose-950/40 text-rose-800 dark:text-rose-200"
                      : "border-zinc-200 dark:border-zinc-700"
                  }`}
                >
                  Không ảnh hưởng vì nợ doanh nghiệp luôn được nhà nước bảo lãnh.
                </button>
              </div>
              {quizAnswers.de !== null && (
                <p className="text-[11px] text-zinc-700 dark:text-zinc-300">
                  {quizAnswers.de === 1
                    ? "✅ Chính xác! Đòn bẩy cao là con dao hai lưỡi: phóng đại lợi nhuận lúc hưng thịnh nhưng sẽ bóp nghẹt thanh khoản khi lãi suất tăng."
                    : "❌ Sai! Nợ doanh nghiệp hoàn toàn là rủi ro của cổ đông và trái chủ, doanh nghiệp có thể phá sản."}
                </p>
              )}
            </div>
          </div>

          {/* Nút lưu Sổ tay */}
          <button
            type="button"
            disabled={isSaving}
            onClick={handleSaveToNotebook}
            className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-indigo-600 text-white font-semibold text-xs hover:bg-indigo-700 active:scale-[0.99] transition-all flex items-center justify-center gap-2 shadow-sm disabled:opacity-50"
          >
            {isSaving ? <Activity className="w-4 h-4 animate-spin" /> : <BookOpen className="w-4 h-4" />}
            Lưu phân tích rủi ro & bài tập vào Sổ tay
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// COMPONENT CHÍNH: FinancePracticeLab
// =============================================================================
interface ExtractedFinanceCaseStudyLabProps {
  lessonMarkdown: string;
  lessonTitle: string;
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
}

function ExtractedFinanceCaseStudyLab({
  lessonMarkdown,
  lessonTitle,
  onSaveToNotebook,
  onCreateFlashcardQuick,
}: ExtractedFinanceCaseStudyLabProps) {
  const extractedCase = useMemo(() => {
    const lines = (lessonMarkdown || "").split(/\r?\n/);
    const tables: string[][] = [];
    let currentTable: string[] = [];

    for (const line of lines) {
      const trimmed = line.trim();
      if (trimmed.startsWith("|") && trimmed.endsWith("|")) {
        currentTable.push(trimmed);
      } else if (currentTable.length > 0) {
        if (currentTable.length >= 2) tables.push([...currentTable]);
        currentTable = [];
      }
    }
    if (currentTable.length >= 2) tables.push(currentTable);

    const keyPoints: string[] = [];
    const bulletRegex = /(?:^|\n)(?:[-*•]|\d+\.)\s+(?:\*\*|\[\s*\])?([A-ZÀ-Ỹa-zà-ỹ0-9][^\n]{15,140})/g;
    let bm: RegExpExecArray | null;
    while ((bm = bulletRegex.exec(lessonMarkdown || "")) !== null) {
      if (keyPoints.length < 8) keyPoints.push(bm[1].trim().replace(/\*+/g, ""));
    }

    return { tables, keyPoints };
  }, [lessonMarkdown]);

  const [monthlyIncome, setMonthlyIncome] = useState(25_000_000);
  const [savingRate, setSavingRate] = useState(20);
  const [investmentReturn, setInvestmentReturn] = useState(10);
  const [years, setYears] = useState(15);
  const [personalSolution, setPersonalSolution] = useState("");
  const [savedSuccess, setSavedSuccess] = useState(false);

  const monthlySavings = (monthlyIncome * savingRate) / 100;
  const totalContributed = monthlySavings * 12 * years;
  const projectedWealth = useMemo(() => {
    const monthlyRate = investmentReturn / 100 / 12;
    const months = years * 12;
    if (monthlyRate === 0) return totalContributed;
    return monthlySavings * ((Math.pow(1 + monthlyRate, months) - 1) / monthlyRate);
  }, [monthlySavings, investmentReturn, years, totalContributed]);

  const handleSaveCase = async () => {
    const content = [
      `### 📊 Kế Hoạch Tài Chính & Tình Huống Bài Học: ${lessonTitle}`,
      `- **Thu nhập giả định:** ${formatVND(monthlyIncome)} / tháng`,
      `- **Tỷ lệ tiết kiệm / đầu tư:** ${savingRate}% (${formatVND(monthlySavings)} / tháng)`,
      `- **Lãi suất kỳ vọng:** ${investmentReturn}% / năm | Thời gian: ${years} năm`,
      `- **Tổng vốn tự tích lũy:** ${formatVND(totalContributed)}`,
      `- **Giá trị tài sản dự kiến (lãi kép):** ${formatVND(Math.round(projectedWealth))}`,
      personalSolution.trim() ? `\n#### Kế hoạch hành động & Phân tích cá nhân:\n${personalSolution}` : "",
    ].filter(Boolean).join("\n");

    await onSaveToNotebook(content);
    setSavedSuccess(true);
    setTimeout(() => setSavedSuccess(false), 3000);
  };

  const handleCreateCard = async () => {
    if (!onCreateFlashcardQuick) return;
    await onCreateFlashcardQuick(
      `Mục tiêu tích lũy bài "${lessonTitle}":`,
      `Thu nhập ${formatVND(monthlyIncome)}, tiết kiệm ${savingRate}% (${formatVND(monthlySavings)}/tháng) với lãi ${investmentReturn}%/năm sau ${years} năm đạt ${formatVND(Math.round(projectedWealth))}.`
    );
  };

  return (
    <div data-testid="finance-case-study-lab" className="space-y-6 p-4 sm:p-6 bg-white dark:bg-zinc-900 rounded-2xl border border-zinc-200 dark:border-zinc-800 shadow-sm text-zinc-900 dark:text-zinc-100">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-zinc-200 dark:border-zinc-800">
        <div>
          <span className="text-xs font-semibold text-purple-600 dark:text-purple-400 uppercase tracking-wider inline-flex items-center gap-1.5">
            <Briefcase className="w-3.5 h-3.5" />
            Tình Huống Thực Tế & Bảng Kế Hoạch Bài Học
          </span>
          <h3 className="text-base sm:text-lg font-bold text-zinc-900 dark:text-zinc-100 mt-0.5">
            {lessonTitle}
          </h3>
        </div>
        <div className="flex items-center gap-2">
          {onCreateFlashcardQuick && (
            <button
              type="button"
              onClick={handleCreateCard}
              className="px-3 py-1.5 rounded-lg border border-purple-200 dark:border-purple-800 text-xs font-medium text-purple-700 dark:text-purple-300 hover:bg-purple-50 dark:hover:bg-purple-950/50 transition cursor-pointer"
            >
              <Sparkles className="w-3.5 h-3.5 inline mr-1" />
              Tạo Thẻ Tình Huống
            </button>
          )}
        </div>
      </div>

      {/* Case Overview & Tables from Lesson */}
      {extractedCase.tables.length > 0 && (
        <div className="space-y-3">
          <h4 className="text-xs font-bold text-zinc-700 dark:text-zinc-300 uppercase tracking-wider flex items-center gap-1.5">
            <Calculator className="w-3.5 h-3.5 text-purple-500" />
            Dữ liệu Bảng trích xuất từ bài học:
          </h4>
          <div className="overflow-x-auto p-3 rounded-xl bg-zinc-50 dark:bg-zinc-950 border border-zinc-200 dark:border-zinc-800 text-xs font-mono">
            {extractedCase.tables.slice(0, 2).map((tbl, tIdx) => (
              <pre key={tIdx} className="whitespace-pre overflow-x-auto text-zinc-700 dark:text-zinc-300">
                {tbl.join("\n")}
              </pre>
            ))}
          </div>
        </div>
      )}

      {/* Interactive Simulation Controls */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 p-4 rounded-xl bg-purple-50/50 dark:bg-purple-950/20 border border-purple-200/60 dark:border-purple-800/60">
        <div>
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 block mb-1">
            Thu nhập hàng tháng:
          </label>
          <input
            type="number"
            step="1000000"
            value={monthlyIncome}
            onChange={(e) => setMonthlyIncome(Math.max(1_000_000, Number(e.target.value)))}
            className="w-full px-3 py-1.5 rounded-lg border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 text-xs font-semibold"
          />
          <span className="text-[11px] text-zinc-500 mt-0.5 block">{formatVND(monthlyIncome)}</span>
        </div>

        <div>
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 block mb-1">
            Tỷ lệ tiết kiệm ({savingRate}%):
          </label>
          <input
            type="range"
            min="5"
            max="60"
            step="1"
            value={savingRate}
            onChange={(e) => setSavingRate(Number(e.target.value))}
            className="w-full cursor-pointer accent-purple-600"
          />
          <span className="text-[11px] text-zinc-500 mt-0.5 block">{formatVND(monthlySavings)} / tháng</span>
        </div>

        <div>
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 block mb-1">
            Lãi suất danh nghĩa ({investmentReturn}%/năm):
          </label>
          <input
            type="range"
            min="4"
            max="20"
            step="0.5"
            value={investmentReturn}
            onChange={(e) => setInvestmentReturn(Number(e.target.value))}
            className="w-full cursor-pointer accent-purple-600"
          />
          <span className="text-[11px] text-zinc-500 mt-0.5 block">Kỳ vọng quỹ chỉ số / đa tài sản</span>
        </div>

        <div>
          <label className="text-xs font-medium text-zinc-600 dark:text-zinc-400 block mb-1">
            Thời gian tích lũy ({years} năm):
          </label>
          <input
            type="range"
            min="3"
            max="35"
            step="1"
            value={years}
            onChange={(e) => setYears(Number(e.target.value))}
            className="w-full cursor-pointer accent-purple-600"
          />
          <span className="text-[11px] text-zinc-500 mt-0.5 block">{years * 12} kỳ đóng góp</span>
        </div>
      </div>

      {/* Projections Card */}
      <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div>
          <span className="text-xs text-zinc-500 block">Tổng vốn tự đóng góp ({years} năm):</span>
          <span className="text-lg font-bold text-zinc-800 dark:text-zinc-200">{formatVND(totalContributed)}</span>
        </div>
        <div>
          <span className="text-xs text-purple-600 dark:text-purple-400 font-semibold block">Tài sản dự kiến (Kèm lãi kép):</span>
          <span className="text-2xl font-extrabold text-purple-700 dark:text-purple-300">{formatVND(Math.round(projectedWealth))}</span>
        </div>
      </div>

      {/* User Notes / Personal Solution */}
      <div className="space-y-2">
        <label className="text-xs font-semibold text-zinc-700 dark:text-zinc-300 block">
          Ghi chú phân tích tình huống & Cam kết cá nhân:
        </label>
        <textarea
          rows={4}
          value={personalSolution}
          onChange={(e) => setPersonalSolution(e.target.value)}
          placeholder="Ghi lại các nhận định cá nhân: ví dụ phân bổ các quỹ khẩn cấp, cắt giảm chi phí lãng phí, kế hoạch tăng thu nhập..."
          className="w-full p-3 rounded-xl border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-950 text-xs text-zinc-800 dark:text-zinc-200 focus:outline-none focus:ring-2 focus:ring-purple-500 leading-relaxed"
        />
      </div>

      <div className="flex items-center justify-between pt-2">
        {savedSuccess ? (
          <span className="text-xs font-semibold text-emerald-600 dark:text-emerald-400 inline-flex items-center gap-1.5">
            <CheckCircle2 className="w-4 h-4" />
            Đã lưu kế hoạch vào Sổ tay học tập!
          </span>
        ) : (
          <span className="text-xs text-zinc-500">
            Dữ liệu tính toán dựa trên công thức Annuity Future Value
          </span>
        )}
        <button
          type="button"
          onClick={handleSaveCase}
          className="min-h-[44px] px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-xs font-semibold text-white inline-flex items-center gap-2 transition cursor-pointer shadow-sm"
        >
          <BookmarkPlus className="w-4 h-4" />
          <span>Lưu Kế Hoạch Vào Sổ Tay</span>
        </button>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 6: MÔ PHỎNG PHÂN BỔ DANH MỤC ĐẦU TƯ ĐA TÀI SẢN (DỮ LIỆU MẪU)
// =============================================================================
export interface PortfolioSimulatorLabProps {
  onSaveToNotebook: (text: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}

export function PortfolioSimulatorLab({
  onSaveToNotebook,
  onRecordAttempt,
}: PortfolioSimulatorLabProps) {
  const [growth, setGrowth] = useState<number>(40);
  const [dividend, setDividend] = useState<number>(25);
  const [bonds, setBonds] = useState<number>(20);
  const [gold, setGold] = useState<number>(10);
  const [cash, setCash] = useState<number>(5);
  const [scenario, setScenario] = useState<"bull" | "inflation" | "recession">("bull");
  const [isRecorded, setIsRecorded] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  const totalAllocation = growth + dividend + bonds + gold + cash;
  const is100 = totalAllocation === 100;

  const normalizeAllocations = () => {
    if (totalAllocation === 0) {
      setGrowth(40); setDividend(25); setBonds(20); setGold(10); setCash(5);
      return;
    }
    const factor = 100 / totalAllocation;
    const g = Math.round(growth * factor);
    const d = Math.round(dividend * factor);
    const b = Math.round(bonds * factor);
    const goldNew = Math.round(gold * factor);
    const c = 100 - (g + d + b + goldNew);
    setGrowth(g); setDividend(d); setBonds(b); setGold(goldNew); setCash(c);
  };

  const scenarioMeta = useMemo(() => {
    if (scenario === "bull") {
      return {
        label: "Thị trường Tăng trưởng (Bull Market)",
        desc: "Kinh tế mở rộng, lợi nhuận doanh nghiệp tăng vọt, lãi suất thấp.",
        rates: { growth: 24, dividend: 14, bonds: 6, gold: 4, cash: 4 },
        drawdownFactor: 0.12,
      };
    }
    if (scenario === "inflation") {
      return {
        label: "Lạm phát cao (High Inflation)",
        desc: "Giá cả leo thang, ngân hàng trung ương thắt chặt tiền tệ, hàng hóa & vàng hưởng lợi.",
        rates: { growth: 8, dividend: 12, bonds: 3, gold: 22, cash: 5 },
        drawdownFactor: 0.18,
      };
    }
    return {
      label: "Suy thoái kinh tế (Recession)",
      desc: "Thị trường sụt giảm sâu, định giá nén lại, trái phiếu & tiền mặt bảo vệ tài sản.",
      rates: { growth: -28, dividend: -12, bonds: 9, gold: 15, cash: 4 },
      drawdownFactor: 0.32,
    };
  }, [scenario]);

  const expectedReturn = useMemo(() => {
    const { rates } = scenarioMeta;
    return Math.round(
      ((growth * rates.growth +
        dividend * rates.dividend +
        bonds * rates.bonds +
        gold * rates.gold +
        cash * rates.cash) /
        100) *
        10
    ) / 10;
  }, [growth, dividend, bonds, gold, cash, scenarioMeta]);

  const maxDrawdown = useMemo(() => {
    if (scenario === "bull") {
      return Math.round(-(growth * 0.15 + dividend * 0.08 + bonds * 0.02 + gold * 0.04));
    }
    if (scenario === "inflation") {
      return Math.round(-(growth * 0.22 + dividend * 0.14 + bonds * 0.06 + gold * 0.04));
    }
    return Math.round(-(growth * 0.45 + dividend * 0.25 + bonds * 0.02 + gold * 0.05));
  }, [growth, dividend, bonds, gold, scenario]);

  const volatility = useMemo(() => {
    const raw = Math.sqrt(
      Math.pow((growth / 100) * 0.24, 2) +
      Math.pow((dividend / 100) * 0.14, 2) +
      Math.pow((bonds / 100) * 0.05, 2) +
      Math.pow((gold / 100) * 0.16, 2)
    ) * 100;
    return Math.max(3.5, Math.round(raw * 10) / 10);
  }, [growth, dividend, bonds, gold]);

  // Dự phóng 10 năm từ vốn gốc 100 triệu VNĐ
  const initialFund = 100_000_000;
  const growthPoints = useMemo(() => {
    const pts: { year: number; val: number }[] = [];
    const r = expectedReturn / 100;
    for (let y = 0; y <= 10; y++) {
      pts.push({ year: y, val: Math.round(initialFund * Math.pow(1 + r, y)) });
    }
    return pts;
  }, [expectedReturn]);

  const svgW = 600;
  const svgH = 200;
  const padL = 60;
  const padR = 20;
  const padT = 20;
  const padB = 30;
  const plotW = svgW - padL - padR;
  const plotH = svgH - padT - padB;

  const maxVal = Math.max(1, ...growthPoints.map((p) => p.val), initialFund * 1.1);
  const minVal = Math.min(...growthPoints.map((p) => p.val), initialFund * 0.8);
  const valRange = Math.max(1, maxVal - minVal);

  const scaleX = (y: number) => padL + (y / 10) * plotW;
  const scaleY = (v: number) => padT + plotH - ((v - minVal) / valRange) * plotH;

  const lineD = useMemo(() => {
    return growthPoints
      .map((p, i) => `${i === 0 ? "M" : "L"} ${scaleX(p.year).toFixed(1)} ${scaleY(p.val).toFixed(1)}`)
      .join(" ");
  }, [growthPoints, maxVal, minVal]);

  const handleRecord = async () => {
    if (!onRecordAttempt) return;
    await onRecordAttempt({
      activityType: "portfolio_sim",
      activityKey: `portfolio-sim-${scenario}`,
      activityTitle: `Mô phỏng Phân bổ Danh mục (${scenarioMeta.label})`,
      score: 100,
      isPassed: true,
      detailJson: JSON.stringify({
        isSampleData: true,
        allocations: { growth, dividend, bonds, gold, cash },
        scenario,
        expectedReturn,
        maxDrawdown,
        volatility,
      }),
    });
    setIsRecorded(true);
    setFeedback("Đã lưu kết quả mô phỏng danh mục mẫu vào Practice Engine!");
    setTimeout(() => setFeedback(null), 3500);
  };

  const handleSaveNotebook = async () => {
    setIsSaving(true);
    const md = `### 📊 Mô Phỏng Phân Bổ Danh Mục Đa Tài Sản (Dữ Liệu Mẫu)
> **Ghi chú bắt buộc:** Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư.

- **Kịch bản vĩ mô:** ${scenarioMeta.label}
- **Cơ cấu phân bổ danh mục:**
  - Cổ phiếu tăng trưởng: ${growth}%
  - Cổ phiếu cổ tức: ${dividend}%
  - Trái phiếu chính phủ: ${bonds}%
  - Vàng phòng vệ: ${gold}%
  - Tiền mặt / Tiết kiệm: ${cash}%
- **Chỉ số danh mục dự phóng:**
  - 📈 Lợi suất kỳ vọng: **${expectedReturn}% / năm**
  - 📉 Mức sụt giảm tối đa ước tính (Max Drawdown): **${maxDrawdown}%**
  - ⚡ Độ biến động hàng năm (Volatility): **${volatility}%**
  - 💰 Giá trị vốn 100 triệu sau 10 năm: **${formatVND(growthPoints[10].val)}**
`;
    try {
      await onSaveToNotebook(md);
      setFeedback("Đã lưu kết quả phân bổ vào Sổ tay!");
      setTimeout(() => setFeedback(null), 3000);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-200">
      {/* KHUYẾN CÁO BẮT BUỘC ZERO-FACADE */}
      <div className="p-3.5 rounded-xl border border-amber-300 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/40 text-amber-900 dark:text-amber-200 text-xs font-semibold flex items-center gap-2.5 shadow-xs">
        <ShieldAlert className="w-5 h-5 text-amber-600 shrink-0" />
        <span>Dữ liệu mẫu lịch sử mô phỏng phục vụ thực hành — không phải khuyến nghị đầu tư</span>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-3">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <PieChart className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            Mô Phỏng Phân Bổ Danh Mục Đa Tài Sản (Portfolio Simulator)
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Phân bổ tỷ trọng 5 lớp tài sản, đo lường lợi suất kỳ vọng, mức sụt giảm tối đa (Max Drawdown) và độ biến động qua 3 kịch bản kinh tế.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            data-testid="portfolio-record-btn"
            onClick={handleRecord}
            className="min-h-[44px] px-3.5 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-xs transition cursor-pointer flex items-center gap-1.5 shadow-xs"
          >
            <CheckCircle2 className="w-4 h-4" />
            Ghi nhận Practice Engine
          </button>
        </div>
      </div>

      {feedback && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 text-emerald-600" />
          <span>{feedback}</span>
        </div>
      )}

      {/* Grid: Tỷ trọng bên trái, Kịch bản & Biểu đồ bên phải */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Cột 1: Sliders Phân bổ 5 lớp tài sản */}
        <div className="lg:col-span-5 space-y-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50/60 dark:bg-zinc-900/40 p-4">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
              <SlidersHorizontal className="w-4 h-4 text-indigo-500" />
              Tỷ Trọng Tài Sản (%)
            </h3>
            <span className={`text-xs font-bold px-2 py-0.5 rounded ${
              is100
                ? "bg-emerald-100 dark:bg-emerald-900/60 text-emerald-700 dark:text-emerald-300"
                : "bg-red-100 dark:bg-red-900/60 text-red-700 dark:text-red-300"
            }`}>
              Tổng: {totalAllocation}%
            </span>
          </div>

          {!is100 && (
            <div className="p-2.5 rounded-lg bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900/40 text-[11px] text-red-700 dark:text-red-300 flex items-center justify-between gap-2">
              <span>Tổng tỷ trọng phải bằng đúng 100%.</span>
              <button
                type="button"
                onClick={normalizeAllocations}
                className="font-bold underline cursor-pointer hover:text-red-900"
              >
                Cân bằng 100%
              </button>
            </div>
          )}

          {/* 1. Cổ phiếu tăng trưởng */}
          <div className="space-y-1">
            <div className="flex justify-between text-xs">
              <span className="font-medium text-zinc-700 dark:text-zinc-300">1. Cổ phiếu tăng trưởng</span>
              <span className="font-bold text-indigo-600 dark:text-indigo-400">{growth}%</span>
            </div>
            <input
              type="range" min={0} max={100} step={5} value={growth}
              onChange={(e) => setGrowth(Number(e.target.value))}
              className="w-full accent-indigo-600 cursor-pointer min-h-[44px]"
            />
          </div>

          {/* 2. Cổ phiếu cổ tức */}
          <div className="space-y-1">
            <div className="flex justify-between text-xs">
              <span className="font-medium text-zinc-700 dark:text-zinc-300">2. Cổ phiếu cổ tức & phòng thủ</span>
              <span className="font-bold text-blue-600 dark:text-blue-400">{dividend}%</span>
            </div>
            <input
              type="range" min={0} max={100} step={5} value={dividend}
              onChange={(e) => setDividend(Number(e.target.value))}
              className="w-full accent-blue-600 cursor-pointer min-h-[44px]"
            />
          </div>

          {/* 3. Trái phiếu chính phủ */}
          <div className="space-y-1">
            <div className="flex justify-between text-xs">
              <span className="font-medium text-zinc-700 dark:text-zinc-300">3. Trái phiếu chính phủ / DN uy tín</span>
              <span className="font-bold text-emerald-600 dark:text-emerald-400">{bonds}%</span>
            </div>
            <input
              type="range" min={0} max={100} step={5} value={bonds}
              onChange={(e) => setBonds(Number(e.target.value))}
              className="w-full accent-emerald-600 cursor-pointer min-h-[44px]"
            />
          </div>

          {/* 4. Vàng */}
          <div className="space-y-1">
            <div className="flex justify-between text-xs">
              <span className="font-medium text-zinc-700 dark:text-zinc-300">4. Vàng phòng vệ lạm phát</span>
              <span className="font-bold text-amber-600 dark:text-amber-400">{gold}%</span>
            </div>
            <input
              type="range" min={0} max={100} step={5} value={gold}
              onChange={(e) => setGold(Number(e.target.value))}
              className="w-full accent-amber-600 cursor-pointer min-h-[44px]"
            />
          </div>

          {/* 5. Tiền mặt */}
          <div className="space-y-1">
            <div className="flex justify-between text-xs">
              <span className="font-medium text-zinc-700 dark:text-zinc-300">5. Tiền mặt / Tiền gửi thanh khoản</span>
              <span className="font-bold text-zinc-600 dark:text-zinc-400">{cash}%</span>
            </div>
            <input
              type="range" min={0} max={100} step={5} value={cash}
              onChange={(e) => setCash(Number(e.target.value))}
              className="w-full accent-zinc-600 cursor-pointer min-h-[44px]"
            />
          </div>
        </div>

        {/* Cột 2: Chọn kịch bản vĩ mô & Chỉ số danh mục */}
        <div className="lg:col-span-7 space-y-4">
          {/* Bộ chọn Kịch bản Vĩ mô */}
          <div className="p-3.5 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 space-y-2">
            <span className="text-xs font-bold uppercase text-zinc-600 dark:text-zinc-400">
              Chọn Kịch Bản Vĩ Mô Mô Phỏng:
            </span>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
              {[
                { id: "bull", name: "Thị trường Tăng trưởng", icon: TrendingUp, color: "text-emerald-600" },
                { id: "inflation", name: "Lạm phát cao", icon: Flame, color: "text-amber-600" },
                { id: "recession", name: "Suy thoái kinh tế", icon: TrendingDown, color: "text-red-600" },
              ].map((sc) => {
                const Icon = sc.icon;
                const isAct = scenario === sc.id;
                return (
                  <button
                    key={sc.id}
                    type="button"
                    onClick={() => setScenario(sc.id as any)}
                    className={`p-2.5 rounded-lg border text-xs font-semibold transition cursor-pointer flex flex-col items-center gap-1.5 ${
                      isAct
                        ? "border-indigo-600 bg-indigo-50/70 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-300 shadow-xs"
                        : "border-zinc-200 dark:border-zinc-800 hover:bg-zinc-50 dark:hover:bg-zinc-800 text-zinc-700 dark:text-zinc-300"
                    }`}
                  >
                    <Icon className={`w-4 h-4 ${sc.color}`} />
                    <span className="text-center">{sc.name}</span>
                  </button>
                );
              })}
            </div>
            <p className="text-[11px] text-zinc-500 italic mt-1">{scenarioMeta.desc}</p>
          </div>

          {/* 3 Thẻ Chỉ Số Cốt Lõi */}
          <div className="grid grid-cols-3 gap-3">
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] font-bold uppercase text-zinc-500">Lợi Suất Dự Phóng</span>
              <div className={`text-base font-extrabold mt-0.5 ${expectedReturn >= 0 ? "text-emerald-600" : "text-red-600"}`}>
                {expectedReturn > 0 ? `+${expectedReturn}` : expectedReturn}%
              </div>
              <span className="text-[10px] text-zinc-500">Mỗi năm</span>
            </div>
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] font-bold uppercase text-zinc-500">Max Drawdown</span>
              <div className="text-base font-extrabold text-red-600 mt-0.5">
                {maxDrawdown}%
              </div>
              <span className="text-[10px] text-zinc-500">Sụt giảm ước tính</span>
            </div>
            <div className="p-3 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900">
              <span className="text-[10px] font-bold uppercase text-zinc-500">Độ Biến Động</span>
              <div className="text-base font-extrabold text-amber-600 mt-0.5">
                ±{volatility}%
              </div>
              <span className="text-[10px] text-zinc-500">Độ lệch chuẩn</span>
            </div>
          </div>

          {/* SVG Biểu đồ đường tăng trưởng mô phỏng 10 năm */}
          <div className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-1.5">
                <BarChart2 className="w-4 h-4 text-indigo-500" />
                Đường Tăng Trưởng Mô Phỏng 10 Năm (Vốn mẫu: 100 tr)
              </span>
              <span className="font-mono text-indigo-600 dark:text-indigo-400 font-bold">
                Năm 10: {formatCompactVND(growthPoints[10].val)}
              </span>
            </div>

            <div className="w-full bg-zinc-50 dark:bg-zinc-950/50 rounded-lg p-2 border border-zinc-100 dark:border-zinc-800">
              <svg viewBox={`0 0 ${svgW} ${svgH}`} className="w-full h-auto block select-none">
                <line x1={padL} y1={padT} x2={padL} y2={padT + plotH} stroke="currentColor" className="text-zinc-300 dark:text-zinc-700" strokeWidth="1" />
                <line x1={padL} y1={padT + plotH} x2={svgW - padR} y2={padT + plotH} stroke="currentColor" className="text-zinc-300 dark:text-zinc-700" strokeWidth="1" />

                <path d={lineD} fill="none" stroke="#6366f1" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" />

                {growthPoints.map((p) => (
                  <circle
                    key={p.year}
                    cx={scaleX(p.year)}
                    cy={scaleY(p.val)}
                    r={3}
                    fill="#6366f1"
                    stroke="#ffffff"
                    strokeWidth="1.5"
                  />
                ))}
              </svg>
            </div>
          </div>

          <button
            type="button"
            disabled={isSaving}
            onClick={handleSaveNotebook}
            className="w-full min-h-[44px] px-4 py-2.5 rounded-xl bg-indigo-600 text-white font-semibold text-xs hover:bg-indigo-700 transition flex items-center justify-center gap-2 shadow-xs cursor-pointer"
          >
            <BookmarkPlus className="w-4 h-4" />
            Lưu Kết Quả Phân Bổ Vào Sổ Tay
          </button>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// CÔNG CỤ 7: QUIZ THIÊN KIẾN HÀNH VI ĐẦU TƯ (BEHAVIORAL FINANCE QUIZ)
// =============================================================================
export interface BehavioralFinanceQuizLabProps {
  onSaveToNotebook: (text: string) => Promise<void>;
  onCreateFlashcardQuick?: (front: string, back: string) => Promise<void>;
  onRecordAttempt?: (params: any) => Promise<void>;
}

export function BehavioralFinanceQuizLab({
  onSaveToNotebook,
  onCreateFlashcardQuick,
  onRecordAttempt,
}: BehavioralFinanceQuizLabProps) {
  const [userAnswers, setUserAnswers] = useState<Record<string, number>>({});
  const [showExplanations, setShowExplanations] = useState<Record<string, boolean>>({});
  const [savedFlashcards, setSavedFlashcards] = useState<Record<string, boolean>>({});
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const questions = useMemo(() => [
    {
      id: "fomo",
      biasTitle: "1. FOMO & Tâm Lý Bầy Đàn (Herd Mentality)",
      scenario: "Một mã cổ phiếu tăng trần 7 phiên liên tiếp với khối lượng đột biến, khắp các diễn đàn mạng xã hội đều bàn tán sôi nổi. Bạn chưa nghiên cứu báo cáo tài chính của doanh nghiệp này. Quyết định đúng đắn theo kỷ luật đầu tư là gì?",
      options: [
        "Đặt lệnh mua bằng mọi giá vào phiên mở cửa ngày mai",
        "Dùng đòn bẩy margin tối đa để bắt trọn con sóng",
        "Giữ bình tĩnh, không mua đuổi khi thiếu biên an toàn và chỉ đầu tư vào tài sản hiểu rõ",
        "Bán toàn bộ danh mục chuyển sang mã nóng này"
      ],
      correctIndex: 2,
      explanation: "FOMO là thiên kiến tâm lý khiến nhà đầu tư bỏ qua phân tích cơ bản để mua theo cảm xúc hưng phấn của đám đông, thường dẫn đến việc mua đúng vùng đỉnh chu kỳ.",
      fcFront: "Thiên kiến FOMO (Fear of Missing Out) trong đầu tư là gì?",
      fcBack: "FOMO là tâm lý sợ bỏ lỡ cơ hội làm giàu nhanh, khiến nhà đầu tư bỏ qua phân tích cơ bản và định giá để mua đuổi theo đám đông, thường dẫn đến mua ở vùng giá đỉnh.",
    },
    {
      id: "loss_aversion",
      biasTitle: "2. Thiên Kiến Sợ Thua Lỗ (Loss Aversion)",
      scenario: "Bạn mua cổ phiếu với nguyên tắc cắt lỗ nghiêm ngặt ở mức -7%. Hiện giá đã giảm -10% do triển vọng kinh doanh cốt lõi của công ty xấu đi rõ rệt. Bạn có xu hướng:",
      options: [
        "Không bán vì tin rằng 'chưa bán là chưa lỗ', tiếp tục gồng lỗ hy vọng giá hồi phục",
        "Thực hiện kỷ luật cắt lỗ ngay lập tức để bảo toàn 90% vốn và tái phân bổ sang cơ hội tốt hơn",
        "Vay nợ thêm để bắt đáy trung bình giá xuống bất chấp rủi ro phá sản",
        "Xóa app giao dịch và phớt lờ thực tế"
      ],
      correctIndex: 1,
      explanation: "Nỗi đau mất tiền lớn gấp 2 đến 2.5 lần niềm vui kiếm được cùng số tiền. Điều này khiến nhà đầu tư từ chối chấp nhận sai lầm và ôm giữ tài sản xấu quá lâu.",
      fcFront: "Tại sao thiên kiến Loss Aversion nguy hiểm với nhà đầu tư?",
      fcBack: "Nỗi đau của khoản lỗ khiến nhà đầu tư từ chối cắt lỗ, giữ tài sản suy giảm chất lượng quá lâu và dẫn đến cháy tài khoản hoặc chôn vốn vĩnh viễn.",
    },
    {
      id: "anchoring",
      biasTitle: "3. Thiên Kiến Mỏ Neo (Anchoring Bias)",
      scenario: "Cổ phiếu X từng đạt đỉnh 120.000đ/cổ phiếu. Doanh nghiệp sau đó mất 60% thị phần và giá giảm về 45.000đ. Nhiều người cho rằng 45.000đ là 'quá rẻ' so với 120.000đ nên mua vào. Lỗi tư duy ở đây là gì?",
      options: [
        "Định giá chuẩn theo Benjamin Graham",
        "Bị neo vào mức giá đỉnh quá khứ (120k) mà không định giá lại năng lực tạo tiền hiện tại của doanh nghiệp",
        "Đầu tư giá trị thuần túy",
        "Quản trị rủi ro danh mục"
      ],
      correctIndex: 1,
      explanation: "Giá quá khứ không mang lại giá trị tương lai. Nếu năng lực tạo lợi nhuận suy giảm vĩnh viễn, mức giá 45.000đ vẫn có thể là đắt đỏ.",
      fcFront: "Thiên kiến Mỏ neo (Anchoring Bias) trong định giá cổ phiếu là gì?",
      fcBack: "Là xu hướng lấy mức giá đỉnh trong quá khứ làm mốc tham chiếu để cho rằng giá hiện tại là 'rẻ', thay vì định giá dựa trên dòng tiền và năng lực tạo lợi nhuận hiện tại.",
    },
    {
      id: "sunk_cost",
      biasTitle: "4. Ảo Tưởng Chi Phí Chìm (Sunk Cost Fallacy)",
      scenario: "Một dự án kinh doanh bạn rót 200 triệu đã phát hiện lỗi kiến trúc không thể sửa đổi và chắc chắn tiếp tục thua lỗ nặng. Quyết định hợp lý là gì?",
      options: [
        "Rót thêm 100 triệu nữa để hy vọng 'cứu vãn' 200 triệu ban đầu",
        "Chấp nhận dừng lại, cắt lỗ để bảo vệ số tiền còn lại và tập trung nguồn lực mới",
        "Đổ lỗi cho hoàn cảnh và tiếp tục chịu lỗ hàng tháng",
        "Giấu kín để người khác không biết mình sai"
      ],
      correctIndex: 1,
      explanation: "200 triệu đã chi là chi phí chìm không thể thu hồi. Quyết định tương lai chỉ được dựa trên chi phí và lợi ích tương lai, không thể cứu tiền quá khứ bằng cách đổ thêm tiền mới.",
      fcFront: "Ảo tưởng Chi phí chìm (Sunk Cost Fallacy) là gì?",
      fcBack: "Là xu hướng tiếp tục đầu tư tiền bạc, thời gian vào một dự án/tài sản thua lỗ chỉ vì tiếc những chi phí đã bỏ ra trong quá khứ mà không thể thu hồi.",
    },
    {
      id: "contrarian",
      biasTitle: "5. Tư Duy Ngược Dòng (Contrarian Mindset)",
      scenario: "Trong cơn hoảng loạn của thị trường, một doanh nghiệp bán lẻ đầu ngành không nợ vay bị bán tháo khiến P/E giảm về mức kỷ lục 8x. Hành động của nhà đầu tư lý trí là gì?",
      options: [
        "Bán tháo theo đám đông vì sợ thị trường sập",
        "Rà soát biên an toàn, định giá dòng tiền và kiên định giải ngân từng phần (DCA)",
        "Đóng tài khoản không theo dõi",
        "Kêu gọi mọi người tẩy chay thị trường"
      ],
      correctIndex: 1,
      explanation: "Warren Buffett khuyên: 'Hãy tham lam khi người khác sợ hãi'. Khủng hoảng hoảng loạn chính là thời điểm tài sản tốt được bán với giá chiết khấu sâu nhất.",
      fcFront: "Tư duy ngược dòng (Contrarian Investing) dựa trên nguyên lý nào?",
      fcBack: "Dựa trên nguyên lý tâm lý đám đông thường thái quá ở cả hai đầu: quá hưng phấn khi giá tăng và quá sợ hãi khi giá giảm. Nhà đầu tư lý trí mua vào khi biên an toàn cao trong hoảng loạn.",
    }
  ], []);

  const totalAnswered = Object.keys(userAnswers).length;
  const correctCount = questions.filter((q) => userAnswers[q.id] === q.correctIndex).length;

  const handleSelectAnswer = async (qId: string, optIdx: number) => {
    setUserAnswers((prev) => ({ ...prev, [qId]: optIdx }));
    setShowExplanations((prev) => ({ ...prev, [qId]: true }));

    const updated = { ...userAnswers, [qId]: optIdx };
    if (Object.keys(updated).length === questions.length && onRecordAttempt) {
      const score = Math.round((questions.filter((q) => updated[q.id] === q.correctIndex).length / questions.length) * 100);
      await onRecordAttempt({
        activityType: "behavioral_quiz",
        activityKey: "behavioral-finance-5biases",
        activityTitle: "Quiz Thiên Kiến Tâm Lý Nhà Đầu Tư",
        score,
        isPassed: score >= 70,
        detailJson: JSON.stringify(updated),
      });
    }
  };

  const handleSaveFlashcard = async (q: typeof questions[0]) => {
    if (savedFlashcards[q.id]) return;
    try {
      if (onCreateFlashcardQuick) {
        await onCreateFlashcardQuick(q.fcFront, q.fcBack);
      } else {
        const headers = getClientRequestHeaders();
        await fetch("/api/v1/flashcards", {
          method: "POST",
          headers: { ...headers, "Content-Type": "application/json" },
          body: JSON.stringify({ frontText: q.fcFront, backText: q.fcBack }),
        });
      }
      setSavedFlashcards((prev) => ({ ...prev, [q.id]: true }));
      setToastMessage(`Đã lưu "${q.biasTitle}" vào Flashcard FSRS!`);
      setTimeout(() => setToastMessage(null), 3000);
    } catch {
      // non-blocking
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-200">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-zinc-200 dark:border-zinc-800 pb-3">
        <div>
          <h2 className="text-lg font-bold text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-purple-600 dark:text-purple-400" />
            Quiz Thiên Kiến Hành Vi Đầu Tư (Behavioral Finance)
          </h2>
          <p className="text-xs text-zinc-600 dark:text-zinc-400 mt-0.5">
            Nhận diện 5 cạm bẫy tâm lý kinh điển và lưu nguyên lý vào Flashcard FSRS để rèn luyện thói quen đầu tư lý trí.
          </p>
        </div>
        <div className="text-xs font-semibold px-3 py-1.5 rounded-lg bg-purple-50 dark:bg-purple-950/60 text-purple-700 dark:text-purple-300 border border-purple-200 dark:border-purple-800">
          Đã làm: {totalAnswered}/{questions.length} · Đúng: {correctCount}/{questions.length}
        </div>
      </div>

      {toastMessage && (
        <div className="p-3 rounded-lg bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-300 dark:border-emerald-800 text-emerald-800 dark:text-emerald-200 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 text-emerald-600" />
          <span>{toastMessage}</span>
        </div>
      )}

      <div className="space-y-4">
        {questions.map((q) => {
          const selected = userAnswers[q.id];
          const hasAnswered = selected !== undefined;
          const isCorrect = selected === q.correctIndex;
          const isCardSaved = savedFlashcards[q.id];

          return (
            <div
              key={q.id}
              className="p-4 rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 space-y-3"
            >
              <div className="flex items-center justify-between">
                <span className="text-xs font-bold text-zinc-900 dark:text-zinc-100 uppercase tracking-wide">
                  {q.biasTitle}
                </span>
                {hasAnswered && (
                  <span className={`text-[11px] font-bold px-2 py-0.5 rounded ${
                    isCorrect
                      ? "bg-emerald-100 dark:bg-emerald-900/60 text-emerald-700 dark:text-emerald-300"
                      : "bg-red-100 dark:bg-red-900/60 text-red-700 dark:text-red-300"
                  }`}>
                    {isCorrect ? "Đúng (+20 điểm)" : "Chưa chính xác"}
                  </span>
                )}
              </div>

              <p className="text-xs text-zinc-700 dark:text-zinc-300 leading-relaxed">
                {q.scenario}
              </p>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 pt-1">
                {q.options.map((optText, optIdx) => {
                  const isThisSelected = selected === optIdx;
                  return (
                    <button
                      key={optIdx}
                      type="button"
                      onClick={() => handleSelectAnswer(q.id, optIdx)}
                      className={`p-2.5 rounded-lg border text-left text-xs transition cursor-pointer ${
                        isThisSelected
                          ? isThisSelected === (optIdx === q.correctIndex)
                            ? "border-emerald-500 bg-emerald-50/80 dark:bg-emerald-950/40 text-emerald-900 dark:text-emerald-100 font-semibold"
                            : "border-red-500 bg-red-50/80 dark:bg-red-950/40 text-red-900 dark:text-red-100 font-semibold"
                          : "border-zinc-200 dark:border-zinc-800 hover:bg-zinc-50 dark:hover:bg-zinc-800/60 text-zinc-700 dark:text-zinc-300"
                      }`}
                    >
                      {optText}
                    </button>
                  );
                })}
              </div>

              {showExplanations[q.id] && (
                <div className={`p-3 rounded-lg border text-xs leading-relaxed space-y-2 ${
                  isCorrect
                    ? "bg-emerald-50/60 border-emerald-200 dark:bg-emerald-950/30 dark:border-emerald-900/40 text-emerald-900 dark:text-emerald-200"
                    : "bg-amber-50/60 border-amber-200 dark:bg-amber-950/30 dark:border-amber-900/40 text-amber-900 dark:text-amber-200"
                }`}>
                  <p>
                    <strong>Nguyên lý cốt lõi:</strong> {q.explanation}
                  </p>
                  <div className="flex items-center justify-between pt-1">
                    <span className="text-[11px] text-zinc-500">
                      Ôn tập thường xuyên giúp hình thành phản xạ kỷ luật trước biến động giá.
                    </span>
                    <button
                      type="button"
                      disabled={isCardSaved}
                      onClick={() => handleSaveFlashcard(q)}
                      className="px-3 py-1 rounded-md bg-purple-600 hover:bg-purple-500 disabled:opacity-50 text-white text-[11px] font-semibold transition cursor-pointer flex items-center gap-1.5"
                    >
                      <Sparkles className="w-3.5 h-3.5" />
                      {isCardSaved ? "Đã lưu vào FSRS" : "Lưu nguyên lý vào Flashcard FSRS"}
                    </button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

export function FinancePracticeLab({
  courseId,
  courseTitle,
  lessonId,
  lessonTitle,
  lessonMarkdown,
  onSaveToNotebook,
  onCreateFlashcardQuick,
  initialActivityType,
  initialActivityKey,
}: PracticeModuleProps) {
  // Tự động nhận diện công cụ phù hợp với nội dung bài học
  const defaultTab = useMemo(
    () => detectDefaultTab(courseTitle, lessonTitle, lessonMarkdown),
    [courseTitle, lessonTitle, lessonMarkdown]
  );

  const resolvedInitialTab = useMemo(() => {
    const resolved = resolvePracticeSelection(
      initialActivityType,
      initialActivityKey,
      courseTitle,
      lessonTitle,
      lessonMarkdown
    );
    return resolved?.labMode === "finance" && resolved.financeTab ? resolved.financeTab : null;
  }, [initialActivityType, initialActivityKey, courseTitle, lessonTitle, lessonMarkdown]);

  const [activeTab, setActiveTab] = useState<ActiveTab>(resolvedInitialTab ?? defaultTab);
  const [dbActivities, setDbActivities] = useState<any[]>([]);
  const [practiceAttempts, setPracticeAttempts] = useState<any[]>([]);
  const [isLoadingBackend, setIsLoadingBackend] = useState(false);

  // Cập nhật tab khi đổi bài học hoặc có tham số điều hướng
  useEffect(() => {
    setActiveTab(resolvedInitialTab ?? defaultTab);
  }, [resolvedInitialTab, defaultTab]);

  useEffect(() => {
    let isMounted = true;
    async function loadBackendPractice() {
      setIsLoadingBackend(true);
      try {
        const headers = getClientRequestHeaders();
        const actUrl = `/api/v1/practice/activities?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}`;
        const attUrl = `/api/v1/practice/attempts?courseId=${encodeURIComponent(courseId)}${lessonId ? `&lessonId=${encodeURIComponent(lessonId)}` : ""}`;

        const [actRes, attRes] = await Promise.all([
          fetch(actUrl, { headers }).catch(() => null),
          fetch(attUrl, { headers }).catch(() => null),
        ]);

        if (actRes && actRes.ok) {
          const actData = await actRes.json();
          if (isMounted && Array.isArray(actData)) {
            setDbActivities(actData);
          }
        }
        if (attRes && attRes.ok) {
          const attData = await attRes.json();
          if (isMounted && Array.isArray(attData)) {
            setPracticeAttempts(attData);
          }
        }
      } catch {
        // ignore
      } finally {
        if (isMounted) setIsLoadingBackend(false);
      }
    }
    loadBackendPractice();
    return () => {
      isMounted = false;
    };
  }, [courseId, lessonId]);

  const recordPracticeAttempt = useCallback(
    async (params: {
      mode: string;
      score: number;
      maxScore?: number;
      isPassed?: boolean;
      details?: any;
      durationSeconds?: number;
      activityTitle?: string;
      activityKey?: string;
    }) => {
      try {
        const headers = getClientRequestHeaders();
        const dbAct = dbActivities.find(a => a.type === params.mode || a.id === params.activityKey);
        const passed = params.isPassed ?? (params.score >= (dbAct?.passingScore ?? 70));
        const payload = {
          activityId: dbAct?.id || null,
          courseId,
          lessonId: lessonId || null,
          activityType: params.mode,
          activityKey: params.activityKey || `${courseId}-${lessonId || "general"}-${params.mode}`,
          activityTitle: params.activityTitle || `${lessonTitle || courseTitle} · ${params.mode}`,
          score: Math.max(0, Math.min(params.maxScore || 100, Math.round(params.score))),
          maxScore: params.maxScore || 100,
          isPassed: passed,
          detailJson: JSON.stringify(params.details || {}),
          durationSeconds: params.durationSeconds || null,
        };

        const res = await fetch("/api/v1/practice/attempts", {
          method: "POST",
          headers: {
            ...headers,
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        });

        if (res.ok) {
          const saved = await res.json();
          setPracticeAttempts(prev => [saved, ...prev]);
          invalidateDashboardCache();
        }
      } catch {
        // ignore
      }
    },
    [courseId, lessonId, lessonTitle, courseTitle, dbActivities]
  );

  return (
    <div
      data-testid="finance-practice-lab"
      className="w-full space-y-6 p-4 sm:p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 shadow-sm transition-all"
    >
      {/* Thanh điều hướng 7 Tab công cụ */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-zinc-200 dark:border-zinc-800 pb-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="p-2 rounded-xl bg-indigo-50 dark:bg-indigo-950/60 text-indigo-600 dark:text-indigo-400">
              <Wallet className="w-5 h-5" />
            </span>
            <div>
              <h1 className="text-base sm:text-lg font-bold">
                Module Thực Hành Tài Chính Cá Nhân & Đầu Tư
              </h1>
              <p className="text-xs text-zinc-600 dark:text-zinc-400">
                Khóa học: <span className="font-semibold text-zinc-800 dark:text-zinc-200">{courseTitle}</span> · Bài:{" "}
                <span className="font-semibold text-zinc-800 dark:text-zinc-200">{lessonTitle}</span>
              </p>
            </div>
          </div>
        </div>

        {/* Thanh tab cuộn mượt / responsive */}
        <div className="flex items-center flex-wrap gap-1.5 p-1 rounded-xl bg-zinc-100 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 max-w-full">
          <button
            type="button"
            data-testid="tab-budget"
            onClick={() => setActiveTab("budget")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "budget"
                ? "bg-white dark:bg-zinc-800 text-indigo-600 dark:text-indigo-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <PieChart className="w-4 h-4" />
            1. Ngân Sách 50/30/20
          </button>
          <button
            type="button"
            data-testid="tab-compound"
            onClick={() => setActiveTab("compound")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "compound"
                ? "bg-white dark:bg-zinc-800 text-emerald-600 dark:text-emerald-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <TrendingUp className="w-4 h-4" />
            2. Lãi Kép & Lạm Phát
          </button>
          <button
            type="button"
            data-testid="tab-debt"
            onClick={() => setActiveTab("debt")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "debt"
                ? "bg-white dark:bg-zinc-800 text-indigo-600 dark:text-indigo-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <Scale className="w-4 h-4" />
            3. Lộ Trình Trả Nợ
          </button>
          <button
            type="button"
            data-testid="tab-trading"
            onClick={() => setActiveTab("trading")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "trading"
                ? "bg-white dark:bg-zinc-800 text-indigo-600 dark:text-indigo-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <CandlestickChart className="w-4 h-4" />
            4. Biểu Đồ Nến & Rủi Ro
          </button>
          <button
            type="button"
            data-testid="tab-cases"
            onClick={() => setActiveTab("cases")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "cases"
                ? "bg-white dark:bg-zinc-800 text-purple-600 dark:text-purple-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <Briefcase className="w-4 h-4" />
            5. Tình Huống Bài Học (Case Study)
          </button>
          <button
            type="button"
            data-testid="tab-portfolio"
            onClick={() => setActiveTab("portfolio")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "portfolio"
                ? "bg-white dark:bg-zinc-800 text-blue-600 dark:text-blue-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <SlidersHorizontal className="w-4 h-4" />
            6. Mô Phỏng Danh Mục (MẪU)
          </button>
          <button
            type="button"
            data-testid="tab-behavioral"
            onClick={() => setActiveTab("behavioral")}
            className={`min-h-[44px] px-3.5 py-2 rounded-lg text-xs font-semibold whitespace-nowrap transition-all flex items-center gap-2 ${
              activeTab === "behavioral"
                ? "bg-white dark:bg-zinc-800 text-amber-600 dark:text-amber-300 shadow-sm"
                : "text-zinc-600 dark:text-zinc-400 hover:text-zinc-900 dark:hover:text-zinc-200"
            }`}
          >
            <Sparkles className="w-4 h-4" />
            7. Quiz Thiên Kiến Đầu Tư
          </button>
        </div>
      </div>

      {/* Hiển thị công cụ tương ứng */}
      <div>
        {activeTab === "budget" && <BudgetLab onSaveToNotebook={onSaveToNotebook} />}
        {activeTab === "compound" && (
          <CompoundInterestLab
            onSaveToNotebook={onSaveToNotebook}
            onCreateFlashcardQuick={onCreateFlashcardQuick}
            onRecordAttempt={recordPracticeAttempt}
          />
        )}
        {activeTab === "debt" && <DebtPayoffLab onSaveToNotebook={onSaveToNotebook} />}
        {activeTab === "trading" && (
          <CandlestickRiskLab
            onSaveToNotebook={onSaveToNotebook}
            onCreateFlashcardQuick={onCreateFlashcardQuick}
          />
        )}
        {activeTab === "cases" && (
          <ExtractedFinanceCaseStudyLab
            lessonMarkdown={lessonMarkdown}
            lessonTitle={lessonTitle}
            onSaveToNotebook={onSaveToNotebook}
            onCreateFlashcardQuick={onCreateFlashcardQuick}
          />
        )}
        {activeTab === "portfolio" && (
          <PortfolioSimulatorLab
            onSaveToNotebook={onSaveToNotebook}
            onRecordAttempt={recordPracticeAttempt}
          />
        )}
        {activeTab === "behavioral" && (
          <BehavioralFinanceQuizLab
            onSaveToNotebook={onSaveToNotebook}
            onCreateFlashcardQuick={onCreateFlashcardQuick}
            onRecordAttempt={recordPracticeAttempt}
          />
        )}
      </div>
    </div>
  );
}

export default FinancePracticeLab;
