using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Contracts;
using MediatR;

namespace FormBuilder.Application.FormTemplates.Create;

/// <summary>Creates and persists a new form template together with all of its fields and approval steps.</summary>
public sealed record CreateFormTemplateCommand(CreateFormTemplateRequest Request) : IRequest<Result<Guid>>;
