using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Infrastructure.Persistence.Repositories;

internal sealed class FormTemplateRepository(AppDbContext dbContext) : IFormTemplateRepository
{
    public async Task AddAsync(FormTemplate template, CancellationToken cancellationToken)
    {
        await dbContext.FormTemplates.AddAsync(template, cancellationToken);
    }
}
