using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Application.FormTemplates.Contracts;
using MediatR;

namespace FormBuilder.Application.FormTemplates.GetById;

/// <summary>Returns a single form template with its full field and approval-step detail.</summary>
public sealed record GetFormTemplateByIdQuery(Guid Id) : IRequest<Result<FormTemplateDetailDto>>;

public sealed class GetFormTemplateByIdQueryHandler(IFormTemplateQueries queries)
    : IRequestHandler<GetFormTemplateByIdQuery, Result<FormTemplateDetailDto>>
{
    public async Task<Result<FormTemplateDetailDto>> Handle(
        GetFormTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var detail = await queries.GetDetailAsync(query.Id, cancellationToken);

        return detail is null
            ? Result.Failure<FormTemplateDetailDto>(Error.NotFound($"Form template '{query.Id}' was not found."))
            : Result.Success(detail);
    }
}
