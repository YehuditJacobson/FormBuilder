using FormBuilder.Application.FormTemplates.Contracts;

namespace FormBuilder.Application.FormTemplates.Abstractions;

/// <summary>
/// Read-side access: returns DTOs projected directly from the database, bypassing the aggregate.
/// Keeps queries efficient (no tracking, no full graph load) and separate from the write model.
/// </summary>
public interface IFormTemplateQueries
{
    Task<IReadOnlyList<FormTemplateSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken);

    Task<FormTemplateDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
}
