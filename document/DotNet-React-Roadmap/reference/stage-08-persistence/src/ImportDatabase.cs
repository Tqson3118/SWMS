using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ExpenseImportPersistence;

public static class ImportDatabase
{
    private static readonly Regex NamePattern = new(@"\ARoadmapImport915_[0-9a-f]{32}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string NewName() => $"RoadmapImport915_{Guid.NewGuid():N}";

    public static string ConnectionString(string name)
    {
        if (!NamePattern.IsMatch(name)) throw new ArgumentException("Only RoadmapImport915_<32 lowercase hex> is allowed.", nameof(name));
        var builder = new SqlConnectionStringBuilder { DataSource = @"(localdb)\MSSQLLocalDB", InitialCatalog = name, IntegratedSecurity = true, TrustServerCertificate = true, ApplicationName = "RoadmapImport915" };
        return builder.ConnectionString;
    }

    public static async Task CreateFreshAsync(string name, string logPath, CancellationToken cancellationToken = default)
    {
        if (!NamePattern.IsMatch(name)) throw new ArgumentException("Only RoadmapImport915_<32 lowercase hex> is allowed.", nameof(name));
        Console.WriteLine($"RoadmapImport915 database before create: {name}");
        await File.AppendAllTextAsync(logPath, name + Environment.NewLine, cancellationToken);
        var master = new SqlConnectionStringBuilder(ConnectionString(name)) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(master.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT_BIG(*) FROM sys.databases WHERE name = @name;";
        exists.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = name;
        if ((long)(await exists.ExecuteScalarAsync(cancellationToken))! != 0) throw new InvalidOperationException("Fresh database already exists; refusing reuse.");
        await using var create = connection.CreateCommand();
        create.CommandText = $"CREATE DATABASE [{name}]";
        await create.ExecuteNonQueryAsync(cancellationToken);
    }
}
