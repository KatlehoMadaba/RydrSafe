using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Enums;
using RydrSafe.Infrastructure.Persistence;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// Clause 7.1. The score is derived only from reports that have cleared the clause 6.3
/// corroboration threshold.
///
/// This used to include pending and approved reports, which meant an uncorroborated allegation
/// moved a driver's public standing the moment it was filed — the precise harm clause 6 is
/// built to prevent. One person could raise a stranger's risk score on their own say-so.
/// </summary>
public class RiskScoringService(AppDbContext db, ICategoryAGate categoryAGate) : IRiskScoringService
{
    public async Task<int> CalculateAsync(Guid driverId)
    {
        var reports = await db.Reports
            .Where(r => r.DriverId == driverId && r.Status == ReportStatus.Corroborated)
            .Select(r => new { r.Severity, r.Classification, r.CreatedAt })
            .ToListAsync();

        // While the s58(2) standstill is in force, Category A reports contribute nothing —
        // scoring them would be processing them.
        if (!categoryAGate.IsProcessingEnabled)
            reports = reports.Where(r => r.Classification == ReportClassification.CategoryB).ToList();

        if (reports.Count == 0) return 0;

        double score = 0;

        foreach (var report in reports)
        {
            score += report.Severity switch
            {
                ReportSeverity.Low => 5,
                ReportSeverity.Medium => 15,
                ReportSeverity.High => 25,
                ReportSeverity.Critical => 40,
                _ => 5
            };
        }

        // Frequency boost: three or more corroborated reports in 30 days.
        var recentCount = reports.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        if (recentCount >= 3) score += 10;

        return Math.Min(100, (int)score);
    }
}
