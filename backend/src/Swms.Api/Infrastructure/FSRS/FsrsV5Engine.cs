namespace Swms.Api.Infrastructure.FSRS;

public record FsrsCardState(
    double Stability,
    double Difficulty,
    int Reps,
    int Lapses,
    int State, // 0=New, 1=Learning, 2=Review, 3=Relearning
    DateTimeOffset? LastReviewAt,
    DateTimeOffset DueAt
);

public record FsrsScheduleOutput(
    double Stability,
    double Difficulty,
    double Retrievability,
    double ScheduledDays,
    double ElapsedDays,
    int Reps,
    int Lapses,
    int State,
    DateTimeOffset ReviewedAt,
    DateTimeOffset NextDueAt
);

public record FsrsReviewEventInput(
    Guid ReviewEventId,
    Guid FlashcardId,
    int Rating, // 1=Again, 2=Hard, 3=Good, 4=Easy
    DateTimeOffset ClientTimestamp,
    int? ReviewDurationMs
);

public static class FsrsV5Engine
{
    public static readonly double[] DefaultWeights =
    [
        0.4072, 1.1829, 3.1262, 15.4722, 7.2102,
        0.5316, 1.0651, 0.0234, 1.616, 0.1544,
        1.0824, 1.9813, 0.0953, 0.2975, 2.2042,
        0.2407, 2.9466, 0.5034, 0.6567
    ];

    private const double Decay = -0.5;
    private static readonly double Factor = Math.Pow(0.9, 1.0 / Decay) - 1.0; // 19.0 / 81.0

    /// <summary>
    /// Asymmetric Server-Authoritative Clock Validation (Decision J & ADR-010):
    /// - Upper Bound: client_timestamp <= server_now + 10 minutes (clamps future-dated reviews to server_now)
    /// - Lower Bound: client_timestamp >= server_now - 30 days (rejects stale >30d timestamps by clamping to lower bound or throwing)
    /// </summary>
    public static DateTimeOffset ValidateAndNormalizeTimestamp(
        DateTimeOffset clientTimestamp,
        DateTimeOffset serverNow,
        out bool wasFutureClamped)
    {
        wasFutureClamped = false;
        var upperBound = serverNow.AddMinutes(10);
        var lowerBound = serverNow.AddDays(-30);

        if (clientTimestamp > upperBound)
        {
            wasFutureClamped = true;
            return serverNow;
        }

        if (clientTimestamp < lowerBound)
        {
            throw new ArgumentOutOfRangeException(
                nameof(clientTimestamp),
                $"Review timestamp {clientTimestamp:O} is older than the 30-day offline window relative to server time {serverNow:O}.");
        }

        return clientTimestamp;
    }

    public static FsrsScheduleOutput Evaluate(
        FsrsCardState card,
        int rating,
        DateTimeOffset reviewTime,
        double[]? customWeights = null,
        double desiredRetention = 0.90,
        int maxIntervalDays = 36500)
    {
        if (rating is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 (Again) and 4 (Easy).");

        var w = customWeights is { Length: 19 } ? customWeights : DefaultWeights;

        double elapsedDays = 0.0;
        if (card.LastReviewAt.HasValue && card.State != 0)
        {
            elapsedDays = Math.Max(0.0, (reviewTime - card.LastReviewAt.Value).TotalDays);
        }

        double newS;
        double newD;
        double retrievability;
        int newState;
        int newLapses = card.Lapses;
        int newReps = card.Reps + 1;

        if (card.State == 0 || card.Stability <= 0.0)
        {
            // Initial review of a New card
            newS = InitialStability(w, rating);
            newD = InitialDifficulty(w, rating);
            retrievability = 1.0;
            newState = rating == 1 ? 1 : 2;
        }
        else
        {
            retrievability = ForgettingCurve(elapsedDays, card.Stability);
            newD = NextDifficulty(w, card.Difficulty, rating);

            if (elapsedDays < 1.0 && card.State is 1 or 3)
            {
                // Short-term / same-day learning step (FSRS v5 w17, w18)
                newS = ShortTermStability(w, card.Stability, rating);
            }
            else if (rating == 1)
            {
                // Forget / Lapse
                newS = NextForgetStability(w, newD, card.Stability, retrievability);
            }
            else
            {
                // Recall (Hard=2, Good=3, Easy=4)
                newS = NextRecallStability(w, newD, card.Stability, retrievability, rating);
            }

            if (rating == 1)
            {
                newLapses++;
                newState = card.State == 2 ? 3 : 1; // Relearning or Learning
            }
            else
            {
                newState = 2; // Review
            }
        }

        newS = Round4(Math.Clamp(newS, 0.01, maxIntervalDays));
        newD = Round4(Math.Clamp(newD, 1.0, 10.0));
        retrievability = Round4(retrievability);

        double scheduledDays;
        if (rating == 1)
        {
            // Again: learning step (< 10m = ~0.0069 days, represented as fractional or 0.01d for interval preview)
            scheduledDays = Round4(Math.Max(0.0069, NextIntervalDays(newS, desiredRetention, maxIntervalDays) * 0.1));
        }
        else
        {
            scheduledDays = Round4(NextIntervalDays(newS, desiredRetention, maxIntervalDays));
        }

        var nextDueAt = rating == 1
            ? reviewTime.AddMinutes(10)
            : reviewTime.AddDays(Math.Max(1.0, Math.Round(scheduledDays)));

        return new FsrsScheduleOutput(
            Stability: newS,
            Difficulty: newD,
            Retrievability: retrievability,
            ScheduledDays: scheduledDays,
            ElapsedDays: Round4(elapsedDays),
            Reps: newReps,
            Lapses: newLapses,
            State: newState,
            ReviewedAt: reviewTime,
            NextDueAt: nextDueAt
        );
    }

    /// <summary>
    /// Deterministic Chronological Replay (Decision J & ADR-018):
    /// Replays a sequence of review events from initial state sorted by ReviewedAt.
    /// </summary>
    public static FsrsCardState ReplayChronologically(
        IEnumerable<(int Rating, DateTimeOffset ReviewedAt)> events,
        double[]? customWeights = null,
        double desiredRetention = 0.90)
    {
        var current = new FsrsCardState(0, 0, 0, 0, 0, null, DateTimeOffset.UtcNow);
        foreach (var ev in events.OrderBy(e => e.ReviewedAt))
        {
            var res = Evaluate(current, ev.Rating, ev.ReviewedAt, customWeights, desiredRetention);
            current = new FsrsCardState(
                res.Stability,
                res.Difficulty,
                res.Reps,
                res.Lapses,
                res.State,
                res.ReviewedAt,
                res.NextDueAt
            );
        }
        return current;
    }

    public static double InitialStability(double[] w, int rating) => w[rating - 1];

    public static double InitialDifficulty(double[] w, int rating)
    {
        var d0 = w[4] - Math.Exp(w[5] * (rating - 1.0)) + 1.0;
        return Math.Clamp(d0, 1.0, 10.0);
    }

    public static double NextDifficulty(double[] w, double d, int rating)
    {
        var deltaD = -w[6] * (rating - 3.0);
        var dPrime = d + deltaD * ((10.0 - d) / 9.0);
        var d0Easy = InitialDifficulty(w, 4);
        var meanReverted = w[7] * d0Easy + (1.0 - w[7]) * dPrime;
        return Math.Clamp(meanReverted, 1.0, 10.0);
    }

    public static double ForgettingCurve(double elapsedDays, double stability)
    {
        if (stability <= 0.0) return 0.0;
        return Math.Pow(1.0 + Factor * (elapsedDays / stability), Decay);
    }

    public static double ShortTermStability(double[] w, double s, int rating)
    {
        return s * Math.Exp(w[17] * (rating - 3.0 + w[18]));
    }

    public static double NextRecallStability(double[] w, double d, double s, double r, int rating)
    {
        var hardPenalty = rating == 2 ? w[15] : 1.0;
        var easyBonus = rating == 4 ? w[16] : 1.0;
        var inc = Math.Exp(w[8]) *
                  (11.0 - d) *
                  Math.Pow(s, -w[9]) *
                  (Math.Exp(w[10] * (1.0 - r)) - 1.0) *
                  hardPenalty *
                  easyBonus;
        return s * (1.0 + inc);
    }

    public static double NextForgetStability(double[] w, double d, double s, double r)
    {
        var sf = w[11] *
                 Math.Pow(d, -w[12]) *
                 (Math.Pow(s + 1.0, w[13]) - 1.0) *
                 Math.Exp(w[14] * (1.0 - r));
        return Math.Min(s, sf);
    }

    public static double NextIntervalDays(double s, double desiredRetention, int maxInterval)
    {
        var raw = (s / Factor) * (Math.Pow(desiredRetention, 1.0 / Decay) - 1.0);
        return Math.Clamp(raw, 1.0, maxInterval);
    }

    private static double Round4(double val) => Math.Round(val, 4, MidpointRounding.AwayFromZero);
}
