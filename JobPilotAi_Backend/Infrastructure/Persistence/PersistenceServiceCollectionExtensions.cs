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

        connectionString = connectionString.Trim().Trim('"', '\'');

        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var prefixIndex = connectionString.IndexOf("://", StringComparison.Ordinal);
                var uriBody = connectionString.Substring(prefixIndex + 3);

                string? userInfo = null;
                string hostAndPath = uriBody;

                var atIndex = uriBody.LastIndexOf('@');
                if (atIndex >= 0)
                {
                    userInfo = uriBody.Substring(0, atIndex);
                    hostAndPath = uriBody.Substring(atIndex + 1);
                }

                var slashIndex = hostAndPath.IndexOf('/');
                string hostAndPort = slashIndex >= 0 ? hostAndPath.Substring(0, slashIndex) : hostAndPath;
                string pathAndQuery = slashIndex >= 0 ? hostAndPath.Substring(slashIndex + 1) : string.Empty;

                var questionIndex = pathAndQuery.IndexOf('?');
                string database = questionIndex >= 0 ? pathAndQuery.Substring(0, questionIndex) : pathAndQuery;

                string host = hostAndPort;
                int port = 5432;
                var colonIndex = hostAndPort.IndexOf(':');
                if (colonIndex >= 0)
                {
                    host = hostAndPort.Substring(0, colonIndex);
                    if (int.TryParse(hostAndPort.Substring(colonIndex + 1), out var parsedPort))
                    {
                        port = parsedPort;
                    }
                }

                string username = string.Empty;
                string password = string.Empty;
                if (!string.IsNullOrEmpty(userInfo))
                {
                    var userColon = userInfo.IndexOf(':');
                    if (userColon >= 0)
                    {
                        username = Uri.UnescapeDataString(userInfo.Substring(0, userColon));
                        password = Uri.UnescapeDataString(userInfo.Substring(userColon + 1));
                    }
                    else
                    {
                        username = Uri.UnescapeDataString(userInfo);
                    }
                }

                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Port = port,
                    Database = database,
                    Username = username,
                    Password = password
                };

                if (!string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    builder.SslMode = SslMode.Require;
                }

                return builder.ConnectionString;
            }
            catch
            {
                return connectionString;
            }
        }

        return connectionString;
    }
}
