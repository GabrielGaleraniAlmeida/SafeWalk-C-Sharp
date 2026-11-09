using SafeWalk.Api.Domain.Entities;
using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Contracts;

public sealed record SafetyReportResponse(
    Guid Id,
    ReportCategory Category,
    string Description,
    double Latitude,
    double Longitude,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    int ConfirmationCount,
    bool IsActive)
{
    public static SafetyReportResponse FromEntity(SafetyReport report, DateTime nowUtc) => new(
        report.Id,
        report.Category,
        report.Description,
        report.Latitude,
        report.Longitude,
        AsUtc(report.CreatedAtUtc),
        report.ExpiresAtUtc is { } expiresAtUtc ? AsUtc(expiresAtUtc) : null,
        report.ConfirmationCount,
        report.IsActiveAt(nowUtc));

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
