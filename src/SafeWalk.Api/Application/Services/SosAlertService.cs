using System.Security.Cryptography;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;
using SafeWalk.Api.Domain.Entities;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Services;

public sealed class SosAlertService(
    ISosAlertRepository repository,
    IClock clock) : ISosAlertService
{
    public async Task<SosAlertResponse> CreateAsync(
        CreateSosAlertRequest request,
        CancellationToken cancellationToken)
    {
        var (salt, hash) = PinHasher.Create(request.Pin);
        var alert = SosAlert.Create(
            request.Latitude,
            request.Longitude,
            salt,
            hash,
            clock.UtcNow);

        await repository.AddAsync(alert, cancellationToken);
        return ToResponse(alert);
    }

    public async Task<SosAlertResponse?> AdvanceEscalationAsync(Guid id, CancellationToken cancellationToken)
    {
        var alert = await repository.GetByIdAsync(id, cancellationToken);
        if (alert is null)
            return null;

        alert.AdvanceEscalation(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(alert);
    }

    public async Task<SosAlertResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var alert = await repository.GetByIdAsync(id, cancellationToken);
        return alert is null ? null : ToResponse(alert);
    }

    public async Task<SosAlertResponse?> CancelAsync(
        Guid id,
        string pin,
        CancellationToken cancellationToken)
    {
        var alert = await repository.GetByIdAsync(id, cancellationToken);
        if (alert is null)
            return null;
        if (!alert.IsActive)
            throw new InvalidOperationException("O alerta já foi encerrado.");
        if (!PinHasher.Verify(pin, alert.PinSalt, alert.PinHash))
            throw new UnauthorizedAccessException("PIN incorreto.");

        alert.Cancel(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToResponse(alert);
    }

    private static SosAlertResponse ToResponse(SosAlert alert) => new(
        alert.Id,
        alert.Latitude,
        alert.Longitude,
        alert.Status,
        alert.Status switch
        {
            SosAlertStatus.ConfirmationPending => "Aguardando confirmação de segurança",
            SosAlertStatus.ContactsNotified => "Contatos de confiança notificados (simulado)",
            SosAlertStatus.CityCenterNotified => "Central da GCM notificada (simulado)",
            SosAlertStatus.Canceled => "Alerta cancelado com PIN",
            SosAlertStatus.Resolved => "Alerta encerrado",
            _ => "Estado desconhecido"
        },
        AsUtc(alert.CreatedAtUtc),
        AsUtc(alert.UpdatedAtUtc));

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static class PinHasher
    {
        private const int Iterations = 120_000;
        private const int HashLength = 32;

        public static (string Salt, string Hash) Create(string pin)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, HashLength);
            return (Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string pin, string saltBase64, string expectedHashBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var expectedHash = Convert.FromBase64String(expectedHashBase64);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, HashLength);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
