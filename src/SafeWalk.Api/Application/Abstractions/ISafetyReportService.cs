using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Abstractions;

public interface ISafetyReportService
{
    Task<SafetyReport> CreateAsync(CreateSafetyReportRequest request, CancellationToken cancellationToken);
    Task<SafetyReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SafetyReport?> ConfirmAsync(Guid id, CancellationToken cancellationToken);
    Task<SafetyReport?> ResolveAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SafetyReport>> GetActiveNearAsync(
        double latitude,
        double longitude,
        double radiusKm,
        CancellationToken cancellationToken);
}
