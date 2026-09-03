using FormBuilder.Domain.FormTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormBuilder.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id).ValueGeneratedNever();

        builder.Property(step => step.Order).IsRequired();

        builder.Property(step => step.Name)
            .IsRequired()
            .HasMaxLength(ApprovalStep.MaxNameLength);

        builder.Property(step => step.ApproverId)
            .HasMaxLength(ApprovalStep.MaxApproverIdLength);

        builder.Property(step => step.ActionType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        // No two steps on the same template may share a position.
        builder.HasIndex(step => new { step.FormTemplateId, step.Order }).IsUnique();
    }
}
