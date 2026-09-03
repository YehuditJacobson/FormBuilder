using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Domain.FormTemplates;

/// <summary>
/// Aggregate root for an organizational form: its envelope (name, description, author,
/// creation time, lifecycle status), the dynamic fields it is built from, and the ordered
/// approval route attached to it.
/// </summary>
/// <remarks>
/// The two child collections are only ever mutated through the methods on this class, which
/// keep every <c>Order</c> contiguous from zero. Nothing outside the aggregate holds a
/// reference to a <see cref="FormField"/> or an <see cref="ApprovalStep"/>.
/// </remarks>
public sealed class FormTemplate : Entity
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;
    public const int MaxCreatedByLength = 256;

    private readonly List<FormField> _fields = [];
    private readonly List<ApprovalStep> _approvalSteps = [];

    // Required by EF Core for materialisation.
    private FormTemplate()
    {
    }

    /// <summary>Creates a new draft template.</summary>
    /// <param name="name">Display name of the form. Required, up to <see cref="MaxNameLength"/> characters.</param>
    /// <param name="description">Optional free-text description.</param>
    /// <param name="createdBy">Identifier of the user creating the form. Required.</param>
    /// <param name="createdAtUtc">Creation timestamp, supplied by the caller so the domain stays deterministic.</param>
    public FormTemplate(string name, string? description, string createdBy, DateTime createdAtUtc)
    {
        Name = DomainGuard.RequiredText(name, MaxNameLength, nameof(Name));
        Description = DomainGuard.OptionalText(description, MaxDescriptionLength, nameof(Description));
        CreatedBy = DomainGuard.RequiredText(createdBy, MaxCreatedByLength, nameof(CreatedBy));
        CreatedAtUtc = createdAtUtc;
        Status = TemplateStatus.Draft;
    }

    /// <summary>Display name of the form.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Optional free-text description.</summary>
    public string? Description { get; private set; }

    /// <summary>When the template was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Identifier of the user who created the template.</summary>
    public string CreatedBy { get; private set; } = null!;

    /// <summary>Lifecycle state. New templates start as <see cref="TemplateStatus.Draft"/>.</summary>
    public TemplateStatus Status { get; private set; }

    /// <summary>The dynamic fields, in display order.</summary>
    public IReadOnlyList<FormField> Fields => _fields.AsReadOnly();

    /// <summary>The approval route, in step order.</summary>
    public IReadOnlyList<ApprovalStep> ApprovalSteps => _approvalSteps.AsReadOnly();

    /// <summary>Appends a field to the end of the form and returns it.</summary>
    public FormField AddField(string label, FieldType fieldType, bool isRequired = false, string? placeholder = null, string? options = null)
    {
        var field = new FormField(label, fieldType, _fields.Count, isRequired, placeholder, options);
        _fields.Add(field);
        return field;
    }

    /// <summary>Removes the field with the given id and closes the gap in the ordering.</summary>
    /// <exception cref="DomainException">The field is not part of this template.</exception>
    public void RemoveField(Guid fieldId)
    {
        var field = _fields.SingleOrDefault(f => f.Id == fieldId)
            ?? throw new DomainException($"Field '{fieldId}' is not part of this template.");

        _fields.Remove(field);
        Reindex(_fields, static (field, order) => field.SetOrder(order));
    }

    /// <summary>Reorders the fields to match the given sequence of ids.</summary>
    /// <exception cref="DomainException">The sequence does not list every field exactly once.</exception>
    public void ReorderFields(IReadOnlyList<Guid> orderedFieldIds)
    {
        ApplyOrder(_fields, orderedFieldIds, "field");
        Reindex(_fields, static (field, order) => field.SetOrder(order));
    }

    /// <summary>Appends a step to the end of the approval route and returns it.</summary>
    public ApprovalStep AddApprovalStep(string name, ApprovalActionType actionType, string? approverId = null)
    {
        var step = new ApprovalStep(name, actionType, _approvalSteps.Count, approverId);
        _approvalSteps.Add(step);
        return step;
    }

    /// <summary>Removes the step with the given id and closes the gap in the ordering.</summary>
    /// <exception cref="DomainException">The step is not part of this template.</exception>
    public void RemoveApprovalStep(Guid stepId)
    {
        var step = _approvalSteps.SingleOrDefault(s => s.Id == stepId)
            ?? throw new DomainException($"Approval step '{stepId}' is not part of this template.");

        _approvalSteps.Remove(step);
        Reindex(_approvalSteps, static (step, order) => step.SetOrder(order));
    }

    /// <summary>Reorders the approval route to match the given sequence of ids.</summary>
    /// <exception cref="DomainException">The sequence does not list every step exactly once.</exception>
    public void ReorderApprovalSteps(IReadOnlyList<Guid> orderedStepIds)
    {
        ApplyOrder(_approvalSteps, orderedStepIds, "approval step");
        Reindex(_approvalSteps, static (step, order) => step.SetOrder(order));
    }

    /// <summary>
    /// Marks the template as published. A template can only be published once it has at least
    /// one field and at least one approval step.
    /// </summary>
    /// <exception cref="DomainException">The template has no fields or no approval steps.</exception>
    public void Publish()
    {
        if (_fields.Count == 0)
        {
            throw new DomainException("A template must have at least one field before it can be published.");
        }

        if (_approvalSteps.Count == 0)
        {
            throw new DomainException("A template must have at least one approval step before it can be published.");
        }

        Status = TemplateStatus.Published;
    }

    private static void Reindex<T>(List<T> items, Action<T, int> setOrder)
    {
        for (var i = 0; i < items.Count; i++)
        {
            setOrder(items[i], i);
        }
    }

    private static void ApplyOrder<T>(List<T> items, IReadOnlyList<Guid> orderedIds, string itemName)
        where T : Entity
    {
        ArgumentNullException.ThrowIfNull(orderedIds);

        var requested = orderedIds.ToHashSet();
        if (orderedIds.Count != items.Count || !requested.SetEquals(items.Select(i => i.Id)))
        {
            throw new DomainException($"The reorder request must list every {itemName} of this template exactly once.");
        }

        var positionById = new Dictionary<Guid, int>(orderedIds.Count);
        for (var i = 0; i < orderedIds.Count; i++)
        {
            positionById[orderedIds[i]] = i;
        }

        items.Sort((left, right) => positionById[left.Id].CompareTo(positionById[right.Id]));
    }
}
