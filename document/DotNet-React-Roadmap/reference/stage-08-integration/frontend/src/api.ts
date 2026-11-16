export type ImportStatus = 'committed' | 'committed-with-postcommit-error' | 'cancelled' | 'rejected' | 'unknown';
export type ImportError = { lineNumber: number | null; code: string; message: string };
export type ImportResponse = { status: ImportStatus; readRows: number | null; acceptedRows: number | null; skippedDuplicates: number | null; errors: ImportError[]; traceId: string };
export type ExpenseDto = { id: number; categoryId: 1 | 2 | 7; amount: string; spentOn: string; note: string };
export class ImportApiError extends Error { constructor(public readonly httpStatus: number, public readonly response?: ImportResponse, message = 'Yêu cầu nhập dữ liệu không thành công.') { super(message); this.name = 'ImportApiError'; } }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null;
const nullableCount = (v: unknown, name: string): number | null => { if (v === null) return null; if (typeof v !== 'number' || !Number.isSafeInteger(v) || v < 0) throw new Error(`Phản hồi không hợp lệ: ${name}.`); return v; };
export function validateApiOrigin(value: string): string { if (!value) return ''; let url: URL; try { url = new URL(value); } catch { throw new Error('VITE_API_ORIGIN phải là origin HTTP loopback hợp lệ.'); } if (url.protocol !== 'http:' || url.hostname !== '127.0.0.1' || url.port === '' || Number(url.port) < 1 || Number(url.port) > 65535 || url.username || url.password || url.pathname !== '/' || url.search || url.hash) throw new Error('VITE_API_ORIGIN phải là http://127.0.0.1:<port>, không có credential hoặc path.'); return url.origin; }
const API_ORIGIN = validateApiOrigin((import.meta as ImportMeta & { env?: Record<string, string> }).env?.VITE_API_ORIGIN ?? '');
const moneyText = (v: unknown): string => { if (typeof v !== 'string' || !/^(?:0|[1-9]\d{0,15})(?:\.\d{2})$/.test(v)) throw new Error('Phản hồi amount không hợp lệ.'); const [whole, fraction] = v.split('.'); return `${whole}.${fraction}`; };
const dateText = (v: unknown): string => { if (typeof v !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(v)) throw new Error('Phản hồi spentOn không hợp lệ.'); const [y,m,d] = v.split('-').map(Number); if (y < 1 || y > 9999 || m < 1 || m > 12 || d < 1) throw new Error('Phản hồi spentOn không hợp lệ.'); const leap = y % 4 === 0 && (y % 100 !== 0 || y % 400 === 0); const days = [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]; if (d > days[m - 1]) throw new Error('Phản hồi spentOn không hợp lệ.'); return v; };
export function parseExpense(value: unknown): ExpenseDto { if (!record(value)) throw new Error('Phản hồi expense không hợp lệ.'); if (typeof value.id !== 'number' || !Number.isSafeInteger(value.id) || value.id < 1) throw new Error('Phản hồi id không hợp lệ.'); if (value.categoryId !== 1 && value.categoryId !== 2 && value.categoryId !== 7) throw new Error('Phản hồi categoryId không hợp lệ.'); if (typeof value.note !== 'string' || value.note.length > 500) throw new Error('Phản hồi note không hợp lệ.'); return { id: value.id, categoryId: value.categoryId, amount: moneyText(value.amount), spentOn: dateText(value.spentOn), note: value.note }; }
export function parseImportResponse(value: unknown): ImportResponse {
  if (!record(value) || typeof value.status !== 'string' || !['committed','committed-with-postcommit-error','cancelled','rejected','unknown'].includes(value.status)) throw new Error('Phản hồi nhập dữ liệu không hợp lệ.');
  if (!Array.isArray(value.errors) || !value.errors.every((e) => record(e) && (e.lineNumber === null || (typeof e.lineNumber === 'number' && Number.isInteger(e.lineNumber) && e.lineNumber > 0)) && typeof e.code === 'string' && typeof e.message === 'string')) throw new Error('Phản hồi lỗi nhập dữ liệu không hợp lệ.');
  if (typeof value.traceId !== 'string' || value.traceId.length === 0) throw new Error('Phản hồi thiếu mã theo dõi.');
  const result: ImportResponse = { status: value.status as ImportStatus, readRows: nullableCount(value.readRows, 'readRows'), acceptedRows: nullableCount(value.acceptedRows, 'acceptedRows'), skippedDuplicates: nullableCount(value.skippedDuplicates, 'skippedDuplicates'), errors: value.errors as ImportError[], traceId: value.traceId };
  if (result.status !== 'unknown' && [result.readRows, result.acceptedRows, result.skippedDuplicates].some((x) => x === null)) throw new Error('Phản hồi trạng thái xác nhận phải có số lượng.');
  if (result.status === 'unknown' && [result.readRows, result.acceptedRows, result.skippedDuplicates].some((x) => x !== null)) throw new Error('Phản hồi chưa xác nhận phải để trống số lượng.');
  if (result.status === 'cancelled' && (result.readRows !== 0 || result.acceptedRows !== 0 || result.skippedDuplicates !== 0)) throw new Error('Phản hồi đã hủy phải có số lượng bằng không.');
  if (result.status === 'rejected' && result.acceptedRows !== 0) throw new Error('Phản hồi bị từ chối không được có dòng đã nhận.');
  if (result.status !== 'unknown' && result.readRows !== null && result.acceptedRows !== null && result.skippedDuplicates !== null && (result.acceptedRows > result.readRows || result.skippedDuplicates > result.readRows || result.acceptedRows + result.skippedDuplicates > result.readRows)) throw new Error('Phản hồi số lượng không hợp lệ.');
  return result;
}
function assertStatusAgreement(httpStatus: number, status: ImportStatus): void {
  const valid = httpStatus === 200 ? status === 'committed' || status === 'committed-with-postcommit-error' :
    httpStatus === 400 || httpStatus === 413 ? status === 'rejected' :
    httpStatus === 499 ? status === 'cancelled' : httpStatus === 500 ? status === 'unknown' : false;
  if (!valid) throw new Error('Phản hồi HTTP và trạng thái nhập không khớp.');
}
export async function fetchExpenses(signal: AbortSignal, base = ''): Promise<ExpenseDto[]> { const response = await fetch(`${base || ''}/api/v1/expenses`, { signal }); let payload: unknown; try { payload = await response.json(); } catch { throw new Error('Phản hồi danh sách không hợp lệ.'); } if (!response.ok || !Array.isArray(payload)) throw new Error('Không thể tải danh sách chi tiêu.'); return payload.map(parseExpense); }
export async function importExpenses(file: File, signal: AbortSignal, base = ''): Promise<ImportResponse> {
  const form = new FormData(); form.append('file', file, file.name);
  let response: Response;
  try { response = await fetch(`${base || ''}/api/v1/expenses/import`, { method: 'POST', body: form, signal }); } catch (error) { throw error; }
  let payload: unknown; try { payload = await response.json(); } catch { throw new ImportApiError(response.status, undefined); }
  let parsed: ImportResponse; try { parsed = parseImportResponse(payload); } catch { throw new ImportApiError(response.status, undefined); }
  try { assertStatusAgreement(response.status, parsed.status); } catch { throw new ImportApiError(response.status, undefined); }
  return parsed;
}
export const getExpenses = fetchExpenses;
