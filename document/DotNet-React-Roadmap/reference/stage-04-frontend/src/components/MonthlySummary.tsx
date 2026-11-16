import React from 'react';
import { MonthlySummaryResponse, RequestState } from '../types';
import { approximatePercent, formatMoney } from '../money';

interface MonthlySummaryProps {
  summary: MonthlySummaryResponse | null;
  year: number;
  month: number;
  state: RequestState;
  error: string | null;
  onMonthChange: (year: number, month: number) => void;
}

export const MonthlySummary: React.FC<MonthlySummaryProps> = ({
  summary,
  year,
  month,
  state,
  error,
  onMonthChange,
}) => {
  const loading = state === 'loading';
  const currentYear = new Date().getFullYear();
  const years = [currentYear - 1, currentYear, currentYear + 1];
  const months = Array.from({ length: 12 }, (_, i) => i + 1);

  const formatCurrency = (val: string) => formatMoney(val);

  const chartColors = [
    '#3b82f6', // blue
    '#10b981', // green
    '#f59e0b', // amber
    '#ef4444', // red
    '#8b5cf6', // purple
    '#ec4899', // pink
    '#14b8a6', // teal
    '#6366f1', // indigo
  ];

  return (
    <section className="card summary-section" aria-labelledby="summary-title">
      <div className="summary-header">
        <h2 id="summary-title" className="section-title">Tổng Hợp Chi Tiêu Tháng</h2>

        <div className="month-picker">
          <label htmlFor="select-month" className="sr-only">Tháng</label>
          <select
            id="select-month"
            value={month}
            onChange={(e) => onMonthChange(year, Number(e.target.value))}
            disabled={loading}
          >
            {months.map((m) => (
              <option key={m} value={m}>
                Tháng {m}
              </option>
            ))}
          </select>

          <label htmlFor="select-year" className="sr-only">Năm</label>
          <select
            id="select-year"
            value={year}
            onChange={(e) => onMonthChange(Number(e.target.value), month)}
            disabled={loading}
          >
            {years.map((y) => (
              <option key={y} value={y}>
                Năm {y}
              </option>
            ))}
          </select>
        </div>
      </div>

      {error && <div className="alert alert-error" role="alert">{error}</div>}
      {loading ? (
        <div className="loading-state" role="status">
          Đang tổng hợp báo cáo...
        </div>
      ) : state === 'error' ? (
        <div className="empty-state" role="status">Không thể tải báo cáo. Hãy thử lại.</div>
      ) : !summary || summary.categories.length === 0 ? (
        <div className="empty-state">
          <p>Chưa có chi tiêu nào được ghi nhận trong Tháng {month}/{year}.</p>
        </div>
      ) : (
        <div className="summary-content">
          {/* Highlight KPI Cards */}
          <div className="kpi-grid">
            <div className="kpi-card">
              <span className="kpi-label">Tổng Chi Tiêu Tháng {month}/{year}</span>
              <span className="kpi-value text-primary">{formatCurrency(summary.grandTotal)}</span>
            </div>
            <div className="kpi-card">
              <span className="kpi-label">Tổng Số Giao Dịch</span>
              <span className="kpi-value">{summary.totalCount}</span>
            </div>
          </div>

          {/* Visual Distribution Chart */}
          <div className="chart-container">
            <h3 className="chart-title">Biểu Đồ Phân Bổ Chi Tiêu Theo Danh Mục</h3>
            
            {/* Horizontal Percentage Bar */}
            <div className="stacked-bar-chart" aria-label="Biểu đồ thanh phân bổ chi tiêu">
              {summary.categories.map((cat, idx) => {
                const percent = approximatePercent(cat.total, summary.grandTotal);
                if (percent <= 0) return null;
                const color = chartColors[idx % chartColors.length];
                return (
                  <div
                    key={cat.categoryId}
                    className="stacked-bar-segment"
                    style={{
                      width: `${percent}%`,
                      backgroundColor: color,
                    }}
                    title={`${cat.categoryName}: ${formatCurrency(cat.total)} (${percent.toFixed(1)}%)`}
                  />
                );
              })}
            </div>

            {/* Bar List with Percentages */}
            <div className="category-bars">
              {summary.categories.map((cat, idx) => {
                const percent = approximatePercent(cat.total, summary.grandTotal);
                const color = chartColors[idx % chartColors.length];
                return (
                  <div key={cat.categoryId} className="category-bar-row">
                    <div className="cat-bar-header">
                      <span className="cat-indicator" style={{ backgroundColor: color }} />
                      <span className="cat-name">{cat.categoryName}</span>
                      <span className="cat-amount">{formatCurrency(cat.total)}</span>
                      <span className="cat-percent">({percent.toFixed(1)}%)</span>
                    </div>
                    <div className="progress-track">
                      <div
                        className="progress-fill"
                        style={{ width: `${percent}%`, backgroundColor: color }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Summary Details Table */}
          <div className="table-responsive">
            <table className="summary-table">
              <thead>
                <tr>
                  <th>Danh mục</th>
                  <th>Số khoản chi</th>
                  <th>Tổng tiền</th>
                  <th>Tỷ trọng</th>
                </tr>
              </thead>
              <tbody>
                {summary.categories.map((cat) => {
                  const percent = approximatePercent(cat.total, summary.grandTotal);
                  return (
                    <tr key={cat.categoryId}>
                      <td>
                        <strong>{cat.categoryName}</strong>
                      </td>
                      <td>{cat.count}</td>
                      <td className="amount-cell">{formatCurrency(cat.total)}</td>
                      <td>{percent.toFixed(1)}%</td>
                    </tr>
                  );
                })}
              </tbody>
              <tfoot>
                <tr className="summary-footer-row">
                  <td><strong>Tổng cộng</strong></td>
                  <td><strong>{summary.totalCount}</strong></td>
                  <td className="amount-cell"><strong>{formatCurrency(summary.grandTotal)}</strong></td>
                  <td><strong>100%</strong></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      )}
    </section>
  );
};
