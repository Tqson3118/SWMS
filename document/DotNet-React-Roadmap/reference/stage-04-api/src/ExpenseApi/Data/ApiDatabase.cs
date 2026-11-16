using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ExpenseApi.Data;

public static class ApiDatabase
{
    private static readonly Regex NamePattern = new(@"\ARoadmapApi779_[0-9a-f]{32}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string NewName() => $"RoadmapApi779_{Guid.NewGuid():N}";

    public static void ValidateFreshName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!NamePattern.IsMatch(name))
            throw new ArgumentException("Only RoadmapApi779_<32 hex> database names are allowed.", nameof(name));
    }

    public static string ConnectionString(string name)
    {
        ValidateFreshName(name);
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = name,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ApplicationName = "RoadmapApi779"
        };
        return builder.ConnectionString;
    }

    public static async Task CreateFreshDatabaseAsync(string name, string? logPath, CancellationToken cancellationToken = default)
    {
        ValidateFreshName(name);
        Console.WriteLine($"RoadmapApi779 database before create: {name}");
        if (!string.IsNullOrWhiteSpace(logPath))
            await File.AppendAllTextAsync(logPath, $"{name}{Environment.NewLine}", cancellationToken);

        var master = new SqlConnectionStringBuilder(ConnectionString(name)) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(master.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT_BIG(*) FROM sys.databases WHERE name = @name;";
        exists.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = name;
        if ((long)(await exists.ExecuteScalarAsync(cancellationToken))! != 0)
            throw new InvalidOperationException($"Fresh database {name} already exists; refusing reuse.");

        await using var create = connection.CreateCommand();
        create.CommandText = $"CREATE DATABASE [{name}]";
        await create.ExecuteNonQueryAsync(cancellationToken);
    }
}
