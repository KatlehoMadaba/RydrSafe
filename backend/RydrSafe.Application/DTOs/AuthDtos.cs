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

/// <summary>
/// Clause 4 — a user correcting their own account details, which POPIA s24 gives them the right
/// to do. Changing the email address changes the sign-in identity, so it is verified for
/// uniqueness server-side.
/// </summary>
public record UpdateProfileRequest(string FullName, string Email);

/// <summary>
/// An in-app password change. The current password is required: an unattended session should not
/// be enough to lock the real account holder out.
/// </summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>
/// Clause 29 / POPIA s24 — the account holder deleting their own account. The password is
/// required because the action is immediate and cannot be undone.
/// </summary>
public record DeleteAccountRequest(string Password, string? Reason = null);

/// <summary>
/// What was actually done, so the confirmation screen can say it rather than promise it.
/// <paramref name="ReportsDeIdentified"/> is the clause 29.2 count: reports are kept but unlinked.
/// </summary>
public record DeleteAccountResponse(
    string Email,
    int ReportsDeIdentified,
    DateTime DeletedAt,
    string Message
);
