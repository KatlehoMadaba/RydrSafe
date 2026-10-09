namespace RydrSafe.Domain.Enums;

/// <summary>
/// How precisely the reporter could place the incident in time.
///
/// A reporter who remembers only the month should not be made to invent a day, and the platform
/// should not then present that invented day as fact. The stored <c>IncidentDate</c> is always the
/// first instant of the period the reporter actually identified; this says how much of it to
/// believe, so a report can be displayed as "September 2026" rather than "1 September 2026".
/// </summary>
public enum IncidentDatePrecision
{
    /// <summary>An exact calendar date.</summary>
    Day = 0,

    /// <summary>A month and year. The stored date is the first of that month.</summary>
    Month = 1,

    /// <summary>A year only. The stored date is 1 January of that year.</summary>
    Year = 2
}
