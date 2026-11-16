import { describe, expect, it } from 'vitest';
import { centsToMoney, formatMoney, moneyToCents } from './money';

describe('exact frontend money model', () => {
  it('keeps a fraction and maximum exact as decimal strings and BigInt cents', () => {
    expect(moneyToCents('125.50')).toBe(12550n);
    expect(centsToMoney(999999999999999998n)).toBe('9999999999999999.98');
    expect(formatMoney('125.50')).toContain('125,50');
    expect(formatMoney('125.50')).not.toContain('126');
  });

  it('supports aggregate totals beyond one SQL decimal(18,2) row', () => {
    const maximum = moneyToCents('9999999999999999.99');
    expect(centsToMoney(maximum + maximum)).toBe('19999999999999999.98');
  });

  it.each(['-1.00', '1.001', '1e3', 'NaN', '10000000000000000.00'])('rejects unsafe money syntax %s', (value) => {
    expect(() => moneyToCents(value)).toThrow();
  });
});
