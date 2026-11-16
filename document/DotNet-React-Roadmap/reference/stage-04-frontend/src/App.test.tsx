import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { App } from './App';
import { fetchExpenses } from './api';

vi.mock('./api', () => ({
  fetchCategories: vi.fn().mockResolvedValue([{ id: 1, name: 'Ăn uống' }]),
  fetchMonthlySummary: vi.fn().mockResolvedValue({ year: 2026, month: 10, grandTotal: 0, totalCount: 0, categories: [] }),
  fetchExpenses: vi.fn(),
  createCategory: vi.fn(),
  deleteExpense: vi.fn()
}));

const row = (id: number) => ({ id, categoryId: 1, categoryName: 'Ăn uống', amount: '100.00', spentOn: '2026-10-01', note: null });
const page = (id: number) => ({ items: [row(id)], totalCount: 1, page: 1, pageSize: 10, totalPages: 1 });

describe('App stale refresh lifecycle', () => {
  it('keeps the newest filter response when an older request resolves later', async () => {
    let resolveOld!: (value: ReturnType<typeof page>) => void;
    let resolveNew!: (value: ReturnType<typeof page>) => void;
    vi.mocked(fetchExpenses)
      .mockImplementationOnce(() => new Promise((resolve) => { resolveOld = resolve; }))
      .mockImplementationOnce(() => new Promise((resolve) => { resolveNew = resolve; }));
    render(<App />);
    await waitFor(() => expect(fetchExpenses).toHaveBeenCalledTimes(1));
    fireEvent.change(screen.getByLabelText('Từ ngày:'), { target: { value: '2026-10-02' } });
    fireEvent.submit(screen.getByRole('button', { name: 'Lọc dữ liệu' }).closest('form')!);
    await waitFor(() => expect(fetchExpenses).toHaveBeenCalledTimes(2));
    resolveNew(page(2));
    await waitFor(() => expect(screen.getByText('#2')).toBeInTheDocument());
    resolveOld(page(1));
    await waitFor(() => expect(screen.getByText('#2')).toBeInTheDocument());
    expect(screen.queryByText('#1')).not.toBeInTheDocument();
  });
});
