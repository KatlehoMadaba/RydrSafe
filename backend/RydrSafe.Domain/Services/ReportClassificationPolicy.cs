using RydrSafe.Domain.Enums;

namespace RydrSafe.Domain.Services;

/// <summary>
/// User agreement clause 6.1. Decides whether a report alleges criminal conduct
/// (Category A, special personal information under POPIA s26(b)) or is a safety and
/// conduct concern (Category B).
/// </summary>
public static class ReportClassificationPolicy
{
    /// <summary>
    /// Categories that allege an offence. <c>Other</c> is deliberately treated as Category A
    /// until a moderator reclassifies it — the safe default is the restrictive one.
    /// </summary>
    public static ReportClassification Classify(ReportCategory category) => category switch
    {
        ReportCategory.UnsafeVehicle => ReportClassification.CategoryB,

        ReportCategory.Assault
            or ReportCategory.Theft
            or ReportCategory.Fraud
            or ReportCategory.IntoxicatedDriving
            or ReportCategory.Harassment
            or ReportCategory.RecklessDriving => ReportClassification.CategoryA,

        // Other, and anything added to the enum later, defaults to the restricted class.
        _ => ReportClassification.CategoryA
    };

    /// <summary>
    /// Only an <c>Other</c> report may be reclassified. Reclassifying a named criminal category
    /// would let a moderator route an allegation of an offence around the clause 6.3 threshold.
    /// </summary>
    public static bool CanReclassify(ReportCategory category) => category == ReportCategory.Other;
}
