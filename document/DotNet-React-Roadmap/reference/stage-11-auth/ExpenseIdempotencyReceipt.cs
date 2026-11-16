public sealed class ExpenseIdempotencyReceipt
{
    public int Id { get; set; }
    public string OwnerUserId { get; set; } = "";
    public string NormalizedKey { get; set; } = "";
    public string RequestHash { get; set; } = "";
    public int ExpenseId { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalNote { get; set; } = "";
}
