using SafeWalk.Api.Application.Abstractions;

namespace SafeWalk.Tests.TestDoubles;

internal sealed class FakeClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}
