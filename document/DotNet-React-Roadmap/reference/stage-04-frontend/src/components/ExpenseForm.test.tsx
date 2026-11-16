import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ExpenseForm } from './ExpenseForm';
import { createExpense, updateExpense } from '../api';

vi.mock('../api', () => ({ createExpense: vi.fn(), updateExpense: vi.fn() }));
const category = { id: 1, name: 'Ăn uống' };
const expense = { id: 7, categoryId: 1, categoryName: 'Ăn uống', amount: '10000.00', spentOn: '2026-10-01', note: 'giữ lại' };

describe('ExpenseForm async lifecycle', () => {
  beforeEach(() => vi.clearAllMocks());

  it('preserves edit fields and shows the server error after a failed PUT', async () => {
    vi.mocked(updateExpense).mockRejectedValueOnce(new Error('validation failed'));
    const user = userEvent.setup();
    render(<ExpenseForm categories={[category]} categoryState="ready" categoryError={null} initialExpense={expense} onExpenseSaved={vi.fn()} onCategoryCreated={vi.fn()} />);
    const amount = screen.getByLabelText('Số tiền (VNĐ):');
    await user.clear(amount); await user.type(amount, '12345.67');
    await user.click(screen.getByRole('button', { name: 'Lưu thay đổi' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('validation failed');
    expect(amount).toHaveValue('12345.67');
    expect(updateExpense).toHaveBeenCalledWith(7, expect.objectContaining({ amount: '12345.67' }), expect.any(AbortSignal));
    expect(screen.getByLabelText('Ngày chi tiêu:')).toHaveValue('2026-10-01');
  });

  it('aborts a delayed POST when the form unmounts', async () => {
    let resolveCreate!: (value: typeof expense) => void;
    vi.mocked(createExpense).mockImplementation((_data, signal) => new Promise((resolve, reject) => {
      signal?.addEventListener('abort', () => reject(new DOMException('aborted', 'AbortError')));
      resolveCreate = resolve;
    }));
    const user = userEvent.setup();
    const onSaved = vi.fn().mockResolvedValue(undefined);
    const view = render(<ExpenseForm categories={[category]} categoryState="ready" categoryError={null} initialExpense={null} onExpenseSaved={onSaved} onCategoryCreated={vi.fn()} />);
    await user.type(screen.getByLabelText('Số tiền (VNĐ):'), '200');
    await user.click(screen.getByRole('button', { name: 'Thêm chi tiêu' }));
    expect(await screen.findByRole('button', { name: 'Đang lưu...' })).toBeDisabled();
    view.unmount();
    resolveCreate(expense);
    await waitFor(() => expect(onSaved).not.toHaveBeenCalled());
  });
});
