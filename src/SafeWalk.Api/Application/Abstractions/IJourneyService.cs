using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Application.Abstractions;

public interface IJourneyService
{
    Task<JourneyCreatedResponse> CreateAsync(CreateJourneyRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateLocationAsync(Guid id, UpdateJourneyLocationRequest request, CancellationToken cancellationToken);
    Task<JourneyLocationResponse?> GetSharedLocationAsync(string shareToken, CancellationToken cancellationToken);
    Task<bool> CompleteAsync(Guid id, string trackerToken, CancellationToken cancellationToken);
}
