// Let's test the exact regex and logic from LessonMarkdownView.tsx

function cleanStudyInlineArtifacts(raw) {
  if (!raw) return raw;
  return raw
    .replace(/\[LO-[A-Z0-9-]+\]\s*(?:\([^)]+\)\s*:?\s*)?/gi, "")
    .replace(/\s*\[C\d{1,3}-\d{1,3}[a-z]?(?:\s*,\s*C\d{1,3}-\d{1,3}[a-z]?)*\](?!\()/gi, "")
    .replace(/\s*(?:🟢|🟡|🔴|⚪)\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo|Strong|Moderate|Weak|Controversial|Verified)[^\]]*\](?!\()/gi, "")
    .replace(/\s*\[(?:Mạnh|Khá|Trung bình|Yếu|Tranh luận|Cảnh báo)\](?!\()/gi, "")
    .replace(/^.*\(AI executor\).*$/gim, "")
    .replace(/^.*HANDOFF.*$/gim, "")
    .replace(/\*\*\s*\*\*/g, "")
    .replace(/\s+([.,;:!?])/g, "$1")
    .replace(/\(\s*\)/g, "")
    .replace(/ {2,}/g, " ")
    .trim();
}

function parseStyledTextRun(segment, style) {
  if (!segment) return [];
  const tokens = [];
  const pattern =
    /(\*\*\*([^\n*]+?)\*\*\*|\*\*([^\n*]+?)\*\*|__([^\n_]+?)__|~~([^\n~]+?)~~|\*([^\s*][^\n*]*?[^\s*]|[^\s*])\*|(?<![a-zA-Z0-9_])_([^\s_][^\n_]*?[^\s_]|[^\s_])_(?![a-zA-Z0-9_])|https?:\/\/[^\s<>()]+[^\s<>().,;:!?"')\]])/g;

  let lastIdx = 0;
  let match;
  while ((match = pattern.exec(segment)) !== null) {
    if (match.index > lastIdx) {
      tokens.push({
        kind: "text",
        text: segment.slice(lastIdx, match.index),
        ...style
      });
    }

    const full = match[0];
    if (match[2] !== undefined) {
      tokens.push(...parseStyledTextRun(match[2], { ...style, bold: true, italic: true }));
    } else if (match[3] !== undefined) {
      tokens.push(...parseStyledTextRun(match[3], { ...style, bold: true }));
    } else if (match[4] !== undefined) {
      tokens.push(...parseStyledTextRun(match[4], { ...style, bold: true }));
    } else if (match[5] !== undefined) {
      tokens.push(...parseStyledTextRun(match[5], { ...style, strike: true }));
    } else if (match[6] !== undefined) {
      tokens.push(...parseStyledTextRun(match[6], { ...style, italic: true }));
    } else if (match[7] !== undefined) {
      tokens.push(...parseStyledTextRun(match[7], { ...style, italic: true }));
    } else if (/^https?:\/\//i.test(full)) {
      tokens.push({
        kind: "link",
        text: full,
        href: full,
        isExternal: true,
        ...style
      });
    }

    lastIdx = pattern.lastIndex;
  }

  if (lastIdx < segment.length) {
    tokens.push({
      kind: "text",
      text: segment.slice(lastIdx),
      ...style
    });
  }

  return tokens;
}

function isDeadOrInternalMarkdown(path) {
  const norm = path.toLowerCase().replace(/\\/g, "/").split("/").pop() || "";
  return /^(handoff.*\.md|implementation_plan.*\.md|roadmap.*\.md|agents.*\.md|review_report.*\.md)$/i.test(norm);
}

function tokenizeInlineMarkdown(rawText) {
  if (!rawText) return [];

  // Step 1: Protect inline code
  const codeItems = [];
  const withCodePlaceholders = rawText.replace(/(`+)([\s\S]*?)\1/g, (_m, _ticks, inner) => {
    const idx = codeItems.length;
    codeItems.push({ code: inner.trim() });
    return `\uE000CODE_${idx}\uE000`;
  });

  // Step 2: Protect images and links
  const linkItems = [];
  const linkRegex = /(!\[([^\]]*)\]\(([^)\s]+(?:\s+"[^"]*")?)\)|\[([^\]]+)\]\(([^)]+)\))/g;

  const withAllPlaceholders = withCodePlaceholders.replace(
    linkRegex,
    (full, _m1, imgAlt, imgUrl, linkText, linkHref) => {
      const label = linkText ?? "";
      const rawHref = (linkHref ?? "").trim();
      if (isDeadOrInternalMarkdown(rawHref)) {
        return label;
      }
      const idx = linkItems.length;
      linkItems.push({ kind: "link", text: label, href: rawHref });
      return `\uE000LINK_${idx}\uE000`;
    }
  );

  console.log('withCodePlaceholders:', withCodePlaceholders);
  console.log('withAllPlaceholders:', withAllPlaceholders);

  // Step 3
  const styledTokens = parseStyledTextRun(withAllPlaceholders, {});
  console.log('styledTokens:', JSON.stringify(styledTokens, null, 2));

  // Step 4
  const finalTokens = [];
  for (const token of styledTokens) {
    if (token.kind !== "text") {
      finalTokens.push(token);
      continue;
    }

    const segments = token.text.split(/(\uE000(?:CODE|LINK)_\d+\uE000)/g);
    for (const seg of segments) {
      if (!seg) continue;
      const codeMatch = seg.match(/^\uE000CODE_(\d+)\uE000$/);
      if (codeMatch) {
        const cIdx = parseInt(codeMatch[1], 10);
        const item = codeItems[cIdx];
        if (item) {
          finalTokens.push({
            kind: "code",
            text: item.code,
            bold: token.bold,
            italic: token.italic,
          });
          continue;
        }
      }
      finalTokens.push({
        ...token,
        text: seg,
      });
    }
  }
  return finalTokens;
}

// What was the raw line in IELTS?
// Let's test what produces "Gói hiện hành: sau Phase 2, mở CODE_0 để theo lịch Month 1 hiện có;..."
const rawLine = "Gói hiện hành: sau Phase 2, mở `roadmap.md` để theo lịch Month 1 hiện có; các phần chưa có hoặc chưa được nghiệm thu vẫn giữ trạng thái chờ.";
console.log('=== TEST WITH CODE ONLY ===');
const tokens = tokenizeInlineMarkdown(rawLine);
console.log('Tokens:', tokens);

const rawWithDeadLink = "Gói hiện hành: sau Phase 2, mở [`roadmap.md`](roadmap.md) để theo lịch Month 1 hiện có; các phần chưa có hoặc chưa được nghiệm thu vẫn giữ trạng thái chờ.";
console.log('\n=== TEST WITH [`roadmap.md`](roadmap.md) ===');
const tokensDead = tokenizeInlineMarkdown(rawWithDeadLink);
console.log('Tokens Dead:', tokensDead);
