using System.Security.Cryptography;
using System.Text;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Application.Services;

public sealed class JourneyService(
    IJourneyRepository repository,
    IClock clock) : IJourneyService
{
    public async Task<JourneyCreatedResponse> CreateAsync(
        CreateJourneyRequest request,
        CancellationToken cancellationToken)
    {
        var shareToken = CreateToken();
        var trackerToken = CreateToken();
        var journey = Journey.Create(
            request.Latitude,
            request.Longitude,
            HashToken(shareToken),
            HashToken(trackerToken),
            clock.UtcNow);

        await repository.AddAsync(journey, cancellationToken);
        return new JourneyCreatedResponse(
            journey.Id,
            trackerToken,
            $"/api/journeys/shared/{shareToken}",
            AsUtc(journey.StartedAtUtc),
            AsUtc(journey.ExpiresAtUtc));
    }

    public async Task<bool> UpdateLocationAsync(
        Guid id,
        UpdateJourneyLocationRequest request,
        CancellationToken cancellationToken)
    {
        var journey = await repository.GetByIdAsync(id, cancellationToken);
        if (journey is null || !HasValidTrackerToken(journey, request.TrackerToken))
            return false;

        var now = clock.UtcNow;
        if (!journey.IsVisibleAt(now))
        {
            journey.Expire(now);
            await repository.SaveChangesAsync(cancellationToken);
            return false;
        }

        journey.UpdateLocation(request.Latitude, request.Longitude, now);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<JourneyLocationResponse?> GetSharedLocationAsync(
        string shareToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shareToken))
            return null;

        var journey = await repository.GetByShareTokenHashAsync(HashToken(shareToken), cancellationToken);
        if (journey is null)
            return null;

        var now = clock.UtcNow;
        if (!journey.IsVisibleAt(now))
        {
            journey.Expire(now);
            await repository.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (journey.CurrentLatitude is null || journey.CurrentLongitude is null)
            return null;

        return new JourneyLocationResponse(
            journey.CurrentLatitude.Value,
            journey.CurrentLongitude.Value,
            AsUtc(journey.LastUpdatedAtUtc),
            AsUtc(journey.ExpiresAtUtc));
    }

    public async Task<bool> CompleteAsync(
        Guid id,
        string trackerToken,
        CancellationToken cancellationToken)
    {
        var journey = await repository.GetByIdAsync(id, cancellationToken);
        if (journey is null || !HasValidTrackerToken(journey, trackerToken))
            return false;

        var now = clock.UtcNow;
        if (!journey.IsVisibleAt(now))
        {
            journey.Expire(now);
            await repository.SaveChangesAsync(cancellationToken);
            return false;
        }

        journey.Complete(now);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool HasValidTrackerToken(Journey journey, string trackerToken) =>
        !string.IsNullOrWhiteSpace(journey.TrackerTokenHash) &&
        TokenMatches(trackerToken, journey.TrackerTokenHash);

    private static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static bool TokenMatches(string token, string expectedHash)
    {
        var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var expectedBytes = Convert.FromHexString(expectedHash);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedBytes);
    }
}
