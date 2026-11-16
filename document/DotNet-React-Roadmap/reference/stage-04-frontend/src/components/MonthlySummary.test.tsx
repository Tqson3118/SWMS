import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { MonthlySummary } from './MonthlySummary';

describe('MonthlySummary exact labels', () => {
  it('renders fractional money without Number rounding', () => {
    render(<MonthlySummary summary={{ year: 2026, month: 7, grandTotal: '19999999999999999.98', totalCount: 2, categories: [{ categoryId: 1, categoryName: 'Ăn uống', total: '9999999999999999.99', count: 1 }, { categoryId: 2, categoryName: 'Đi lại', total: '9999999999999999.99', count: 1 }] }} year={2026} month={7} state="ready" error={null} onMonthChange={() => undefined} />);
    expect(screen.getAllByText(/19\.999\.999\.999\.999\.999,98/).length).toBeGreaterThan(0);
    expect(screen.queryByText(/20\.000\.000\.000\.000\.000/)).not.toBeInTheDocument();
  });
});
