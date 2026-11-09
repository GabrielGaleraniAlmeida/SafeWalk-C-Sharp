using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Application.Services;
using SafeWalk.Api.Domain.Enums;
using SafeWalk.Tests.TestDoubles;

namespace SafeWalk.Tests.Services;

public sealed class SosAlertServiceTests
{
    [Fact]
    public async Task CancelAsync_RejectsWrongPinAndKeepsAlertActive()
    {
        var repository = new FakeSosAlertRepository();
        var service = new SosAlertService(repository, new FakeClock(DateTime.UtcNow));
        var alert = await service.CreateAsync(new CreateSosAlertRequest
        {
            Latitude = -23.55,
            Longitude = -46.63,
            Pin = "1234"
        }, CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CancelAsync(alert.Id, "9999", CancellationToken.None));

        var stillActive = await service.GetByIdAsync(alert.Id, CancellationToken.None);
        Assert.NotNull(stillActive);
        Assert.Equal(SosAlertStatus.ConfirmationPending, stillActive.Status);
    }

    [Fact]
    public async Task CancelAsync_WithCorrectPinCancelsTheAlert()
    {
        var repository = new FakeSosAlertRepository();
        var service = new SosAlertService(repository, new FakeClock(DateTime.UtcNow));
        var alert = await service.CreateAsync(new CreateSosAlertRequest
        {
            Latitude = -23.55,
            Longitude = -46.63,
            Pin = "4826"
        }, CancellationToken.None);

        var canceled = await service.CancelAsync(alert.Id, "4826", CancellationToken.None);

        Assert.NotNull(canceled);
        Assert.Equal(SosAlertStatus.Canceled, canceled.Status);
    }

    [Fact]
    public async Task AdvanceEscalationAsync_NotifiesContactsThenCityCenter()
    {
        var repository = new FakeSosAlertRepository();
        var service = new SosAlertService(repository, new FakeClock(DateTime.UtcNow));
        var alert = await service.CreateAsync(new CreateSosAlertRequest
        {
            Latitude = -23.55,
            Longitude = -46.63,
            Pin = "4826"
        }, CancellationToken.None);

        var contacts = await service.AdvanceEscalationAsync(alert.Id, CancellationToken.None);
        var cityCenter = await service.AdvanceEscalationAsync(alert.Id, CancellationToken.None);

        Assert.Equal(SosAlertStatus.ContactsNotified, contacts?.Status);
        Assert.Equal(SosAlertStatus.CityCenterNotified, cityCenter?.Status);
    }
}
