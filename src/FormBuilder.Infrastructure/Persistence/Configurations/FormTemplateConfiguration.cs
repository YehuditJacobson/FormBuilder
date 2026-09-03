using FormBuilder.Domain.FormTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormBuilder.Infrastructure.Persistence.Configurations;

internal sealed class FormTemplateConfiguration : IEntityTypeConfiguration<FormTemplate>
{
    public void Configure(EntityTypeBuilder<FormTemplate> builder)
    {
        builder.ToTable("FormTemplates");

        builder.HasKey(template => template.Id);
        builder.Property(template => template.Id).ValueGeneratedNever();

        builder.Property(template => template.Name)
            .IsRequired()
            .HasMaxLength(FormTemplate.MaxNameLength);

        builder.Property(template => template.Description)
            .HasMaxLength(FormTemplate.MaxDescriptionLength);

        builder.Property(template => template.CreatedBy)
            .IsRequired()
            .HasMaxLength(FormTemplate.MaxCreatedByLength);

        builder.Property(template => template.CreatedAtUtc)
            .IsRequired();

        builder.Property(template => template.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasMany(template => template.Fields)
            .WithOne()
            .HasForeignKey(field => field.FormTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(template => template.ApprovalSteps)
            .WithOne()
            .HasForeignKey(step => step.FormTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate exposes read-only collections backed by private lists; EF writes the fields directly.
        builder.Metadata
            .FindNavigation(nameof(FormTemplate.Fields))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(FormTemplate.ApprovalSteps))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
