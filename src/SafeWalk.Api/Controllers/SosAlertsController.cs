using Microsoft.AspNetCore.Mvc;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Controllers;

[ApiController]
[Route("api/sos-alerts")]
[Produces("application/json")]
public sealed class SosAlertsController(ISosAlertService alertService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SosAlertResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SosAlertResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var alert = await alertService.GetByIdAsync(id, cancellationToken);
        return alert is null ? NotFound() : Ok(alert);
    }

    [HttpPost]
    [ProducesResponseType<SosAlertResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SosAlertResponse>> Create(
        CreateSosAlertRequest request,
        CancellationToken cancellationToken)
    {
        var alert = await alertService.CreateAsync(request, cancellationToken);
        return Created($"/api/sos-alerts/{alert.Id}", alert);
    }

    /// <summary>Avança uma etapa do fluxo para simular o temporizador da demo.</summary>
    [HttpPost("{id:guid}/advance")]
    [ProducesResponseType<SosAlertResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SosAlertResponse>> Advance(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await alertService.AdvanceEscalationAsync(id, cancellationToken);
            return alert is null ? NotFound() : Ok(alert);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Etapa indisponível", Detail = exception.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<SosAlertResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SosAlertResponse>> Cancel(
        Guid id,
        CancelSosAlertRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var alert = await alertService.CancelAsync(id, request.Pin, cancellationToken);
            return alert is null ? NotFound() : Ok(alert);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new ProblemDetails { Title = "PIN inválido", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Alerta indisponível", Detail = exception.Message });
        }
    }
}
