using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Domain.Entities;

public sealed class SosAlert
{
    private SosAlert() { }

    private SosAlert(
        double latitude,
        double longitude,
        string pinSalt,
        string pinHash,
        DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        Latitude = latitude;
        Longitude = longitude;
        PinSalt = pinSalt;
        PinHash = pinHash;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Status = SosAlertStatus.ConfirmationPending;
    }

    public Guid Id { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string PinSalt { get; private set; } = string.Empty;
    public string PinHash { get; private set; } = string.Empty;
    public SosAlertStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static SosAlert Create(
        double latitude,
        double longitude,
        string pinSalt,
        string pinHash,
        DateTime createdAtUtc)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
        if (string.IsNullOrWhiteSpace(pinSalt) || string.IsNullOrWhiteSpace(pinHash))
            throw new ArgumentException("O PIN precisa ser protegido antes de criar o alerta.");

        return new SosAlert(latitude, longitude, pinSalt, pinHash, AsUtc(createdAtUtc));
    }

    public bool IsActive => Status is SosAlertStatus.ConfirmationPending
        or SosAlertStatus.ContactsNotified
        or SosAlertStatus.CityCenterNotified;

    public void AdvanceEscalation(DateTime atUtc)
    {
        Status = Status switch
        {
            SosAlertStatus.ConfirmationPending => SosAlertStatus.ContactsNotified,
            SosAlertStatus.ContactsNotified => SosAlertStatus.CityCenterNotified,
            _ => throw new InvalidOperationException("O alerta não possui outra etapa de escalonamento.")
        };

        UpdatedAtUtc = AsUtc(atUtc);
    }

    public void Cancel(DateTime atUtc)
    {
        if (!IsActive)
            throw new InvalidOperationException("Somente alertas ativos podem ser cancelados.");

        Status = SosAlertStatus.Canceled;
        UpdatedAtUtc = AsUtc(atUtc);
    }

    public void Resolve(DateTime atUtc)
    {
        if (!IsActive)
            throw new InvalidOperationException("Somente alertas ativos podem ser encerrados.");

        Status = SosAlertStatus.Resolved;
        UpdatedAtUtc = AsUtc(atUtc);
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
