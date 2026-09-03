using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Application.FormTemplates.Abstractions;

/// <summary>Write-side access to the <see cref="FormTemplate"/> aggregate.</summary>
public interface IFormTemplateRepository
{
    /// <summary>Stages a new template (with its fields and approval steps) for insertion.</summary>
    Task AddAsync(FormTemplate template, CancellationToken cancellationToken);
}
