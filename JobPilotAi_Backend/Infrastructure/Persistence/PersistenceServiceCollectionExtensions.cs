using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    private const string DefaultConnectionName = "Default";

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName);
        var databaseOptions = new DatabaseConnectionOptions(connectionString);

        services.AddSingleton(databaseOptions);
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(databaseOptions.ConnectionString ?? string.Empty);
        });

        return services;
    }
}
