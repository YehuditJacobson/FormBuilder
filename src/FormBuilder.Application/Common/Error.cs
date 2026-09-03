namespace FormBuilder.Application.Common;

/// <summary>The type of failure a <see cref="Result"/> carries, so callers can branch without string matching.</summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
}

/// <summary>A single failure: a stable <see cref="Code"/>, a human-readable <see cref="Message"/>, and a <see cref="Type"/>.</summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Validation(string message) => new("validation", message, ErrorType.Validation);

    public static Error NotFound(string message) => new("not_found", message, ErrorType.NotFound);

    public static Error Conflict(string message) => new("conflict", message, ErrorType.Conflict);
}
