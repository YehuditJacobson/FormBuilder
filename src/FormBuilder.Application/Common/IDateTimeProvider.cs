namespace FormBuilder.Application.Common;

/// <summary>Supplies the current time, so handlers and the domain stay deterministic and testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
