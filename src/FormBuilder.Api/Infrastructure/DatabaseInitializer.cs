using FormBuilder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Api.Infrastructure;

/// <summary>
/// Brings the database up to date at startup: a SQL Server database is migrated; SQLite and
/// InMemory are created straight from the model (they do not use the checked-in migration).
/// </summary>
internal static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var providerName = context.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("SqlServer", StringComparison.Ordinal))
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}
