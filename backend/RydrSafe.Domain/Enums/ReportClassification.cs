namespace RydrSafe.Domain.Enums;

/// <summary>
/// User agreement clause 6.1. Category A is an allegation of criminal conduct and is
/// special personal information under POPIA s26(b); it is subject to the corroboration
/// threshold in clause 6.3 and to the s57 prior-authorisation gate.
/// </summary>
public enum ReportClassification
{
    /// <summary>Alleged criminal conduct. Restricted processing.</summary>
    CategoryA,

    /// <summary>Safety and conduct concern that does not allege an offence.</summary>
    CategoryB
}
