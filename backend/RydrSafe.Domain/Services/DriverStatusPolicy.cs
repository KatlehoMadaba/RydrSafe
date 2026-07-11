using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Services;

/// <summary>
/// Single source of truth for turning a driver's report history into a status.
/// Used by the reporting and verification flows so a driver's status stays consistent
/// no matter which action recalculates it.
/// </summary>
public static class DriverStatusPolicy
{
    /// <param name="riskScore">Weighted risk score (0–100) from <c>IRiskScoringService</c>.</param>
    /// <param name="reportCount">Number of reports filed against the driver.</param>
    /// <param name="reportedToPolice">Whether any report indicated the incident was reported to the police.</param>
    public static DriverStatus Evaluate(int riskScore, int reportCount, bool reportedToPolice)
    {
        // Multiple reports where at least one was taken to the police is treated as high risk,
        // regardless of the numeric score.
        if (reportCount >= 2 && reportedToPolice)
            return DriverStatus.HighRisk;

        return riskScore switch
        {
            >= 80 => DriverStatus.HighRisk,
            >= 60 => DriverStatus.Flagged,
            >= 30 => DriverStatus.UnderReview,
            // Any report at all keeps the driver under review even if the score is still low.
            _ => reportCount >= 1 ? DriverStatus.UnderReview : DriverStatus.Safe
        };
    }
}
