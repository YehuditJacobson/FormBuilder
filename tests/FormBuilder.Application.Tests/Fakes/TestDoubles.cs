using FormBuilder.Application.Common;

namespace FormBuilder.Application.Tests.Fakes;

internal sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}

internal sealed class StubCurrentUser(string id) : ICurrentUser
{
    public string Id { get; } = id;
}
