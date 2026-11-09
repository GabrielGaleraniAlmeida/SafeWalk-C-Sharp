using Microsoft.AspNetCore.Mvc;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public sealed class CityDashboardController(
    ICityDashboardService dashboardService,
    IClock clock) : ControllerBase
{
    [HttpGet("reports")]
    [ProducesResponseType<CityDashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CityDashboardResponse>> GetReports(
        [FromQuery] DateTime? startUtc,
        [FromQuery] DateTime? endUtc,
        CancellationToken cancellationToken)
    {
        var end = endUtc ?? clock.UtcNow;
        var start = startUtc ?? end.AddDays(-30);
        try
        {
            return Ok(await dashboardService.GetSummaryAsync(start, end, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Período inválido", Detail = exception.Message });
        }
    }
}
