using RydrSafe.Domain.Entities;
using RydrSafe.Domain.Services;

namespace RydrSafe.Application.Common.Interfaces;

/// <summary>
/// Applies clause 6.3 across a driver's reports. Runs whenever something that could change a
/// corroboration outcome happens: a report is approved, an official reference is verified, a
/// public-record source is recorded, or a supporting report is withdrawn or rejected.
/// </summary>
public interface ICorroborationService
{
    /// <summary>
    /// Re-tests every live report for this driver and promotes or demotes each one.
    /// Returns the reports whose status changed.
    /// </summary>
    Task<IReadOnlyCollection<Report>> ReevaluateDriverAsync(Guid driverId, Guid actorUserId, string reason);

    /// <summary>Tests a single report without persisting anything. Used to preview a decision.</summary>
    Task<CorroborationResult> EvaluateAsync(Guid reportId);
}
