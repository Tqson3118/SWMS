export type MoneyString = string;
export const MAX_MONEY_CENTS = 999999999999999999n;
const MONEY_TEXT = /^(?:0|[1-9]\d*)(?:\.\d{1,2})?$/;

function decimalToCents(value: string, field: string, maximum?: bigint): bigint {
  if (!MONEY_TEXT.test(value)) throw new Error(`${field} must be a nonnegative decimal with at most 2 fractional digits.`);
  const [whole, fraction = ''] = value.split('.');
  const cents = BigInt(whole) * 100n + BigInt((fraction + '00').slice(0, 2));
  if (maximum !== undefined && cents > maximum) throw new Error(`${field} exceeds decimal(18,2).`);
  return cents;
}
export function moneyToCents(value: string, field = 'amount'): bigint { return decimalToCents(value, field, MAX_MONEY_CENTS); }
export function aggregateToCents(value: string, field = 'total'): bigint { return decimalToCents(value, field); }

export function legacyNumberToMoney(value: number, field: string): MoneyString {
  if (!Number.isFinite(value) || value < 0 || value > Number.MAX_SAFE_INTEGER / 100 || Math.abs(value * 100 - Math.round(value * 100)) > 1e-7) throw new Error(`${field} numeric value is unsafe; send an exact decimal string.`);
  const text = value.toFixed(2);
  moneyToCents(text, field);
  return text;
}

export function readMoney(value: unknown, field: string): MoneyString {
  if (typeof value === 'string') { moneyToCents(value, field); return value; }
  if (typeof value === 'number') return legacyNumberToMoney(value, field);
  throw new Error(`API response field ${field} is invalid.`);
}
export function readAggregateMoney(value: unknown, field: string): MoneyString {
  if (typeof value === 'string') { aggregateToCents(value, field); return value; }
  if (typeof value === 'number') return legacyNumberToMoney(value, field);
  throw new Error(`API response field ${field} is invalid.`);
}

export function centsToMoney(cents: bigint): MoneyString {
  if (cents < 0n) throw new Error('Money cannot be negative.');
  const whole = cents / 100n; const fraction = (cents % 100n).toString().padStart(2, '0');
  return `${whole}.${fraction}`;
}

export function formatMoney(value: MoneyString): string {
  const cents = aggregateToCents(value); const whole = cents / 100n; const fraction = (cents % 100n).toString().padStart(2, '0');
  const grouped = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 }).format(whole);
  return `${grouped},${fraction}\u00a0₫`;
}

export function approximatePercent(part: MoneyString, total: MoneyString): number {
  const numerator = aggregateToCents(part) * 10000n; const denominator = aggregateToCents(total);
  return denominator === 0n ? 0 : Number(numerator / denominator) / 100;
}
