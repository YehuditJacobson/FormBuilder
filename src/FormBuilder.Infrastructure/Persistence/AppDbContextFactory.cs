using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FormBuilder.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> build an <see cref="AppDbContext"/> at design time (for
/// <c>migrations add</c>) without booting the API host. It targets SQL Server so the checked-in
/// migration describes the canonical relational schema; at runtime the provider is chosen in
/// <see cref="DependencyInjection.AddInfrastructure"/> and SQLite / InMemory use
/// <c>EnsureCreated()</c> instead of this migration.
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FormBuilder;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }
}
