using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Application.Abstractions;

public interface ISosAlertService
{
    Task<SosAlertResponse> CreateAsync(CreateSosAlertRequest request, CancellationToken cancellationToken);
    Task<SosAlertResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<SosAlertResponse?> AdvanceEscalationAsync(Guid id, CancellationToken cancellationToken);
    Task<SosAlertResponse?> CancelAsync(Guid id, string pin, CancellationToken cancellationToken);
}
