namespace RydrSafe.Application.Common.Exceptions;

/// <summary>
/// Thrown when a Category A report is submitted while the POPIA s58(2) standstill is in force.
/// Surfaces as 503 Service Unavailable — the request is well-formed, we are simply not permitted
/// to process it yet.
/// </summary>
public class CategoryAProcessingDisabledException(string message) : Exception(message);

/// <summary>Thrown when an authenticated user asks for a record they are not entitled to see.</summary>
public class ForbiddenException(string message) : Exception(message);

/// <summary>Thrown when an anti-enumeration or abuse limit is hit. Surfaces as 429.</summary>
public class RateLimitedException(string message) : Exception(message);

/// <summary>Thrown when a moderation action is not a legal transition for the report's current state.</summary>
public class InvalidStateTransitionException(string message) : Exception(message);
