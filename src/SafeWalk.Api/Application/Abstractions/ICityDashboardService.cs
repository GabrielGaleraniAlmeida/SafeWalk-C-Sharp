using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Application.Abstractions;

public interface ICityDashboardService
{
    Task<CityDashboardResponse> GetSummaryAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken);
}
