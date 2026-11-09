using Microsoft.AspNetCore.Mvc;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Controllers;

[ApiController]
[Route("api/journeys")]
[Produces("application/json")]
public sealed class JourneysController(IJourneyService journeyService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<JourneyCreatedResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<JourneyCreatedResponse>> Create(
        CreateJourneyRequest request,
        CancellationToken cancellationToken)
    {
        var journey = await journeyService.CreateAsync(request, cancellationToken);
        return Created(journey.ShareUrl, journey);
    }

    [HttpPut("{id:guid}/location")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocation(
        Guid id,
        UpdateJourneyLocationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await journeyService.UpdateLocationAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Retorna apenas a posição atual de uma jornada ativa; o token funciona como link de compartilhamento.</summary>
    [HttpGet("shared/{shareToken}")]
    [ProducesResponseType<JourneyLocationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JourneyLocationResponse>> GetSharedLocation(
        string shareToken,
        CancellationToken cancellationToken)
    {
        var location = await journeyService.GetSharedLocationAsync(shareToken, cancellationToken);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id,
        CompleteJourneyRequest request,
        CancellationToken cancellationToken)
    {
        var completed = await journeyService.CompleteAsync(id, request.TrackerToken, cancellationToken);
        return completed ? NoContent() : NotFound();
    }
}
