using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Infrastructure.Persistence;

public sealed class SafetyReportRepository(SafeWalkDbContext dbContext) : ISafetyReportRepository
{
    public async Task AddAsync(SafetyReport report, CancellationToken cancellationToken)
    {
        await dbContext.SafetyReports.AddAsync(report, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<SafetyReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SafetyReports.FirstOrDefaultAsync(report => report.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SafetyReport>> GetActiveNearAsync(
        double latitude,
        double longitude,
        double radiusKm,
        DateTime atUtc,
        CancellationToken cancellationToken)
    {
        var latitudeDelta = radiusKm / 111.32;
        var longitudeScale = Math.Max(Math.Cos(latitude * Math.PI / 180), 0.01);
        var longitudeDelta = radiusKm / (111.32 * longitudeScale);

        return await dbContext.SafetyReports
            .AsNoTracking()
            .Where(report => report.CreatedAtUtc <= atUtc)
            .Where(report => report.ExpiresAtUtc == null || report.ExpiresAtUtc > atUtc)
            .Where(report => report.ResolvedAtUtc == null)
            .Where(report => report.Latitude >= latitude - latitudeDelta && report.Latitude <= latitude + latitudeDelta)
            .Where(report => report.Longitude >= longitude - longitudeDelta && report.Longitude <= longitude + longitudeDelta)
            .OrderByDescending(report => report.CreatedAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SafetyReport>> GetCreatedBetweenAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken) =>
        await dbContext.SafetyReports
            .AsNoTracking()
            .Where(report => report.CreatedAtUtc >= startUtc && report.CreatedAtUtc <= endUtc)
            .OrderByDescending(report => report.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
