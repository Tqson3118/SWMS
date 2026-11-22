/**
 * Markdown utilities and text sanitization for SWMS lesson reader.
 */

export function cleanStudyInlineArtifacts(raw: string): string {
  if (!raw) return raw;
  return raw
    // Strip leftover Unicode Private Use Area characters and control characters
    .replace(/[\uE000-\uF8FF\u001A]/g, "")
    // Hide Learning Objective metadata prefixes like `[LO-W01-01] (Nhớ/Hiểu):` while keeping the bolded verb intact
    .replace(/\[LO-[A-Z0-9-]+\]\s*(?:\([^)]+\)\s*:?\s*)?/gi, "")
    // Hide evidence claim references like [C01-01], [C00-01a, C01-01], [C03-08] (preserve Markdown links `[label](href)`)
    .replace(/\s*\[C\d{1,3}-\d{1,3}[a-z]?(?:\s*,\s*C\d{1,3}-\d{1,3}[a-z]?)*\](?!\()/gi, "")
    // Hide evidence strength / audit badges like 🟢 [Mạnh], 🟡 [Khá], 🔴 [Tranh luận]
    .replace(
      /\s*(?:🟢|🟡|🔴|⚪)\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo|Strong|Moderate|Weak|Controversial|Verified)[^\]]*\](?!\()/gi,
      ""
    )
    .replace(/\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo)\](?!\()/gi, "")
    // Hide developer internal work tasks / AI executor rows
    .replace(/^.*\(AI executor\).*$/gim, "")
    .replace(/^.*HANDOFF.*$/gim, "")
    // Clean orphaned bold markers / whitespace before punctuation
    .replace(/\*\*\s*\*\*/g, "")
    .replace(/\s+([.,;:!?])/g, "$1")
    .replace(/\(\s*\)/g, "")
    .replace(/ {2,}/g, " ")
    .trim();
}

export function stripYamlFrontmatter(raw: string): string {
  const normalized = raw.replace(/^\uFEFF/, "");
  const match = normalized.match(/^---\r?\n[\s\S]*?\r?\n---(?:\r?\n|$)/);
  if (match) {
    return normalized.slice(match[0].length).trimStart();
  }
  return normalized;
}

export function stripDiacritics(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase();
}

export function slugifyHeadingText(text: string, idx: number): string {
  const base = text
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-")
    .slice(0, 48);
  return base ? `sec-${base}-${idx}` : `sec-heading-${idx}`;
}

export function resolveRelativeCoursePath(
  sourceRelativePath: string | null | undefined,
  href: string
): string {
  const cleanHref = href.split("#")[0].split("?")[0].replace(/\\/g, "/").trim();
  if (!cleanHref) return "";
  if (cleanHref.startsWith("/")) {
    return cleanHref.replace(/^\/+/, "");
  }
  const normalizedSource = (sourceRelativePath || "").replace(/\\/g, "/");
  const lastSlash = normalizedSource.lastIndexOf("/");
  const baseDir = lastSlash >= 0 ? normalizedSource.slice(0, lastSlash) : "";
  const parts = baseDir ? baseDir.split("/").filter(Boolean) : [];
  for (const seg of cleanHref.split("/")) {
    if (!seg || seg === ".") continue;
    if (seg === "..") {
      if (parts.length > 0) parts.pop();
    } else {
      parts.push(seg);
    }
  }
  return parts.join("/");
}

export function sanitizeSafeUrl(url: string | undefined | null): string {
  if (!url) return "#";
  const raw = url.trim();
  if (!raw) return "#";

  let normalized = raw.replace(/[\u0000-\u001F\u007F\s]+/g, "");
  try {
    const decoded = decodeURIComponent(normalized);
    normalized = decoded.replace(/[\u0000-\u001F\u007F\s]+/g, "");
  } catch {
    // Keep normalized string if malformed URI escape
  }

  const lower = normalized.toLowerCase();
  if (
    lower.startsWith("javascript:") ||
    lower.startsWith("data:") ||
    lower.startsWith("vbscript:")
  ) {
    return "#";
  }

  return raw;
}

export function isDeadOrInternalMarkdown(path: string): boolean {
  if (!path) return false;
  const clean = path.replace(/[`"'()]/g, "").trim();
  const norm = clean.toLowerCase().replace(/\\/g, "/").split("/").pop() || "";
  return /^(handoff.*\.md|implementation_plan.*\.md|roadmap.*\.md|agents.*\.md|review_report.*\.md|prompt.*\.md)$/i.test(
    norm
  );
}

export function isAudioPath(href: string): boolean {
  const clean = href.split("#")[0].split("?")[0].trim().toLowerCase();
  return /\.(wav|mp3|ogg|m4a)$/.test(clean);
}

export function isMarkdownPath(href: string): boolean {
  const clean = href.split("#")[0].split("?")[0].replace(/[`"'()]/g, "").trim().toLowerCase();
  return /\.md$/.test(clean);
}

export function isExternalUrl(href: string): boolean {
  return /^https?:\/\//i.test(href.trim()) || /^mailto:/i.test(href.trim());
}

export function buildDocumentAssetUrl(sourceFolder: string, resolvedPath: string): string {
  const encodedSegments = resolvedPath
    .split("/")
    .filter(Boolean)
    .map((s) => encodeURIComponent(s))
    .join("/");
  return `/api/v1/document-assets/${encodeURIComponent(sourceFolder)}/${encodedSegments}`;
}

export function inferPrecedingWord(fullText: string, ipaSlice: string): string {
  const idx = fullText.indexOf(ipaSlice);
  if (idx <= 0) return ipaSlice.replace(/\//g, "").trim();
  const before = fullText.slice(Math.max(0, idx - 60), idx).trim();
  const words = before.split(/[\s,;:!?"'()[\]{}]+/).filter(Boolean);
  const last = words[words.length - 1];
  if (last && /^[a-zA-Z'-]{2,}$/.test(last)) {
    return last;
  }
  return ipaSlice.replace(/\//g, "").trim();
}

export function normalizeTitleForComparison(input: string): string {
  return input
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .replace(/[*_`~#]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, " ")
    .trim();
}
