using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Domain.Entities;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Services;

public sealed class RiskScoringService(
    ISafetyReportRepository reportRepository,
    IClock clock) : IRiskScoringService
{
    private const double SearchRadiusKm = 0.5;

    public async Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Routes.Count < 2)
            throw new ArgumentException("Envie pelo menos duas opções de rota para comparar.", nameof(request));

        var departureAtUtc = request.DepartureAtUtc is { } requestedTime
            ? ToUtc(requestedTime)
            : clock.UtcNow;
        var scoredOptions = new List<RouteOptionResponse>(request.Routes.Count);

        foreach (var route in request.Routes)
        {
            var segmentResults = new List<RouteSegmentRiskResponse>(route.Segments.Count);
            foreach (var segment in route.Segments)
            {
                var reports = await reportRepository.GetActiveNearAsync(
                    segment.Latitude,
                    segment.Longitude,
                    SearchRadiusKm,
                    departureAtUtc,
                    cancellationToken);
                var nearbyReports = reports
                    .Where(report => DistanceInKm(segment.Latitude, segment.Longitude, report.Latitude, report.Longitude) <= SearchRadiusKm)
                    .ToArray();
                var score = CalculateSegmentScore(segment, nearbyReports);

                segmentResults.Add(new RouteSegmentRiskResponse(
                    segment.Latitude,
                    segment.Longitude,
                    Round(score),
                    GetRiskLevel(score),
                    nearbyReports.Length,
                    segment.SimulatedCameraCoverage));
            }

            var routeScore = segmentResults.Average(segment => segment.RiskScore);
            scoredOptions.Add(new RouteOptionResponse(
                route.Name,
                route.DurationMinutes,
                Round(routeScore),
                GetRiskLevel(routeScore),
                IsSafest: false,
                IsFastest: false,
                segmentResults));
        }

        var safestScore = scoredOptions.Min(option => option.RiskScore);
        var fastestDuration = scoredOptions.Min(option => option.DurationMinutes);
        var options = scoredOptions
            .Select(option => option with
            {
                IsSafest = option.RiskScore == safestScore,
                IsFastest = option.DurationMinutes == fastestDuration
            })
            .ToArray();

        return new RouteComparisonResponse(departureAtUtc, options);
    }

    private static double CalculateSegmentScore(RouteSegmentRequest segment, IReadOnlyList<SafetyReport> reports)
    {
        var score = 8.0;
        foreach (var report in reports)
        {
            var distance = DistanceInKm(segment.Latitude, segment.Longitude, report.Latitude, report.Longitude);
            var proximityFactor = Math.Clamp(1 - distance / SearchRadiusKm, 0.15, 1);
            var confirmationFactor = Math.Min(1, 0.35 + report.ConfirmationCount * 0.12);
            score += CategoryWeight(report.Category) * proximityFactor * confirmationFactor * 34;
        }

        if (segment.SimulatedCameraCoverage)
            score *= 0.88;

        return Math.Clamp(score, 0, 100);
    }

    private static double CategoryWeight(ReportCategory category) => category switch
    {
        ReportCategory.PublicSafety => 1.00,
        ReportCategory.Traffic => 0.72,
        ReportCategory.Flood => 0.82,
        ReportCategory.Sidewalk => 0.40,
        ReportCategory.Lighting => 0.48,
        _ => 0.35
    };

    private static double DistanceInKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusKm = 6371;
        var latitudeDelta = DegreesToRadians(latitude2 - latitude1);
        var longitudeDelta = DegreesToRadians(longitude2 - longitude1);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                Math.Cos(DegreesToRadians(latitude1)) * Math.Cos(DegreesToRadians(latitude2)) *
                Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static string GetRiskLevel(double score) => score switch
    {
        < 25 => "Baixo",
        < 50 => "Moderado",
        < 75 => "Alto",
        _ => "Muito alto"
    };

    private static double Round(double score) => Math.Round(score, 1, MidpointRounding.AwayFromZero);

    private static DateTime ToUtc(DateTime value) => value.Kind == DateTimeKind.Utc
        ? value
        : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
