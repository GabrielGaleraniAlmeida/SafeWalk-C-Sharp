using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Domain.Entities;

public sealed class SafetyReport
{
    private SafetyReport() { }

    private SafetyReport(
        ReportCategory category,
        string description,
        double latitude,
        double longitude,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        Category = category;
        Description = description.Trim();
        Latitude = latitude;
        Longitude = longitude;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = GetExpiration(category, createdAtUtc);
    }

    public Guid Id { get; private set; }
    public ReportCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public int ConfirmationCount { get; private set; }

    public static SafetyReport Create(
        ReportCategory category,
        string description,
        double latitude,
        double longitude,
        DateTime createdAtUtc)
    {
        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição do relato é obrigatória.", nameof(description));
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        return new SafetyReport(category, description, latitude, longitude, AsUtc(createdAtUtc));
    }

    public bool IsActiveAt(DateTime atUtc) =>
        ResolvedAtUtc is null &&
        CreatedAtUtc <= AsUtc(atUtc) &&
        (ExpiresAtUtc is null || ExpiresAtUtc > AsUtc(atUtc));

    public void Confirm(DateTime atUtc)
    {
        if (!IsActiveAt(atUtc))
            throw new InvalidOperationException("Somente relatos ativos podem receber confirmação.");

        ConfirmationCount++;
    }

    public void Resolve(DateTime atUtc)
    {
        if (ResolvedAtUtc is not null)
            throw new InvalidOperationException("O relato já foi resolvido.");

        ResolvedAtUtc = AsUtc(atUtc);
    }

    private static DateTime? GetExpiration(ReportCategory category, DateTime createdAtUtc) => category switch
    {
        ReportCategory.PublicSafety => createdAtUtc.AddHours(4),
        ReportCategory.Traffic => createdAtUtc.AddHours(6),
        _ => null
    };

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
