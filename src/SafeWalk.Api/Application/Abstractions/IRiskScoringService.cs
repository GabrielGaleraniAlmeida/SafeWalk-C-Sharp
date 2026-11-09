using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Application.Abstractions;

public interface IRiskScoringService
{
    Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken);
}
