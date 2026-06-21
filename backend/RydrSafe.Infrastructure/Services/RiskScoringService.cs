using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Enums;
using RydrSafe.Infrastructure.Persistence;

namespace RydrSafe.Infrastructure.Services;

public class RiskScoringService(AppDbContext db) : IRiskScoringService
{
    public async Task<int> CalculateAsync(Guid driverId)
    {
        var reports = await db.Reports
            .Where(r => r.DriverId == driverId && r.Status != ReportStatus.Rejected)
            .ToListAsync();

        if (!reports.Any()) return 0;

        var approvedReports = reports.Where(r => r.Status == ReportStatus.Approved).ToList();
        var pendingReports = reports.Where(r => r.Status == ReportStatus.Pending).ToList();

        double score = 0;

        foreach (var report in reports)
        {
            double severityWeight = report.Severity switch
            {
                ReportSeverity.Low => 5,
                ReportSeverity.Medium => 15,
                ReportSeverity.High => 25,
                ReportSeverity.Critical => 40,
                _ => 5
            };

            double verificationMultiplier = report.Status == ReportStatus.Approved ? 1.5 : 1.0;
            score += severityWeight * verificationMultiplier;
        }

        // Frequency boost: more than 3 reports in 30 days adds 10 points
        var recentCount = reports.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        if (recentCount >= 3) score += 10;

        return Math.Min(100, (int)score);
    }
}
