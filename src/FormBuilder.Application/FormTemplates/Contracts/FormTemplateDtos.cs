using FormBuilder.Domain.Enums;

namespace FormBuilder.Application.FormTemplates.Contracts;

/// <summary>Row shape for the "list templates" screen: the envelope plus child counts, no field/step detail.</summary>
public sealed record FormTemplateSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    string CreatedBy,
    TemplateStatus Status,
    int FieldCount,
    int ApprovalStepCount);

/// <summary>Full template graph returned by "get template by id".</summary>
public sealed record FormTemplateDetailDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    string CreatedBy,
    TemplateStatus Status,
    IReadOnlyList<FormFieldDto> Fields,
    IReadOnlyList<ApprovalStepDto> ApprovalSteps);

public sealed record FormFieldDto(
    Guid Id,
    string Label,
    FieldType FieldType,
    int Order,
    bool IsRequired,
    string? Placeholder,
    string? Options);

public sealed record ApprovalStepDto(
    Guid Id,
    int Order,
    string Name,
    string? ApproverId,
    ApprovalActionType ActionType);
