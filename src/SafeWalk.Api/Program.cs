using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Application.Abstractions;
using SafeWalk.Api.Application.Services;
using SafeWalk.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var configuredDatabasePath = builder.Configuration["Storage:DatabasePath"] ?? "../../data/safewalk.db";
var databasePath = Path.GetFullPath(configuredDatabasePath, builder.Environment.ContentRootPath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<SafeWalkDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ISafetyReportRepository, SafetyReportRepository>();
builder.Services.AddScoped<ISosAlertRepository, SosAlertRepository>();
builder.Services.AddScoped<IJourneyRepository, JourneyRepository>();

builder.Services.AddScoped<ISafetyReportService, SafetyReportService>();
builder.Services.AddScoped<IRiskScoringService, RiskScoringService>();
builder.Services.AddScoped<ISosAlertService, SosAlertService>();
builder.Services.AddScoped<IJourneyService, JourneyService>();
builder.Services.AddScoped<ICityDashboardService, CityDashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "SafeWalk API" }));
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SafeWalkDbContext>();
    var clock = scope.ServiceProvider.GetRequiredService<IClock>();
    await DatabaseInitializer.InitializeAsync(dbContext, clock);
}

app.Run();

public partial class Program { }
