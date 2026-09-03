using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Application.FormTemplates;

/// <summary>
/// Assembles a <see cref="FormTemplate"/> aggregate from a create request. The single place that
/// knows how a request maps onto the aggregate's construction methods; any <see cref="Domain.Common.DomainException"/>
/// it raises is a last line of defence behind the request validator.
/// </summary>
public static class FormTemplateFactory
{
    public static FormTemplate Create(CreateFormTemplateRequest request, string createdBy, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = new FormTemplate(request.Name, request.Description, createdBy, createdAtUtc);

        foreach (var field in request.Fields)
        {
            template.AddField(field.Label, field.FieldType, field.IsRequired, field.Placeholder, field.Options);
        }

        foreach (var step in request.ApprovalSteps)
        {
            template.AddApprovalStep(step.Name, step.ActionType, step.ApproverId);
        }

        return template;
    }
}
