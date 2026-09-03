using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Infrastructure.Identity;
using FormBuilder.Infrastructure.Persistence;
using FormBuilder.Infrastructure.Persistence.Queries;
using FormBuilder.Infrastructure.Persistence.Repositories;
using FormBuilder.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FormBuilder.Infrastructure;

/// <summary>
/// Registers the infrastructure layer: the <see cref="AppDbContext"/> with a configurable provider,
/// the repository / query / unit-of-work implementations, and the system service defaults.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString
                        ?? throw new InvalidOperationException("ConnectionStrings:Default is required for the SqlServer provider."));
                    break;

                case "inmemory":
                    options.UseInMemoryDatabase("FormBuilder");
                    break;

                case "sqlite":
                    options.UseSqlite(connectionString ?? "Data Source=formbuilder.db");
                    break;

                default:
                    throw new InvalidOperationException($"Unknown Database:Provider '{provider}'. Use SqlServer, Sqlite, or InMemory.");
            }
        });

        services.AddScoped<IFormTemplateRepository, FormTemplateRepository>();
        services.AddScoped<IFormTemplateQueries, FormTemplateQueries>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICurrentUser, SystemCurrentUser>();

        return services;
    }
}
