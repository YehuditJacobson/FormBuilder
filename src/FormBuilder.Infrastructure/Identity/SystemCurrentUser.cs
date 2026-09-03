using FormBuilder.Application.Common;

namespace FormBuilder.Infrastructure.Identity;

/// <summary>
/// Fallback identity for contexts with no HTTP request (design time, tests, background jobs).
/// The API layer registers an <see cref="ICurrentUser"/> that reads the real request instead.
/// </summary>
internal sealed class SystemCurrentUser : ICurrentUser
{
    public string Id => "system";
}
