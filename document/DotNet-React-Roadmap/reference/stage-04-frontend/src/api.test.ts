import { afterEach, describe, expect, it, vi } from 'vitest';
import { fetchExpenses, fetchMonthlySummary, parseExpense } from './api';

describe('API runtime contract failures', () => {
  afterEach(() => vi.restoreAllMocks());
  it('rejects malformed non-array expense payloads instead of casting them', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ items: [] }), { status: 200, headers: { 'content-type': 'application/json' } })));
    await expect(fetchExpenses(1, 10)).rejects.toThrow('must be an array');
  });

  it.each([
    ['missing pagination header', new Headers({ 'X-Total-Count': '0', 'X-Page': '1', 'X-Page-Size': '10' })],
    ['zero page', new Headers({ 'X-Total-Count': '1', 'X-Page': '0', 'X-Page-Size': '10', 'X-Total-Pages': '1' })],
  ])('rejects %s', async (_name, headers) => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('[]', { status: 200, headers })));
    await expect(fetchExpenses(1, 10)).rejects.toThrow(/pagination header|pagination headers/);
  });

  it.each([
    [{ ...({ id: 0, categoryId: 1, categoryName: 'x', amount: '1.00', spentOn: '2026-01-01', note: null }) }, 'id'],
    [{ ...({ id: 1, categoryId: 1, categoryName: 'x', amount: '-1.00', spentOn: '2026-01-01', note: null }) }, 'amount'],
    [{ ...({ id: 1, categoryId: 1, categoryName: 'x', amount: '1.00', spentOn: '2026-02-29', note: null }) }, 'spentOn'],
  ])('rejects invalid expense field %s', (value) => expect(() => parseExpense(value)).toThrow(/invalid|nonnegative/));

  it('accepts the minimum DateOnly year and applies Gregorian leap-year rules', () => {
    expect(parseExpense({ id: 1, categoryId: 1, categoryName: 'x', amount: '1.00', spentOn: '0001-01-01', note: null }).spentOn).toBe('0001-01-01');
    expect(() => parseExpense({ id: 1, categoryId: 1, categoryName: 'x', amount: '1.00', spentOn: '0100-02-29', note: null })).toThrow(/invalid/);
  });

  it('rejects a summary with an invalid date domain or count', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ year: 2026, month: 13, grandTotal: 0, totalCount: -1, categories: [] }), { status: 200, headers: { 'content-type': 'application/json' } })));
    await expect(fetchMonthlySummary(2026, 10)).rejects.toThrow(/summary date|response field/);
  });

  it('rejects a summary whose exact category sums do not match grandTotal', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ year: 2026, month: 7, grandTotal: '2.00', totalCount: 1, categories: [{ categoryId: 1, categoryName: 'x', total: '1.00', count: 1 }] }), { status: 200, headers: { 'content-type': 'application/json' } })));
    await expect(fetchMonthlySummary(2026, 7)).rejects.toThrow(/totals do not match/);
  });
});
