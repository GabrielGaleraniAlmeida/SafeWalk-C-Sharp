using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Application.Services;

public sealed class CityDashboardService(
    ISafetyReportRepository reportRepository,
    IClock clock) : ICityDashboardService
{
    public async Task<CityDashboardResponse> GetSummaryAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        startUtc = ToUtc(startUtc);
        endUtc = ToUtc(endUtc);
        if (startUtc > endUtc)
            throw new ArgumentException("A data inicial deve ser anterior à data final.");

        var reports = await reportRepository.GetCreatedBetweenAsync(startUtc, endUtc, cancellationToken);
        var now = clock.UtcNow;
        var byCategory = reports
            .GroupBy(report => report.Category)
            .Select(group => new ReportCategoryCount(group.Key, group.Count()))
            .OrderByDescending(group => group.Count)
            .ToArray();

        return new CityDashboardResponse(
            startUtc,
            endUtc,
            now,
            reports.Count,
            reports.Count(report => report.IsActiveAt(now)),
            byCategory);
    }

    private static DateTime ToUtc(DateTime value) => value.Kind == DateTimeKind.Utc
        ? value
        : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
