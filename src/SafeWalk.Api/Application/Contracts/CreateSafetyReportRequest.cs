using System.ComponentModel.DataAnnotations;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Contracts;

public sealed class CreateSafetyReportRequest
{
    [EnumDataType(typeof(ReportCategory))]
    public ReportCategory Category { get; init; }

    [Required]
    [StringLength(400, MinimumLength = 5)]
    public string Description { get; init; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; init; }

    [Range(-180, 180)]
    public double Longitude { get; init; }
}
