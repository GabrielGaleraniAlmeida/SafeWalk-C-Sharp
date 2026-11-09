using Microsoft.AspNetCore.Mvc;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Controllers;

[ApiController]
[Route("api/routes")]
[Produces("application/json")]
public sealed class RoutesController(IRiskScoringService riskScoringService) : ControllerBase
{
    /// <summary>Compara alternativas de trajeto usando relatos ativos e cobertura de câmera simulada.</summary>
    [HttpPost("compare")]
    [ProducesResponseType<RouteComparisonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RouteComparisonResponse>> Compare(
        RouteComparisonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await riskScoringService.CompareAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Comparação inválida", Detail = exception.Message });
        }
    }
}
