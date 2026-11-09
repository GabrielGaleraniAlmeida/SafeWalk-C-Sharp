using System.ComponentModel.DataAnnotations;

namespace SafeWalk.Api.Application.Contracts;

public sealed class RouteComparisonRequest
{
    [Required]
    [MinLength(2, ErrorMessage = "Envie pelo menos duas opções de rota para comparar.")]
    [MaxLength(4)]
    public List<RouteCandidateRequest> Routes { get; init; } = [];

    public DateTime? DepartureAtUtc { get; init; }
}

public sealed class RouteCandidateRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int DurationMinutes { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "Cada rota precisa ter ao menos um trecho.")]
    public List<RouteSegmentRequest> Segments { get; init; } = [];
}

public sealed class RouteSegmentRequest
{
    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }

    /// <summary>Indica cobertura de câmera simulada para a demonstração do Smart Sampa.</summary>
    public bool SimulatedCameraCoverage { get; init; }
}

public sealed record RouteComparisonResponse(
    DateTime DepartureAtUtc,
    IReadOnlyList<RouteOptionResponse> Options);

public sealed record RouteOptionResponse(
    string Name,
    int DurationMinutes,
    double RiskScore,
    string RiskLevel,
    bool IsSafest,
    bool IsFastest,
    IReadOnlyList<RouteSegmentRiskResponse> Segments);

public sealed record RouteSegmentRiskResponse(
    double Latitude,
    double Longitude,
    double RiskScore,
    string RiskLevel,
    int ActiveReports,
    bool SimulatedCameraCoverage);
