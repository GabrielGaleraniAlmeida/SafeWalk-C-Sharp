using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Application.Services;
using SafeWalk.Api.Domain.Enums;
using SafeWalk.Tests.TestDoubles;

namespace SafeWalk.Tests.Services;

public sealed class SafetyReportServiceTests
{
    [Fact]
    public async Task CreateAsync_SetsShortExpiryForPublicSafetyReports()
    {
        var now = new DateTime(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(now);
        var repository = new FakeSafetyReportRepository();
        var service = new SafetyReportService(repository, clock);

        var report = await service.CreateAsync(new CreateSafetyReportRequest
        {
            Category = ReportCategory.PublicSafety,
            Description = "  Relato de assalto  ",
            Latitude = -23.55,
            Longitude = -46.63
        }, CancellationToken.None);

        Assert.Equal("Relato de assalto", report.Description);
        Assert.Equal(now.AddHours(4), report.ExpiresAtUtc);
        Assert.Single(repository.Reports);
    }

    [Fact]
    public async Task ConfirmAsync_IncreasesConfirmationCount()
    {
        var clock = new FakeClock(DateTime.UtcNow);
        var repository = new FakeSafetyReportRepository();
        var service = new SafetyReportService(repository, clock);
        var report = await service.CreateAsync(new CreateSafetyReportRequest
        {
            Category = ReportCategory.Sidewalk,
            Description = "Calçada quebrada neste trecho.",
            Latitude = -23.55,
            Longitude = -46.63
        }, CancellationToken.None);

        var confirmed = await service.ConfirmAsync(report.Id, CancellationToken.None);

        Assert.NotNull(confirmed);
        Assert.Equal(1, confirmed.ConfirmationCount);
    }
}
