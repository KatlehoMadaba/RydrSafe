using RydrSafe.Domain.Entities;

namespace RydrSafe.Application.Common.Interfaces;

/// <summary>Append-only. There is deliberately no update or delete.</summary>
public interface IAuditRepository
{
    Task AddReportAuditAsync(ReportStatusAudit audit);
    Task AddDriverAuditAsync(DriverStatusAudit audit);
    Task<IEnumerable<ReportStatusAudit>> GetReportAuditsAsync(Guid reportId);
    Task<IEnumerable<DriverStatusAudit>> GetDriverAuditsAsync(Guid driverId);
}
