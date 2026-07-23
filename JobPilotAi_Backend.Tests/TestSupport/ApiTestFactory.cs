using JobPilotAi_Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobPilotAi_Backend.Tests.TestSupport;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DatabaseConnectionOptions>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            RemoveNpgsqlProviderServices(services);

            services.AddSingleton(new DatabaseConnectionOptions("DataSource=:memory:"));
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            services.AddHostedService<TestDatabaseInitializer>();
        });
    }

    private static void RemoveNpgsqlProviderServices(IServiceCollection services)
    {
        var npgsqlServices = services
            .Where(IsNpgsqlServiceDescriptor)
            .ToArray();

        foreach (var service in npgsqlServices)
        {
            services.Remove(service);
        }
    }

    private static bool IsNpgsqlServiceDescriptor(ServiceDescriptor service) =>
        IsNpgsqlService(service.ServiceType)
        || IsNpgsqlService(service.ImplementationType)
        || IsNpgsqlService(service.ImplementationInstance?.GetType());

    private static bool IsNpgsqlService(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        if (ContainsNpgsql(type.Assembly.GetName().Name) || ContainsNpgsql(type.FullName))
        {
            return true;
        }

        return type.IsGenericType
            && type.GetGenericArguments().Any(IsNpgsqlService);
    }

    private static bool ContainsNpgsql(string? value) =>
        value?.Contains("Npgsql", StringComparison.Ordinal) is true;

    private sealed class TestDatabaseInitializer(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
