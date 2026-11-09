using System.ComponentModel.DataAnnotations;

namespace SafeWalk.Api.Application.Contracts;

public sealed class CreateJourneyRequest
{
    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }
}

public sealed class UpdateJourneyLocationRequest
{
    [Required]
    public string TrackerToken { get; init; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }
}

public sealed class CompleteJourneyRequest
{
    [Required]
    public string TrackerToken { get; init; } = string.Empty;
}

public sealed record JourneyCreatedResponse(
    Guid Id,
    string TrackerToken,
    string ShareUrl,
    DateTime StartedAtUtc,
    DateTime ExpiresAtUtc);

public sealed record JourneyLocationResponse(
    double Latitude,
    double Longitude,
    DateTime LastUpdatedAtUtc,
    DateTime ExpiresAtUtc);
