using SafeWalk.Api.Domain.Enums;

namespace SafeWalk.Api.Application.Contracts;

public sealed record CityDashboardResponse(
    DateTime StartUtc,
    DateTime EndUtc,
    DateTime GeneratedAtUtc,
    int TotalReports,
    int ActiveReports,
    IReadOnlyList<ReportCategoryCount> ByCategory);

public sealed record ReportCategoryCount(ReportCategory Category, int Count);
