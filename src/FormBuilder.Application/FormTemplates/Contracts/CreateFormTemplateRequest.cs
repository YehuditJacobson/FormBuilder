using FormBuilder.Domain.Enums;

namespace FormBuilder.Application.FormTemplates.Contracts;

/// <summary>Payload for creating a form template in one call: the envelope, its fields, and its approval route.</summary>
public sealed record CreateFormTemplateRequest(
    string Name,
    string? Description,
    IReadOnlyList<CreateFormFieldInput> Fields,
    IReadOnlyList<CreateApprovalStepInput> ApprovalSteps);

/// <summary>One field to add to the form. Position is taken from the list order, not sent by the client.</summary>
public sealed record CreateFormFieldInput(
    string Label,
    FieldType FieldType,
    bool IsRequired,
    string? Placeholder,
    string? Options);

/// <summary>One approval step to add. Position is taken from the list order, not sent by the client.</summary>
public sealed record CreateApprovalStepInput(
    string Name,
    ApprovalActionType ActionType,
    string? ApproverId);
