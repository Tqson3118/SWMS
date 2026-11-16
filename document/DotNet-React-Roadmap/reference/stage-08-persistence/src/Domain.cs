using Microsoft.EntityFrameworkCore;

namespace ExpenseImportPersistence;

public sealed class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public sealed class ImportedExpense
{
    public long Id { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly SpentOn { get; set; }
    public string Note { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
}

public sealed class ImportDbContext(DbContextOptions<ImportDbContext> options) : DbContext(options)
{
    public DbSet<ExpenseCategory> Categories => Set<ExpenseCategory>();
    public DbSet<ImportedExpense> Expenses => Set<ImportedExpense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasData(new ExpenseCategory { Id = 1, Name = "Food" }, new ExpenseCategory { Id = 2, Name = "Travel" }, new ExpenseCategory { Id = 7, Name = "Learning" });
        });
        modelBuilder.Entity<ImportedExpense>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.SpentOn).HasColumnType("date").IsRequired();
            entity.Property(x => x.Note).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
