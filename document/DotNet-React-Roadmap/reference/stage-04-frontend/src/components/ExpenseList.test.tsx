import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ExpenseList } from './ExpenseList';

const item = { id: 9, categoryId: 1, categoryName: 'Ăn uống', amount: '10.00', spentOn: '2026-10-01', note: null };

describe('ExpenseList delete lifecycle', () => {
  it('aborts a delayed DELETE when the list unmounts', async () => {
    let receivedSignal!: AbortSignal;
    const onDelete = vi.fn((_id: number, signal?: AbortSignal) => { receivedSignal = signal!; return new Promise<void>(() => undefined); });
    const view = render(<ExpenseList expenses={[item]} totalCount={1} page={1} totalPages={1} state="ready" error={null} from="" to="" onPageChange={vi.fn()} onFilterChange={vi.fn()} onDelete={onDelete} onEdit={vi.fn()} />);
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    await userEvent.setup().click(screen.getByRole('button', { name: 'Xóa khoản chi tiêu #9' }));
    expect(receivedSignal.aborted).toBe(false);
    view.unmount();
    expect(receivedSignal.aborted).toBe(true);
    vi.restoreAllMocks();
  });
});
