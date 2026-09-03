namespace FormBuilder.Domain.Enums;

/// <summary>The action an approver is allowed to take at a single step of an approval route.</summary>
public enum ApprovalActionType
{
    Approve = 0,
    Reject = 1,
    ReturnForRevision = 2,
    Sign = 3,
    Acknowledge = 4,
}
