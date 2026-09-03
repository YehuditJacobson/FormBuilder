namespace FormBuilder.Application.Common;

/// <summary>Identifies the user on whose behalf the current request is running.</summary>
public interface ICurrentUser
{
    /// <summary>A stable identifier for the user (claim, header, or a system default for the PoC).</summary>
    string Id { get; }
}
