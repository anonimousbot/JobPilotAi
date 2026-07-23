using JobPilotAi_Backend.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Tests.TestSupport;

public sealed class DatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
