namespace FormBuilder.Domain.Common;

/// <summary>
/// Small guard helpers used by aggregates to enforce their invariants. Each helper returns
/// the normalised value so it can be used inline in a constructor or setter.
/// </summary>
internal static class DomainGuard
{
    public static string AgainstNullOrWhiteSpace(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    public static string AgainstTooLong(string value, int maxLength, string fieldName)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be {maxLength} characters or fewer.");
        }

        return value;
    }

    public static string RequiredText(string? value, int maxLength, string fieldName)
    {
        return AgainstTooLong(AgainstNullOrWhiteSpace(value, fieldName), maxLength, fieldName);
    }

    public static string? OptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return AgainstTooLong(value.Trim(), maxLength, fieldName);
    }
}
