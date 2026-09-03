using FormBuilder.Domain.FormTemplates;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Persistence;

/// <summary>The EF Core unit of work for the whole model. Entity mappings live in <c>Persistence/Configurations</c>.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();

    public DbSet<FormField> FormFields => Set<FormField>();

    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
