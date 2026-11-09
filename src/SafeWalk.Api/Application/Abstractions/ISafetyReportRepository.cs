using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Abstractions;

public interface ISafetyReportRepository
{
    Task AddAsync(SafetyReport report, CancellationToken cancellationToken);
    Task<SafetyReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SafetyReport>> GetActiveNearAsync(
        double latitude,
        double longitude,
        double radiusKm,
        DateTime atUtc,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<SafetyReport>> GetCreatedBetweenAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
