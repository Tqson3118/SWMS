using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class AuthDesignTimeFactory : IDesignTimeDbContextFactory<AuthDb>
{
    public AuthDb CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDb>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=RoadmapAuth11_0123456789abcdef0123456789abcdef;Integrated Security=True;TrustServerCertificate=True;")
            .Options;
        return new AuthDb(options);
    }
}
