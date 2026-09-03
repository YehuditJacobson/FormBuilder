using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Domain.FormTemplates;

/// <summary>
/// A single dynamic field on a <see cref="FormTemplate"/> (for example a text box or a date
/// picker). A field only exists as part of a template; it is created and ordered through the
/// <see cref="FormTemplate"/> aggregate root, never on its own.
/// </summary>
public sealed class FormField : Entity
{
    public const int MaxLabelLength = 200;
    public const int MaxPlaceholderLength = 200;
    public const int MaxOptionsLength = 2000;

    // Required by EF Core for materialisation.
    private FormField()
    {
    }

    internal FormField(string label, FieldType fieldType, int order, bool isRequired, string? placeholder, string? options)
    {
        Label = DomainGuard.RequiredText(label, MaxLabelLength, nameof(Label));
        FieldType = fieldType;
        Order = order;
        IsRequired = isRequired;
        Placeholder = DomainGuard.OptionalText(placeholder, MaxPlaceholderLength, nameof(Placeholder));
        Options = DomainGuard.OptionalText(options, MaxOptionsLength, nameof(Options));
    }

    /// <summary>Foreign key to the owning template.</summary>
    public Guid FormTemplateId { get; private set; }

    /// <summary>Text shown next to the input on the rendered form.</summary>
    public string Label { get; private set; } = null!;

    /// <summary>The kind of input to render.</summary>
    public FieldType FieldType { get; private set; }

    /// <summary>Zero-based position of the field within the template. Kept contiguous by the aggregate.</summary>
    public int Order { get; private set; }

    /// <summary>Whether the end user must fill this field in.</summary>
    public bool IsRequired { get; private set; }

    /// <summary>Optional placeholder / hint text.</summary>
    public string? Placeholder { get; private set; }

    /// <summary>Optional JSON payload of choices, used when <see cref="FieldType"/> is <see cref="FieldType.Dropdown"/>.</summary>
    public string? Options { get; private set; }

    internal void SetOrder(int order)
    {
        Order = order;
    }
}
