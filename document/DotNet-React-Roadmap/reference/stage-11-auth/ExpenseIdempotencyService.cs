namespace OrderManagement;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Data.Common;

public sealed record IdempotencyOperationResult(int ExpenseId, decimal Amount, string Note, bool IsReplay);
public sealed class IdempotencyConflictException : Exception { public IdempotencyConflictException() : base("The idempotency key was already used for a different request.") { } }
public sealed class IdempotencyUnknownOutcomeException : Exception { public IdempotencyUnknownOutcomeException(Exception inner) : base("The idempotent operation outcome could not be determined.", inner) { } }

public sealed class ExpenseIdempotencyService
{
    private const int MaxHeaderLength = 128;
    private const int MaxNoteLength = 500;
    private const decimal MaxAmount = 1_000_000m;
    private const string ReceiptIndexName = "UX_ExpenseIdempotencyReceipts_Owner_Key";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ExpenseIdempotencyService> logger;

    public ExpenseIdempotencyService(IServiceScopeFactory scopeFactory, ILogger<ExpenseIdempotencyService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    public async Task<IdempotencyOperationResult> CreateOrReplayAsync(
        string ownerUserId,
        IReadOnlyCollection<string?> keyValues,
        decimal amount,
        string note,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId)) throw new ArgumentException("Owner is required.", nameof(ownerUserId));
        var key = NormalizeIdempotencyKey(keyValues);
        var hash = ComputeRequestHash(amount, note);
        cancellationToken.ThrowIfCancellationRequested();
        var state = new SaveDispatchState();
        try
        {
            return await PersistOnceAsync(ownerUserId, key, hash, amount, note, state, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsIntendedReceiptDuplicate(exception))
        {
            var sql = FindSqlException(exception)!;
            logger.LogWarning("idempotency_duplicate_sql number={Number} index={Index}", sql.Number, ReceiptIndexName);
            // The failed context is disposed by PersistOnceAsync before this fresh winner lookup.
            return await FindWinnerOrUnknownAsync(ownerUserId, key, hash, exception, cancellationToken);
        }
        catch (OperationCanceledException) when (!state.ProviderCommandStarted)
        {
            throw;
        }
        catch (OperationCanceledException exception) when (state.SaveDispatched)
        {
            // Cancellation after SaveChangesAsync dispatch may leave commit state unknown.
            throw new IdempotencyUnknownOutcomeException(exception);
        }
    }

    private async Task<IdempotencyOperationResult> PersistOnceAsync(
        string ownerUserId, string key, string hash, decimal amount, string note, SaveDispatchState state, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
        var existing = await db.ExpenseIdempotencyReceipts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.NormalizedKey == key, cancellationToken);
        if (existing is not null) return Replay(existing, hash);

        var expense = new Expense { OwnerUserId = ownerUserId, Amount = amount, Note = note };
        var receipt = new ExpenseIdempotencyReceipt
        {
            OwnerUserId = ownerUserId,
            NormalizedKey = key,
            RequestHash = hash,
            OriginalAmount = amount,
            OriginalNote = note
        };
        db.Expenses.Add(expense);
        var temporaryExpenseId = db.Entry(expense).Property(x => x.Id).CurrentValue;
        receipt.ExpenseId = temporaryExpenseId;
        db.ExpenseIdempotencyReceipts.Add(receipt);
        cancellationToken.ThrowIfCancellationRequested();
        state.SaveDispatched = true;
        var observer = scope.ServiceProvider.GetRequiredService<IdempotencyCommandObserver>();
        observer.Attach(db, state);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            observer.Detach(db);
        }
        return new IdempotencyOperationResult(expense.Id, amount, note, false);
    }

    private async Task<IdempotencyOperationResult> FindWinnerOrUnknownAsync(
        string ownerUserId, string key, string hash, Exception duplicate, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
        var winner = await db.ExpenseIdempotencyReceipts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.NormalizedKey == key, cancellationToken);
        if (winner is null) throw new IdempotencyUnknownOutcomeException(duplicate);
        return Replay(winner, hash);
    }

    private static IdempotencyOperationResult Replay(ExpenseIdempotencyReceipt receipt, string hash)
    {
        if (!string.Equals(receipt.RequestHash, hash, StringComparison.Ordinal)) throw new IdempotencyConflictException();
        return new IdempotencyOperationResult(receipt.ExpenseId, receipt.OriginalAmount, receipt.OriginalNote, true);
    }

    internal static bool IsIntendedReceiptDuplicate(DbUpdateException exception)
    {
        var sql = FindSqlException(exception);
        return sql is not null && IsIntendedReceiptDuplicate(sql.Number, sql.Message);
    }

    internal static bool IsIntendedReceiptDuplicate(int number, string message)
    {
        if (number is not (2601 or 2627)) return false;
        var match = Regex.Match(message, @"\bunique index '([^']+)'", RegexOptions.CultureInvariant);
        return match.Success && string.Equals(match.Groups[1].Value, ReceiptIndexName, StringComparison.Ordinal);
    }

    private static SqlException? FindSqlException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is SqlException sql) return sql;
        return null;
    }

    internal sealed class SaveDispatchState
    {
        public bool SaveDispatched { get; set; }
        public bool ProviderCommandStarted { get; set; }
    }

    public static string NormalizeIdempotencyKey(IReadOnlyCollection<string?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != 1 || values.FirstOrDefault() is not { } value || value.Length is < 1 or > MaxHeaderLength)
            throw new ArgumentException("The idempotency key header is invalid.", nameof(values));
        foreach (var character in value)
            if (!IsHttpTokenCharacter(character)) throw new ArgumentException("The idempotency key header is invalid.", nameof(values));
        return value.ToUpperInvariant();
    }

    public static string ComputeRequestHash(decimal amount, string note)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (amount <= 0 || amount > MaxAmount || decimal.Round(amount, 2) != amount) throw new ArgumentOutOfRangeException(nameof(amount));
        if (note.Length > MaxNoteLength) throw new ArgumentOutOfRangeException(nameof(note));
        for (var index = 0; index < note.Length; index++)
        {
            if (char.IsHighSurrogate(note[index]))
            {
                if (index + 1 >= note.Length || !char.IsLowSurrogate(note[index + 1])) throw new ArgumentException("The note must contain valid Unicode.", nameof(note));
                index++;
            }
            else if (char.IsLowSurrogate(note[index])) throw new ArgumentException("The note must contain valid Unicode.", nameof(note));
        }
        var payload = JsonSerializer.Serialize(new[] { "v1", amount.ToString("0.00", CultureInfo.InvariantCulture), note }, JsonOptions);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private static bool IsHttpTokenCharacter(char character) => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~';
}

public sealed class IdempotencyCommandObserver : DbCommandInterceptor
{
    private readonly ConcurrentDictionary<DbContext, ExpenseIdempotencyService.SaveDispatchState> states = new();

    internal void Attach(DbContext context, ExpenseIdempotencyService.SaveDispatchState state) => states[context] = state;
    internal void Detach(DbContext context) => states.TryRemove(context, out _);

    private void Mark(DbCommand command, CommandEventData eventData)
    {
        if (eventData.Context is not null && states.TryGetValue(eventData.Context, out var state))
            state.ProviderCommandStarted = true;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Mark(command, eventData);
        return result;
    }
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Mark(command, eventData);
        return result;
    }
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Mark(command, eventData);
        return result;
    }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Mark(command, eventData);
        return ValueTask.FromResult(result);
    }
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Mark(command, eventData);
        return ValueTask.FromResult(result);
    }
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        Mark(command, eventData);
        return ValueTask.FromResult(result);
    }
}
