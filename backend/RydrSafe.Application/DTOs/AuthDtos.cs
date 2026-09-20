namespace RydrSafe.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,

    /// <summary>Clause 3.1. Required so the 18+ rule is applied to a date rather than a tick alone.</summary>
    DateOnly? DateOfBirth = null,

    /// <summary>Part D. Every required key in <c>ConsentKeys.Required</c> must be present and accepted.</summary>
    IEnumerable<ConsentAcceptance>? Consents = null,

    /// <summary>Version of the agreement the user was shown. Recorded against each consent row.</summary>
    string? AgreementVersion = null,

    string? Locale = null
);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(string Id, string FullName, string Email, string Role, string CreatedAt);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string FullName,
    string Email,
    string Role
);

public record ConsentRecordDto(
    string ConsentKey,
    string AgreementVersion,
    string Locale,
    string CollectionSurface,
    bool Accepted,
    DateTime AcceptedAt,
    DateTime? WithdrawnAt
);
