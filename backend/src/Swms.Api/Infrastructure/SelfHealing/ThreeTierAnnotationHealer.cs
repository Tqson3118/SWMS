using System.Text.Json;
using Swms.Api.Domain.Entities;

namespace Swms.Api.Infrastructure.SelfHealing;

public record BlockTextSegment(Guid BlockId, string Text);

public record HealingResult(
    Guid AnnotationId,
    string PreviousStatus,
    string NewStatus,
    string TierUsed, // Tier1_Exact, Tier2_CoordinateShift, Tier3_SmithWaterman, Ambiguous_NeedsReview, Orphaned
    Guid? NewBlockId,
    int NewStartOffset,
    int NewEndOffset,
    string UpdatedExactQuote,
    double ConfidenceScore
);

/// <summary>
/// Server-Side 3-Tier Self-Healing Engine (Decision H & ADR-008):
/// - Preserves pristine lessons.content_ast (NEVER injects student marks into shared master AST).
/// - Preserves immutable OriginalExactQuote while updating ExactQuote/PrefixQuote/SuffixQuote on Tier 3 typo healing.
/// - Flags ambiguous multiple matches (<90% confidence) as 'needs_review' and deleted passages as 'orphaned'.
/// </summary>
public static class ThreeTierAnnotationHealer
{
    private const int ShiftSearchWindowRadius = 250;
    private const double SmithWatermanThreshold = 0.85;

    public static List<BlockTextSegment> ExtractBlocksFromAst(string contentAstJson)
    {
        var blocks = new List<BlockTextSegment>();
        try
        {
            using var doc = JsonDocument.Parse(contentAstJson);
            if (doc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var node in contentArray.EnumerateArray())
                {
                    Guid blockId = Guid.Empty;
                    if (node.TryGetProperty("attrs", out var attrs) &&
                        attrs.TryGetProperty("block_id", out var bidProp) &&
                        Guid.TryParse(bidProp.GetString(), out var parsedId))
                    {
                        blockId = parsedId;
                    }

                    var sb = new System.Text.StringBuilder();
                    if (node.TryGetProperty("content", out var inlineArr) && inlineArr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in inlineArr.EnumerateArray())
                        {
                            if (child.TryGetProperty("text", out var tProp))
                            {
                                sb.Append(tProp.GetString());
                            }
                        }
                    }

                    if (blockId != Guid.Empty && sb.Length > 0)
                    {
                        blocks.Add(new BlockTextSegment(blockId, sb.ToString()));
                    }
                }
            }
        }
        catch
        {
            // Fallback empty block list if AST is malformed
        }
        return blocks;
    }

    public static HealingResult HealAnnotation(
        Annotation annotation,
        string oldPlainText,
        string newContentAstJson,
        string newPlainText)
    {
        var prevStatus = annotation.Status;
        var blocks = ExtractBlocksFromAst(newContentAstJson);
        if (blocks.Count == 0 && !string.IsNullOrWhiteSpace(newPlainText))
        {
            blocks.Add(new BlockTextSegment(annotation.BlockId ?? Guid.NewGuid(), newPlainText));
        }

        var targetQuote = annotation.ExactQuote;
        if (string.IsNullOrWhiteSpace(targetQuote))
        {
            annotation.Status = "orphaned";
            annotation.UpdatedAt = DateTimeOffset.UtcNow;
            return new HealingResult(annotation.Id, prevStatus, "orphaned", "Orphaned", annotation.BlockId, 0, 0, string.Empty, 0.0);
        }

        // ====================================================================
        // TIER 1: Block-Scoped Exact Match & Context Disambiguation (W3C Selector)
        // ====================================================================
        var targetBlock = blocks.FirstOrDefault(b => b.BlockId == annotation.BlockId);
        if (targetBlock != null)
        {
            var occurrences = FindAllOccurrences(targetBlock.Text, targetQuote);
            if (occurrences.Count == 1)
            {
                var start = occurrences[0];
                var end = start + targetQuote.Length;
                UpdateAnchoredAnnotation(annotation, targetBlock.BlockId, targetBlock.Text, start, end, targetQuote);
                return new HealingResult(annotation.Id, prevStatus, "anchored", "Tier1_Exact", targetBlock.BlockId, start, end, targetQuote, 1.0);
            }
            if (occurrences.Count > 1)
            {
                var disambiguated = TryDisambiguateByContext(targetBlock.Text, occurrences, targetQuote.Length, annotation.PrefixQuote, annotation.SuffixQuote);
                if (disambiguated.HasValue && disambiguated.Value.Confidence >= 0.90)
                {
                    var start = disambiguated.Value.Offset;
                    var end = start + targetQuote.Length;
                    UpdateAnchoredAnnotation(annotation, targetBlock.BlockId, targetBlock.Text, start, end, targetQuote);
                    return new HealingResult(annotation.Id, prevStatus, "anchored", "Tier1_Exact", targetBlock.BlockId, start, end, targetQuote, disambiguated.Value.Confidence);
                }
                else
                {
                    // Ambiguous duplicate quotes without sufficient prefix/suffix disambiguation -> needs_review
                    annotation.Status = "needs_review";
                    annotation.UpdatedAt = DateTimeOffset.UtcNow;
                    return new HealingResult(annotation.Id, prevStatus, "needs_review", "Ambiguous_NeedsReview", targetBlock.BlockId, occurrences[0], occurrences[0] + targetQuote.Length, targetQuote, 0.5);
                }
            }
        }

        // ====================================================================
        // TIER 2: Coordinate Shift Projection via Prefix/Suffix Diff Alignment (±250ch)
        // ====================================================================
        if (targetBlock != null)
        {
            var shiftVector = EstimateCoordinateShift(oldPlainText, targetBlock.Text, annotation.StartOffset);
            var projectedCenter = Math.Clamp(annotation.StartOffset + shiftVector, 0, targetBlock.Text.Length);
            var winStart = Math.Max(0, projectedCenter - ShiftSearchWindowRadius);
            var winEnd = Math.Min(targetBlock.Text.Length, projectedCenter + targetQuote.Length + ShiftSearchWindowRadius);
            if (winEnd > winStart)
            {
                var windowText = targetBlock.Text.Substring(winStart, winEnd - winStart);
                var relIdx = windowText.IndexOf(targetQuote, StringComparison.Ordinal);
                if (relIdx >= 0)
                {
                    var absStart = winStart + relIdx;
                    var absEnd = absStart + targetQuote.Length;
                    UpdateAnchoredAnnotation(annotation, targetBlock.BlockId, targetBlock.Text, absStart, absEnd, targetQuote);
                    return new HealingResult(annotation.Id, prevStatus, "anchored", "Tier2_CoordinateShift", targetBlock.BlockId, absStart, absEnd, targetQuote, 0.96);
                }
            }
        }

        // ====================================================================
        // TIER 3: Local Fuzzy Alignment (Smith-Waterman / Normalized Similarity >= 0.85)
        // ====================================================================
        var candidateBlocks = targetBlock != null ? new[] { targetBlock }.Concat(blocks.Where(b => b.BlockId != targetBlock.BlockId)) : blocks;
        foreach (var blk in candidateBlocks)
        {
            var (bestStart, bestLength, similarity) = FindBestFuzzySubstring(blk.Text, targetQuote);
            if (similarity >= SmithWatermanThreshold && bestLength > 0)
            {
                var healedQuote = blk.Text.Substring(bestStart, bestLength);
                var absEnd = bestStart + bestLength;
                UpdateAnchoredAnnotation(annotation, blk.BlockId, blk.Text, bestStart, absEnd, healedQuote);
                return new HealingResult(annotation.Id, prevStatus, "anchored", "Tier3_SmithWaterman", blk.BlockId, bestStart, absEnd, healedQuote, Math.Round(similarity, 4));
            }
        }

        // ====================================================================
        // GLOBAL FALLBACK ACROSS OTHER BLOCKS (Paragraph Reordering)
        // ====================================================================
        var globalMatches = new List<(BlockTextSegment Block, int Offset)>();
        foreach (var blk in blocks)
        {
            foreach (var occ in FindAllOccurrences(blk.Text, targetQuote))
            {
                globalMatches.Add((blk, occ));
            }
        }

        if (globalMatches.Count == 1)
        {
            var match = globalMatches[0];
            UpdateAnchoredAnnotation(annotation, match.Block.BlockId, match.Block.Text, match.Offset, match.Offset + targetQuote.Length, targetQuote);
            return new HealingResult(annotation.Id, prevStatus, "anchored", "Tier1_Exact", match.Block.BlockId, match.Offset, match.Offset + targetQuote.Length, targetQuote, 0.95);
        }
        if (globalMatches.Count > 1)
        {
            annotation.Status = "needs_review";
            annotation.UpdatedAt = DateTimeOffset.UtcNow;
            return new HealingResult(annotation.Id, prevStatus, "needs_review", "Ambiguous_NeedsReview", globalMatches[0].Block.BlockId, globalMatches[0].Offset, globalMatches[0].Offset + targetQuote.Length, targetQuote, 0.5);
        }

        // ====================================================================
        // UNRECOVERABLE / DELETED TEXT -> ORPHANED
        // ====================================================================
        annotation.Status = "orphaned";
        annotation.UpdatedAt = DateTimeOffset.UtcNow;
        return new HealingResult(annotation.Id, prevStatus, "orphaned", "Orphaned", annotation.BlockId, annotation.StartOffset, annotation.EndOffset, annotation.ExactQuote, 0.0);
    }

    private static void UpdateAnchoredAnnotation(
        Annotation annotation,
        Guid blockId,
        string blockText,
        int startOffset,
        int endOffset,
        string healedExactQuote)
    {
        annotation.BlockId = blockId;
        annotation.StartOffset = startOffset;
        annotation.EndOffset = endOffset;
        // OriginalExactQuote is strictly immutable; ExactQuote, PrefixQuote, SuffixQuote update to healed text (ADR-008)
        annotation.ExactQuote = healedExactQuote;
        var prefixStart = Math.Max(0, startOffset - 32);
        annotation.PrefixQuote = blockText.Substring(prefixStart, startOffset - prefixStart);
        var suffixLen = Math.Min(32, Math.Max(0, blockText.Length - endOffset));
        annotation.SuffixQuote = blockText.Substring(endOffset, suffixLen);
        annotation.Status = "anchored";
        annotation.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static List<int> FindAllOccurrences(string text, string pattern)
    {
        var list = new List<int>();
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern)) return list;
        int idx = 0;
        while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) != -1)
        {
            list.Add(idx);
            idx += 1;
        }
        return list;
    }

    private static (int Offset, double Confidence)? TryDisambiguateByContext(
        string text,
        List<int> occurrences,
        int quoteLen,
        string? prefixQuote,
        string? suffixQuote)
    {
        if (string.IsNullOrEmpty(prefixQuote) && string.IsNullOrEmpty(suffixQuote))
            return null;

        int bestOffset = -1;
        double bestScore = -1.0;
        double secondBestScore = -1.0;

        foreach (var pos in occurrences)
        {
            var actualPrefixStart = Math.Max(0, pos - (prefixQuote?.Length ?? 0));
            var actualPrefix = text.Substring(actualPrefixStart, pos - actualPrefixStart);

            var afterPos = pos + quoteLen;
            var actualSuffixLen = Math.Min(suffixQuote?.Length ?? 0, Math.Max(0, text.Length - afterPos));
            var actualSuffix = text.Substring(afterPos, actualSuffixLen);

            double prefixSim = string.IsNullOrEmpty(prefixQuote) ? 0.5 : ComputeNormalizedSimilarity(actualPrefix, prefixQuote);
            double suffixSim = string.IsNullOrEmpty(suffixQuote) ? 0.5 : ComputeNormalizedSimilarity(actualSuffix, suffixQuote);
            double combined = (prefixSim + suffixSim) / 2.0;

            if (combined > bestScore)
            {
                secondBestScore = bestScore;
                bestScore = combined;
                bestOffset = pos;
            }
            else if (combined > secondBestScore)
            {
                secondBestScore = combined;
            }
        }

        // If two occurrences have nearly identical context scores, confidence is < 90%
        if (bestScore >= 0.90 && (bestScore - secondBestScore) >= 0.15)
        {
            return (bestOffset, bestScore);
        }

        return null;
    }

    private static int EstimateCoordinateShift(string oldText, string newText, int oldOffset)
    {
        if (string.IsNullOrEmpty(oldText) || string.IsNullOrEmpty(newText)) return 0;
        return newText.Length - oldText.Length;
    }

    private static (string NfcText, int[] NfcToRawStart, int[] NfcToRawEnd) NormalizeNfcWithMapping(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return (string.Empty, [0], [0]);

        var sb = new System.Text.StringBuilder(raw.Length);
        var starts = new List<int>(raw.Length + 1);
        var ends = new List<int>(raw.Length + 1);

        var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(raw);
        while (enumerator.MoveNext())
        {
            int rawStart = enumerator.ElementIndex;
            string element = enumerator.GetTextElement();
            int rawEnd = rawStart + element.Length;
            string nfcElement = element.Normalize(System.Text.NormalizationForm.FormC);

            for (int k = 0; k < nfcElement.Length; k++)
            {
                sb.Append(nfcElement[k]);
                starts.Add(rawStart);
                ends.Add(rawEnd);
            }
        }
        starts.Add(raw.Length);
        ends.Add(raw.Length);

        return (sb.ToString(), starts.ToArray(), ends.ToArray());
    }

    private static (int BestStart, int BestLength, double Similarity) FindBestFuzzySubstring(string text, string query)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
            return (0, 0, 0.0);

        var (nfcText, nfcToRawStart, nfcToRawEnd) = NormalizeNfcWithMapping(text);
        var nfcQuery = query.Normalize(System.Text.NormalizationForm.FormC);

        int n = nfcText.Length;
        int m = nfcQuery.Length;
        if (n == 0 || m == 0 || n < m / 2)
            return (0, 0, 0.0);

        // Fast path: if NFC normalized query occurs verbatim inside NFC normalized text (e.g., pure NFD <-> NFC difference)
        int exactNfcIdx = nfcText.IndexOf(nfcQuery, StringComparison.Ordinal);
        if (exactNfcIdx >= 0)
        {
            int rawStartExact = nfcToRawStart[exactNfcIdx];
            int rawEndExact = nfcToRawEnd[exactNfcIdx + m - 1];
            return (rawStartExact, rawEndExact - rawStartExact, 1.0);
        }

        // Seller's Semi-Global Edit Distance DP over NFC code units (O(N * M))
        // dp[i, j] = minimum edit distance between nfcQuery[0..i) and a substring of nfcText ending at j
        var prevCost = new int[n + 1];
        var currCost = new int[n + 1];
        var prevStart = new int[n + 1];
        var currStart = new int[n + 1];

        for (int j = 0; j <= n; j++)
        {
            prevCost[j] = 0;
            prevStart[j] = j;
        }

        for (int i = 1; i <= m; i++)
        {
            currCost[0] = i;
            currStart[0] = 0;
            char qCh = nfcQuery[i - 1];

            for (int j = 1; j <= n; j++)
            {
                int subCost = prevCost[j - 1] + (qCh == nfcText[j - 1] ? 0 : 1);
                int delCost = prevCost[j] + 1;     // char in query deleted
                int insCost = currCost[j - 1] + 1; // extra char in text

                if (subCost <= delCost && subCost <= insCost)
                {
                    currCost[j] = subCost;
                    currStart[j] = prevStart[j - 1];
                }
                else if (delCost <= insCost)
                {
                    currCost[j] = delCost;
                    currStart[j] = prevStart[j];
                }
                else
                {
                    currCost[j] = insCost;
                    currStart[j] = currStart[j - 1];
                }
            }

            (prevCost, currCost) = (currCost, prevCost);
            (prevStart, currStart) = (currStart, prevStart);
        }

        int minDistance = int.MaxValue;
        int bestEndNfc = m;
        int bestStartNfc = 0;

        for (int j = 1; j <= n; j++)
        {
            if (prevCost[j] < minDistance)
            {
                minDistance = prevCost[j];
                bestEndNfc = j;
                bestStartNfc = prevStart[j];
            }
        }

        // Fine-tune start/end in NFC space around [bestStartNfc, bestEndNfc] to maximize normalized similarity
        double bestSim = 0.0;
        int finalStartNfc = bestStartNfc;
        int finalEndNfc = bestEndNfc;

        int sMin = Math.Max(0, bestStartNfc - 3);
        int sMax = Math.Min(n - 1, bestStartNfc + 3);
        int eMin = Math.Max(sMin + 1, bestEndNfc - 3);
        int eMax = Math.Min(n, bestEndNfc + 3);

        for (int s = sMin; s <= sMax; s++)
        {
            for (int e = Math.Max(s + 1, eMin); e <= eMax; e++)
            {
                var candidateNfc = nfcText.Substring(s, e - s);
                double sim = ComputeNormalizedSimilarityPreNormalized(candidateNfc, nfcQuery);
                if (sim > bestSim)
                {
                    bestSim = sim;
                    finalStartNfc = s;
                    finalEndNfc = e;
                }
            }
        }

        int rawBestStart = nfcToRawStart[finalStartNfc];
        int rawBestEnd = finalEndNfc > finalStartNfc ? nfcToRawEnd[finalEndNfc - 1] : rawBestStart;
        return (rawBestStart, Math.Max(0, rawBestEnd - rawBestStart), bestSim);
    }

    public static double ComputeNormalizedSimilarity(string a, string b)
    {
        if (a == b) return 1.0;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;

        var normA = a.Normalize(System.Text.NormalizationForm.FormC);
        var normB = b.Normalize(System.Text.NormalizationForm.FormC);
        return ComputeNormalizedSimilarityPreNormalized(normA, normB);
    }

    private static double ComputeNormalizedSimilarityPreNormalized(string normA, string normB)
    {
        if (normA == normB) return 1.0;
        if (string.IsNullOrEmpty(normA) || string.IsNullOrEmpty(normB)) return 0.0;

        int maxLen = Math.Max(normA.Length, normB.Length);
        int dist = LevenshteinDistance(normA, normB);
        return Math.Max(0.0, 1.0 - (double)dist / maxLen);
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        var prev = new int[m + 1];
        var curr = new int[m + 1];

        for (int j = 0; j <= m; j++) prev[j] = j;

        for (int i = 1; i <= n; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[m];
    }
}
