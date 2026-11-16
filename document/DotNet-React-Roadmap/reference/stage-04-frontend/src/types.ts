export interface ExpenseResponse {
  id: number;
  categoryId: number;
  categoryName: string;
  amount: string;
  spentOn: string; // ISO Date YYYY-MM-DD
  note: string | null;
}

export interface CreateExpenseRequest {
  categoryId: number;
  amount: string;
  spentOn: string;
  note?: string | null;
}

export type UpdateExpenseRequest = CreateExpenseRequest;

export interface CategoryResponse {
  id: number;
  name: string;
}

export interface CategoryMonthlySummary {
  categoryId: number;
  categoryName: string;
  total: string;
  count: number;
}

export interface MonthlySummaryResponse {
  year: number;
  month: number;
  grandTotal: string;
  totalCount: number;
  categories: CategoryMonthlySummary[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ValidationProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

export type RequestState = 'idle' | 'loading' | 'ready' | 'error';
