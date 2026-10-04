using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JobPilotAi_Backend.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    private const string DefaultConnectionName = "Default";

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString(DefaultConnectionName);
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            rawConnectionString = configuration["DATABASE_URL"] ?? configuration["POSTGRES_URL"];
        }

        var connectionString = NormalizeConnectionString(rawConnectionString);
        var databaseOptions = new DatabaseConnectionOptions(connectionString);

        services.AddSingleton(databaseOptions);
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (databaseOptions.IsConfigured)
            {
                options.UseNpgsql(databaseOptions.ConnectionString);
            }
        });

        return services;
    }

    private static string? NormalizeConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;

        try
        {
            NpgsqlConnectionStringBuilder builder;

            if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Database = uri.AbsolutePath.TrimStart('/'),
                    Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
                    Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty
                };

                if (!string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    builder.SslMode = SslMode.Require;
                }
            }
            else
            {
                builder = new NpgsqlConnectionStringBuilder(connectionString);
            }

            builder["GssEncMode"] = "Disable";
            return builder.ConnectionString;
        }
        catch
        {
            return connectionString;
        }
    }
}
