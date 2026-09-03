using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Application.Tests.Fakes;

/// <summary>Captures the aggregate handed to <see cref="AddAsync"/> so tests can assert on how it was built.</summary>
internal sealed class FakeFormTemplateRepository : IFormTemplateRepository
{
    public FormTemplate? Added { get; private set; }

    public Task AddAsync(FormTemplate template, CancellationToken cancellationToken)
    {
        Added = template;
        return Task.CompletedTask;
    }
}
