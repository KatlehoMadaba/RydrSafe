using Microsoft.EntityFrameworkCore;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Enums;
using RydrSafe.Domain.Services;
using RydrSafe.Infrastructure.Persistence;

namespace RydrSafe.Infrastructure.Services;

/// <summary>
/// Applies clause 6.3 across every live report for a driver, in both directions.
///
/// Promotion and demotion are handled in the same pass on purpose. If corroboration could only
/// ever be granted, withdrawing one of two supporting reports would leave the other one
/// published on a threshold that is no longer met — which is exactly the failure the clause
/// exists to prevent.
/// </summary>
public class CorroborationService(AppDbContext db) : ICorroborationService
{
    public async Task<IReadOnlyCollection<Report>> ReevaluateDriverAsync(
        Guid driverId, Guid actorUserId, string reason)
    {
        var live = await db.Reports
            .Where(r => r.DriverId == driverId
                        && r.Status != ReportStatus.Rejected
                        && r.Status != ReportStatus.Withdrawn)
            .ToListAsync();

        var changed = new List<Report>();
        var audits = new List<ReportStatusAudit>();

        foreach (var report in live)
        {
            // Pending reports are not eligible: a moderator has to approve a report before it
            // can be corroborated or corroborate anything else.
            if (report.Status == ReportStatus.Pending) continue;

            var result = CorroborationPolicy.Evaluate(report, live);

            if (result.IsCorroborated && report.Status == ReportStatus.Approved)
            {
                report.Status = ReportStatus.Corroborated;
                report.CorroborationPath = result.Path;
                report.CorroboratedBy = actorUserId;
                report.CorroboratedAt = DateTime.UtcNow;
                report.CorroborationRevokedAt = null;
                report.CorroborationRevokedReason = null;

                audits.Add(Audit(report, ReportStatus.Approved, ReportStatus.Corroborated,
                    $"Corroborated under clause 6.3. {result.Reason} {reason}", actorUserId));
                changed.Add(report);
            }
            else if (!result.IsCorroborated && report.Status == ReportStatus.Corroborated)
            {
                report.Status = ReportStatus.Approved;
                report.CorroborationPath = CorroborationPath.None;
                report.CorroboratedBy = null;
                report.CorroboratedAt = null;
                report.CorroborationRevokedAt = DateTime.UtcNow;
                report.CorroborationRevokedReason = result.Reason;

                audits.Add(Audit(report, ReportStatus.Corroborated, ReportStatus.Approved,
                    $"Corroboration revoked. {result.Reason} {reason}", actorUserId));
                changed.Add(report);
            }
        }

        if (changed.Count > 0)
        {
            db.ReportStatusAudits.AddRange(audits);
            await db.SaveChangesAsync();
        }

        return changed;
    }

    public async Task<CorroborationResult> EvaluateAsync(Guid reportId)
    {
        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException("Report not found.");

        var siblings = await db.Reports
            .Where(r => r.DriverId == report.DriverId
                        && r.Status != ReportStatus.Rejected
                        && r.Status != ReportStatus.Withdrawn)
            .ToListAsync();

        return CorroborationPolicy.Evaluate(report, siblings);
    }

    /// <summary>
    /// Corroboration is applied by the platform rather than chosen by a person, but it still
    /// changes what the public sees, so it is recorded against the moderator whose action
    /// triggered it.
    /// </summary>
    private static ReportStatusAudit Audit(
        Report report, ReportStatus from, ReportStatus to, string reason, Guid actorUserId) => new()
    {
        ReportId = report.Id,
        ActorUserId = actorUserId,
        FromStatus = from,
        ToStatus = to,
        Reason = reason,
        ReviewedReportContent = true
    };
}
