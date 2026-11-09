using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Domain.Entities;

public sealed class Journey
{
    private Journey() { }

    private Journey(
        double latitude,
        double longitude,
        string shareTokenHash,
        string trackerTokenHash,
        DateTime startedAtUtc)
    {
        Id = Guid.NewGuid();
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        ShareTokenHash = shareTokenHash;
        TrackerTokenHash = trackerTokenHash;
        StartedAtUtc = startedAtUtc;
        LastUpdatedAtUtc = startedAtUtc;
        ExpiresAtUtc = startedAtUtc.AddHours(4);
        Status = JourneyStatus.Active;
    }

    public Guid Id { get; private set; }
    public double? CurrentLatitude { get; private set; }
    public double? CurrentLongitude { get; private set; }
    public string? ShareTokenHash { get; private set; }
    public string? TrackerTokenHash { get; private set; }
    public JourneyStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public static Journey Create(
        double latitude,
        double longitude,
        string shareTokenHash,
        string trackerTokenHash,
        DateTime startedAtUtc)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        return new Journey(
            latitude,
            longitude,
            shareTokenHash,
            trackerTokenHash,
            AsUtc(startedAtUtc));
    }

    public bool IsVisibleAt(DateTime atUtc) =>
        Status == JourneyStatus.Active && ExpiresAtUtc > AsUtc(atUtc);

    public void UpdateLocation(double latitude, double longitude, DateTime atUtc)
    {
        if (!IsVisibleAt(atUtc))
            throw new InvalidOperationException("A jornada não está ativa.");
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        LastUpdatedAtUtc = AsUtc(atUtc);
    }

    public void Complete(DateTime atUtc)
    {
        if (Status != JourneyStatus.Active)
            throw new InvalidOperationException("A jornada não está ativa.");

        Status = JourneyStatus.Completed;
        ClearSensitiveData();
        LastUpdatedAtUtc = AsUtc(atUtc);
    }

    public void Expire(DateTime atUtc)
    {
        if (Status != JourneyStatus.Active)
            return;

        Status = JourneyStatus.Expired;
        ClearSensitiveData();
        LastUpdatedAtUtc = AsUtc(atUtc);
    }

    private void ClearSensitiveData()
    {
        CurrentLatitude = null;
        CurrentLongitude = null;
        ShareTokenHash = null;
        TrackerTokenHash = null;
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
