namespace FormBuilder.Domain.Common;

/// <summary>
/// Raised when an operation would violate a domain rule (an invariant of an aggregate).
/// The application layer translates this into a client-facing validation error.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
