using Microsoft.EntityFrameworkCore;

public partial class AuthDb
{
    public DbSet<ExpenseIdempotencyReceipt> ExpenseIdempotencyReceipts => Set<ExpenseIdempotencyReceipt>();

    partial void ConfigureReceiptModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseIdempotencyReceipt>(entity =>
        {
            entity.ToTable("ExpenseIdempotencyReceipts");
            entity.HasKey(receipt => receipt.Id).IsClustered();

            entity.Property(receipt => receipt.OwnerUserId)
                .HasMaxLength(450)
                .IsRequired();
            entity.Property(receipt => receipt.NormalizedKey)
                .HasMaxLength(128)
                .IsUnicode(false)
                .UseCollation("Latin1_General_100_BIN2")
                .IsRequired();
            entity.Property(receipt => receipt.RequestHash)
                .HasColumnType("char(64)")
                .IsUnicode(false)
                .IsFixedLength()
                .IsRequired();
            entity.Property(receipt => receipt.ExpenseId)
                .HasColumnType("int")
                .IsRequired();
            entity.Property(receipt => receipt.OriginalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(receipt => receipt.OriginalNote)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne<Expense>()
                .WithMany()
                .HasForeignKey(receipt => receipt.ExpenseId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(receipt => new { receipt.OwnerUserId, receipt.NormalizedKey })
                .HasDatabaseName("UX_ExpenseIdempotencyReceipts_Owner_Key")
                .IsUnique()
                .IsClustered(false);
        });
    }
}
