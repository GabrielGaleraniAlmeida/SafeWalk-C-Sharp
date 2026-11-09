using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Services;

public sealed class SafetyReportService(
    ISafetyReportRepository repository,
    IClock clock) : ISafetyReportService
{
    public async Task<SafetyReport> CreateAsync(
        CreateSafetyReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = SafetyReport.Create(
            request.Category,
            request.Description,
            request.Latitude,
            request.Longitude,
            clock.UtcNow);

        await repository.AddAsync(report, cancellationToken);
        return report;
    }

    public Task<SafetyReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<SafetyReport?> ConfirmAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await repository.GetByIdAsync(id, cancellationToken);
        if (report is null)
            return null;

        report.Confirm(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<SafetyReport?> ResolveAsync(Guid id, CancellationToken cancellationToken)
    {
        var report = await repository.GetByIdAsync(id, cancellationToken);
        if (report is null)
            return null;

        report.Resolve(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return report;
    }

    public Task<IReadOnlyList<SafetyReport>> GetActiveNearAsync(
        double latitude,
        double longitude,
        double radiusKm,
        CancellationToken cancellationToken) =>
        repository.GetActiveNearAsync(latitude, longitude, radiusKm, clock.UtcNow, cancellationToken);
}
