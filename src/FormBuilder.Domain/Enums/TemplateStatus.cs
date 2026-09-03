namespace FormBuilder.Domain.Enums;

/// <summary>Lifecycle state of a <see cref="FormTemplates.FormTemplate"/>.</summary>
public enum TemplateStatus
{
    /// <summary>Still being edited; may be incomplete.</summary>
    Draft = 0,

    /// <summary>Finalised and available for use; has at least one field and one approval step.</summary>
    Published = 1,
}
