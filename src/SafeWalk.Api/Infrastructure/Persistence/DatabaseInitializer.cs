using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Domain.Entities;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(SafeWalkDbContext dbContext, IClock clock, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (await dbContext.SafetyReports.AnyAsync(cancellationToken))
            return;

        var now = clock.UtcNow;
        dbContext.SafetyReports.AddRange(
            SafetyReport.Create(
                ReportCategory.PublicSafety,
                "Relato de roubo de celular perto do cruzamento.",
                -23.5509,
                -46.6342,
                now.AddMinutes(-25)),
            SafetyReport.Create(
                ReportCategory.Lighting,
                "Trecho com iluminação pública insuficiente.",
                -23.5520,
                -46.6328,
                now.AddHours(-3)),
            SafetyReport.Create(
                ReportCategory.Sidewalk,
                "Calçada danificada; caminhe com atenção.",
                -23.5488,
                -46.6350,
                now.AddHours(-2)));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
