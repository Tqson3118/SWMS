export interface ReviewCapSettings {
  maxReviewsPerDay: number; // default 100
  maxNewPerDay: number;     // default 20
}

const STORAGE_KEY = "studxy_daily_review_caps";

export const DEFAULT_REVIEW_CAPS: ReviewCapSettings = {
  maxReviewsPerDay: 100,
  maxNewPerDay: 20,
};

export function getReviewCapSettings(): ReviewCapSettings {
  if (typeof window === "undefined") return DEFAULT_REVIEW_CAPS;
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      return {
        maxReviewsPerDay: Math.max(10, Number(parsed.maxReviewsPerDay) || 100),
        maxNewPerDay: Math.max(5, Number(parsed.maxNewPerDay) || 20),
      };
    }
  } catch {
    // ignore parse error
  }
  return DEFAULT_REVIEW_CAPS;
}

export function saveReviewCapSettings(settings: ReviewCapSettings): void {
  if (typeof window === "undefined") return;
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
    window.dispatchEvent(new CustomEvent("studxy:review-caps-updated", { detail: settings }));
  } catch {
    // ignore
  }
}

const COMMON_VIETNAMESE_SURNAMES = new Set([
  "nguyen",
  "tran",
  "le",
  "pham",
  "hoang",
  "huynh",
  "phan",
  "vu",
  "vo",
  "dang",
  "bui",
  "do",
  "ho",
  "ngo",
  "duong",
  "ly",
  "dinh",
  "doan",
  "lam"
]);

function normalizeVn(str: string): string {
  return str
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

/**
 * Quy tắc xưng hô Studxy:
 * Nếu token cuối là họ Việt phổ biến (vd "Sơn Trần") -> lấy token đầu ("Sơn").
 * Ngược lại lấy token cuối (vd "Nguyễn Minh Anh" -> "Anh").
 * Không có tên -> "bạn".
 */
export function getVietnameseGreetingName(fullName?: string | null): string {
  if (!fullName || !fullName.trim()) return "bạn";
  const tokens = fullName.trim().split(/\s+/);
  if (tokens.length === 1) return tokens[0];

  const lastTokenNorm = normalizeVn(tokens[tokens.length - 1]);
  if (COMMON_VIETNAMESE_SURNAMES.has(lastTokenNorm)) {
    return tokens[0];
  }
  return tokens[tokens.length - 1];
}
