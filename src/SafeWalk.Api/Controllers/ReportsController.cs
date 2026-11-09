using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Contracts;

namespace SafeWalk.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public sealed class ReportsController(
    ISafetyReportService reportService,
    IClock clock) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SafetyReportResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SafetyReportResponse>>> GetNear(
        [FromQuery, Range(-90, 90)] double latitude,
        [FromQuery, Range(-180, 180)] double longitude,
        [FromQuery, Range(0.05, 2)] double radiusKm = 0.5,
        CancellationToken cancellationToken = default)
    {
        var reports = await reportService.GetActiveNearAsync(latitude, longitude, radiusKm, cancellationToken);
        var response = reports.Select(report => SafetyReportResponse.FromEntity(report, clock.UtcNow)).ToArray();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<SafetyReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SafetyReportResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var report = await reportService.GetByIdAsync(id, cancellationToken);
        return report is null
            ? NotFound()
            : Ok(SafetyReportResponse.FromEntity(report, clock.UtcNow));
    }

    [HttpPost]
    [ProducesResponseType<SafetyReportResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SafetyReportResponse>> Create(
        CreateSafetyReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await reportService.CreateAsync(request, cancellationToken);
            var response = SafetyReportResponse.FromEntity(report, clock.UtcNow);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Relato inválido", Detail = exception.Message });
        }
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType<SafetyReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SafetyReportResponse>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await reportService.ConfirmAsync(id, cancellationToken);
            return report is null
                ? NotFound()
                : Ok(SafetyReportResponse.FromEntity(report, clock.UtcNow));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Relato indisponível", Detail = exception.Message });
        }
    }

    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType<SafetyReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SafetyReportResponse>> Resolve(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await reportService.ResolveAsync(id, cancellationToken);
            return report is null
                ? NotFound()
                : Ok(SafetyReportResponse.FromEntity(report, clock.UtcNow));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Relato indisponível", Detail = exception.Message });
        }
    }
}
