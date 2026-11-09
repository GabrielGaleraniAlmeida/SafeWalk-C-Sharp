using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Abstractions;

public interface IJourneyRepository
{
    Task AddAsync(Journey journey, CancellationToken cancellationToken);
    Task<Journey?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Journey?> GetByShareTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
