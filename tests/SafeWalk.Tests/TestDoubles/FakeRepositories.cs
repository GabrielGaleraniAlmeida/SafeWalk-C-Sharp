using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Tests.TestDoubles;

internal sealed class FakeSafetyReportRepository : ISafetyReportRepository
{
    private readonly List<SafetyReport> _reports = [];

    public IReadOnlyList<SafetyReport> Reports => _reports;

    public Task AddAsync(SafetyReport report, CancellationToken cancellationToken)
    {
        _reports.Add(report);
        return Task.CompletedTask;
    }

    public Task<SafetyReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_reports.FirstOrDefault(report => report.Id == id));

    public Task<IReadOnlyList<SafetyReport>> GetActiveNearAsync(
        double latitude,
        double longitude,
        double radiusKm,
        DateTime atUtc,
        CancellationToken cancellationToken)
    {
        var reports = _reports
            .Where(report => report.IsActiveAt(atUtc))
            .Where(report => DistanceInKm(latitude, longitude, report.Latitude, report.Longitude) <= radiusKm)
            .ToArray();
        return Task.FromResult<IReadOnlyList<SafetyReport>>(reports);
    }

    public Task<IReadOnlyList<SafetyReport>> GetCreatedBetweenAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var reports = _reports
            .Where(report => report.CreatedAtUtc >= startUtc && report.CreatedAtUtc <= endUtc)
            .ToArray();
        return Task.FromResult<IReadOnlyList<SafetyReport>>(reports);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static double DistanceInKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double radius = 6371;
        var latitudeDelta = (latitude2 - latitude1) * Math.PI / 180;
        var longitudeDelta = (longitude2 - longitude1) * Math.PI / 180;
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                Math.Cos(latitude1 * Math.PI / 180) * Math.Cos(latitude2 * Math.PI / 180) *
                Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}

internal sealed class FakeSosAlertRepository : ISosAlertRepository
{
    private readonly Dictionary<Guid, SosAlert> _alerts = [];

    public Task AddAsync(SosAlert alert, CancellationToken cancellationToken)
    {
        _alerts.Add(alert.Id, alert);
        return Task.CompletedTask;
    }

    public Task<SosAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_alerts.GetValueOrDefault(id));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeJourneyRepository : IJourneyRepository
{
    private readonly Dictionary<Guid, Journey> _journeys = [];

    public Task AddAsync(Journey journey, CancellationToken cancellationToken)
    {
        _journeys.Add(journey.Id, journey);
        return Task.CompletedTask;
    }

    public Task<Journey?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_journeys.GetValueOrDefault(id));

    public Task<Journey?> GetByShareTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(_journeys.Values.FirstOrDefault(journey => journey.ShareTokenHash == tokenHash));

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
