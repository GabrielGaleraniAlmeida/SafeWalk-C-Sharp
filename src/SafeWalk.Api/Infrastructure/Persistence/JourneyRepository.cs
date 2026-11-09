using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Infrastructure.Persistence;

public sealed class JourneyRepository(SafeWalkDbContext dbContext) : IJourneyRepository
{
    public async Task AddAsync(Journey journey, CancellationToken cancellationToken)
    {
        await dbContext.Journeys.AddAsync(journey, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Journey?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Journeys.FirstOrDefaultAsync(journey => journey.Id == id, cancellationToken);

    public Task<Journey?> GetByShareTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.Journeys.FirstOrDefaultAsync(
            journey => journey.ShareTokenHash == tokenHash,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
