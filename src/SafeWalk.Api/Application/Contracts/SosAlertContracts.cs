using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Contracts;

public sealed class CreateSosAlertRequest
{
    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }

    [Required]
    [RegularExpression("^\\d{4,8}$", ErrorMessage = "O PIN deve conter de 4 a 8 dígitos.")]
    public string Pin { get; init; } = string.Empty;
}

public sealed class CancelSosAlertRequest
{
    [Required]
    [RegularExpression("^\\d{4,8}$", ErrorMessage = "O PIN deve conter de 4 a 8 dígitos.")]
    public string Pin { get; init; } = string.Empty;
}

public sealed record SosAlertResponse(
    Guid Id,
    double Latitude,
    double Longitude,
    SosAlertStatus Status,
    string StageDescription,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
