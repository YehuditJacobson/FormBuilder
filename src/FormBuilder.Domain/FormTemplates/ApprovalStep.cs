using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Domain.FormTemplates;

/// <summary>
/// One milestone in a template's approval route: its position, a name, the identity of the
/// approver, and the action that approver is permitted to take. Like <see cref="FormField"/>
/// it is created and ordered through the <see cref="FormTemplate"/> aggregate root.
/// </summary>
public sealed class ApprovalStep : Entity
{
    public const int MaxNameLength = 200;
    public const int MaxApproverIdLength = 200;

    // Required by EF Core for materialisation.
    private ApprovalStep()
    {
    }

    internal ApprovalStep(string name, ApprovalActionType actionType, int order, string? approverId)
    {
        Name = DomainGuard.RequiredText(name, MaxNameLength, nameof(Name));
        ActionType = actionType;
        Order = order;
        ApproverId = DomainGuard.OptionalText(approverId, MaxApproverIdLength, nameof(ApproverId));
    }

    /// <summary>Foreign key to the owning template.</summary>
    public Guid FormTemplateId { get; private set; }

    /// <summary>Zero-based position of the step within the route. Kept contiguous by the aggregate.</summary>
    public int Order { get; private set; }

    /// <summary>Human-readable name of the step, for example "Direct manager approval".</summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Identity of the approver for this step. Free text for the PoC (a name or an external
    /// reference); ready to become a foreign key to a dedicated approver entity later.
    /// </summary>
    public string? ApproverId { get; private set; }

    /// <summary>The action this approver may take.</summary>
    public ApprovalActionType ActionType { get; private set; }

    internal void SetOrder(int order)
    {
        Order = order;
    }
}
