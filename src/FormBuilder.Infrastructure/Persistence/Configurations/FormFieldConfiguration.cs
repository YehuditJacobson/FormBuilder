using FormBuilder.Domain.FormTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormBuilder.Infrastructure.Persistence.Configurations;

internal sealed class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("FormFields");

        builder.HasKey(field => field.Id);
        builder.Property(field => field.Id).ValueGeneratedNever();

        builder.Property(field => field.Label)
            .IsRequired()
            .HasMaxLength(FormField.MaxLabelLength);

        builder.Property(field => field.FieldType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(field => field.Order).IsRequired();
        builder.Property(field => field.IsRequired).IsRequired();

        builder.Property(field => field.Placeholder)
            .HasMaxLength(FormField.MaxPlaceholderLength);

        builder.Property(field => field.Options)
            .HasMaxLength(FormField.MaxOptionsLength);

        // No two fields on the same template may share a position.
        builder.HasIndex(field => new { field.FormTemplateId, field.Order }).IsUnique();
    }
}
