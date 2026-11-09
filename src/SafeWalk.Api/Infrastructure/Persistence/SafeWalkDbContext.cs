using Microsoft.EntityFrameworkCore;
using SafeWalk.Api.Domain.Entities;

namespace SafeWalk.Api.Infrastructure.Persistence;

public sealed class SafeWalkDbContext(DbContextOptions<SafeWalkDbContext> options) : DbContext(options)
{
    public DbSet<SafetyReport> SafetyReports => Set<SafetyReport>();
    public DbSet<SosAlert> SosAlerts => Set<SosAlert>();
    public DbSet<Journey> Journeys => Set<Journey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SafetyReport>(entity =>
        {
            entity.HasKey(report => report.Id);
            entity.Property(report => report.Description).HasMaxLength(400).IsRequired();
            entity.HasIndex(report => new { report.Category, report.CreatedAtUtc });
            entity.HasIndex(report => new { report.Latitude, report.Longitude });
        });

        modelBuilder.Entity<SosAlert>(entity =>
        {
            entity.HasKey(alert => alert.Id);
            entity.Property(alert => alert.PinSalt).IsRequired();
            entity.Property(alert => alert.PinHash).IsRequired();
            entity.HasIndex(alert => alert.CreatedAtUtc);
        });

        modelBuilder.Entity<Journey>(entity =>
        {
            entity.HasKey(journey => journey.Id);
            entity.HasIndex(journey => journey.ShareTokenHash).IsUnique();
            entity.HasIndex(journey => journey.ExpiresAtUtc);
        });
    }
}
