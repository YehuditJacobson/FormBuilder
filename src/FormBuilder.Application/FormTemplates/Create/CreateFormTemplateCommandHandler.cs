using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Domain.Common;
using MediatR;

namespace FormBuilder.Application.FormTemplates.Create;

public sealed class CreateFormTemplateCommandHandler(
    IFormTemplateRepository repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser)
    : IRequestHandler<CreateFormTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateFormTemplateCommand command, CancellationToken cancellationToken)
    {
        Domain.FormTemplates.FormTemplate template;
        try
        {
            template = FormTemplateFactory.Create(
                command.Request,
                currentUser.Id,
                dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            // The request validator catches these first; this keeps the aggregate authoritative.
            return Result.Failure<Guid>(Error.Validation(exception.Message));
        }

        await repository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
