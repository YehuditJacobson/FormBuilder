using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Application.FormTemplates.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Persistence.Queries;

/// <summary>Read-side projections straight from the database: no tracking, no aggregate materialisation.</summary>
internal sealed class FormTemplateQueries(AppDbContext dbContext) : IFormTemplateQueries
{
    public async Task<IReadOnlyList<FormTemplateSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.FormTemplates
            .AsNoTracking()
            .OrderByDescending(template => template.CreatedAtUtc)
            .Select(template => new FormTemplateSummaryDto(
                template.Id,
                template.Name,
                template.Description,
                template.CreatedAtUtc,
                template.CreatedBy,
                template.Status,
                template.Fields.Count,
                template.ApprovalSteps.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<FormTemplateDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.FormTemplates
            .AsNoTracking()
            .Where(template => template.Id == id)
            .Select(template => new FormTemplateDetailDto(
                template.Id,
                template.Name,
                template.Description,
                template.CreatedAtUtc,
                template.CreatedBy,
                template.Status,
                template.Fields
                    .OrderBy(field => field.Order)
                    .Select(field => new FormFieldDto(
                        field.Id,
                        field.Label,
                        field.FieldType,
                        field.Order,
                        field.IsRequired,
                        field.Placeholder,
                        field.Options))
                    .ToList(),
                template.ApprovalSteps
                    .OrderBy(step => step.Order)
                    .Select(step => new ApprovalStepDto(
                        step.Id,
                        step.Order,
                        step.Name,
                        step.ApproverId,
                        step.ActionType))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
