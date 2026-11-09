using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Infrastructure.Persistence;

public sealed class SosAlertRepository(SafeWalkDbContext dbContext) : ISosAlertRepository
{
    public async Task AddAsync(SosAlert alert, CancellationToken cancellationToken)
    {
        await dbContext.SosAlerts.AddAsync(alert, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<SosAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SosAlerts.FirstOrDefaultAsync(alert => alert.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
