using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Application.Services;
using SafeWalk.Api.Domain.Entities;
using SafeWalk.Api.Domain.Enums;
using SafeWalk.Tests.TestDoubles;

namespace SafeWalk.Tests.Services;

public sealed class RiskScoringServiceTests
{
    [Fact]
    public async Task CompareAsync_NearbyPublicSafetyReportRaisesRouteRisk()
    {
        var now = DateTime.UtcNow;
        var reports = new FakeSafetyReportRepository();
        await reports.AddAsync(
            SafetyReport.Create(ReportCategory.PublicSafety, "Roubo relatado.", -23.55, -46.63, now),
            CancellationToken.None);
        var service = new RiskScoringService(reports, new FakeClock(now));
        var request = new RouteComparisonRequest
        {
            Routes =
            [
                Route("Com relato", -23.55, -46.63),
                Route("Sem relato", -23.56, -46.65)
            ]
        };

        var result = await service.CompareAsync(request, CancellationToken.None);

        var reportedRoute = Assert.Single(result.Options, option => option.Name == "Com relato");
        var quieterRoute = Assert.Single(result.Options, option => option.Name == "Sem relato");
        Assert.True(reportedRoute.RiskScore > quieterRoute.RiskScore);
        Assert.Equal(1, reportedRoute.Segments[0].ActiveReports);
        Assert.True(quieterRoute.IsSafest);
    }

    [Fact]
    public async Task CompareAsync_CameraCoverageReducesTheSegmentScore()
    {
        var now = DateTime.UtcNow;
        var service = new RiskScoringService(new FakeSafetyReportRepository(), new FakeClock(now));
        var request = new RouteComparisonRequest
        {
            Routes =
            [
                Route("Sem câmera", -23.55, -46.63, camera: false),
                Route("Com câmera", -23.56, -46.65, camera: true)
            ]
        };

        var result = await service.CompareAsync(request, CancellationToken.None);

        var noCamera = Assert.Single(result.Options, option => option.Name == "Sem câmera");
        var withCamera = Assert.Single(result.Options, option => option.Name == "Com câmera");
        Assert.True(withCamera.RiskScore < noCamera.RiskScore);
    }

    private static RouteCandidateRequest Route(string name, double latitude, double longitude, bool camera = false) => new()
    {
        Name = name,
        DurationMinutes = name == "Com relato" ? 12 : 17,
        Segments = [new RouteSegmentRequest
        {
            Latitude = latitude,
            Longitude = longitude,
            SimulatedCameraCoverage = camera
        }]
    };
}
