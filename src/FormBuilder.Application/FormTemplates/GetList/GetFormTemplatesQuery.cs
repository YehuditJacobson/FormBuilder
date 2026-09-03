using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Application.FormTemplates.Contracts;
using MediatR;

namespace FormBuilder.Application.FormTemplates.GetList;

/// <summary>Returns every form template as a summary row, newest first.</summary>
public sealed record GetFormTemplatesQuery : IRequest<IReadOnlyList<FormTemplateSummaryDto>>;

public sealed class GetFormTemplatesQueryHandler(IFormTemplateQueries queries)
    : IRequestHandler<GetFormTemplatesQuery, IReadOnlyList<FormTemplateSummaryDto>>
{
    public Task<IReadOnlyList<FormTemplateSummaryDto>> Handle(
        GetFormTemplatesQuery query,
        CancellationToken cancellationToken)
        => queries.GetSummariesAsync(cancellationToken);
}
