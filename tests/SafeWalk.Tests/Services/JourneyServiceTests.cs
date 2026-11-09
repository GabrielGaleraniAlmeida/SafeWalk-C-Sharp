using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Application.Services;
using SafeWalk.Tests.TestDoubles;

namespace SafeWalk.Tests.Services;

public sealed class JourneyServiceTests
{
    [Fact]
    public async Task CreateAndCompleteAsync_ShareLocationOnlyWhileJourneyIsActive()
    {
        var clock = new FakeClock(DateTime.UtcNow);
        var service = new JourneyService(new FakeJourneyRepository(), clock);
        var created = await service.CreateAsync(new CreateJourneyRequest
        {
            Latitude = -23.55,
            Longitude = -46.63
        }, CancellationToken.None);
        var shareToken = created.ShareUrl[(created.ShareUrl.LastIndexOf('/') + 1)..];

        var visible = await service.GetSharedLocationAsync(shareToken, CancellationToken.None);
        Assert.NotNull(visible);
        Assert.Equal(-23.55, visible.Latitude);

        var updated = await service.UpdateLocationAsync(created.Id, new UpdateJourneyLocationRequest
        {
            TrackerToken = created.TrackerToken,
            Latitude = -23.54,
            Longitude = -46.62
        }, CancellationToken.None);
        Assert.True(updated);

        var completed = await service.CompleteAsync(created.Id, created.TrackerToken, CancellationToken.None);
        var hidden = await service.GetSharedLocationAsync(shareToken, CancellationToken.None);

        Assert.True(completed);
        Assert.Null(hidden);
    }

    [Fact]
    public async Task UpdateLocationAsync_RejectsInvalidTrackerToken()
    {
        var service = new JourneyService(new FakeJourneyRepository(), new FakeClock(DateTime.UtcNow));
        var created = await service.CreateAsync(new CreateJourneyRequest
        {
            Latitude = -23.55,
            Longitude = -46.63
        }, CancellationToken.None);

        var updated = await service.UpdateLocationAsync(created.Id, new UpdateJourneyLocationRequest
        {
            TrackerToken = "wrong-token",
            Latitude = -23.54,
            Longitude = -46.62
        }, CancellationToken.None);

        Assert.False(updated);
    }
}
