using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Abstractions;

public interface ISosAlertRepository
{
    Task AddAsync(SosAlert alert, CancellationToken cancellationToken);
    Task<SosAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
