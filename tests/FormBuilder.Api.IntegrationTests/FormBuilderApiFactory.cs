using System.Text.Json;
using System.Text.Json.Serialization;
using FormBuilder.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FormBuilder.Api.IntegrationTests;

/// <summary>
/// Hosts the real API in memory. Environment <c>Testing</c> turns off the dev seeder and
/// Swagger; the <see cref="AppDbContext"/> is swapped for a per-factory in-memory database so
/// test classes do not share state.
/// </summary>
public sealed class FormBuilderApiFactory : WebApplicationFactory<Program>
{
    /// <summary>JSON options matching the API (enums as strings, camelCase).</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // One database name per factory instance (built once, not per DbContext resolution).
    private readonly string _databaseName = $"formbuilder-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
