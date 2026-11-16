using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseImportPersistence;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<ImportDbContext>
{
    public ImportDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ImportDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DesignTimeOnly;Integrated Security=True;TrustServerCertificate=True;")
            .Options;
        return new ImportDbContext(options);
    }
}
