using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RydrSafe.Application.Common.Interfaces;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Account.Commands;
using RydrSafe.Application.Features.Auth.Commands;
using RydrSafe.Application.Features.Auth.Queries;
using RydrSafe.Domain.Entities;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator, IConsentRepository consentRepository) : ControllerBase
{
    /// <summary>The agreement version this build serves. Recorded against every consent row.</summary>
    private const string CurrentAgreementVersion = "1.0";

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await mediator.Send(new RegisterCommand(
            request.FullName,
            request.Email,
            request.Password,
            request.DateOfBirth,
            request.Consents?.ToList() ?? [],
            // The client may tell us which version it rendered, but it does not get to invent one.
            string.IsNullOrWhiteSpace(request.AgreementVersion)
                ? CurrentAgreementVersion
                : request.AgreementVersion,
            string.IsNullOrWhiteSpace(request.Locale) ? "en-ZA" : request.Locale,
            CollectionSurface: "web/register",
            IpAddress: ClientIp()));

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password));
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken, Guid.Parse(userId)));
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new GetCurrentUserQuery(Guid.Parse(userId)));
        return Ok(result);
    }

    /// <summary>
    /// Part D / clause 4.3 — what this account agreed to, and when. A consent record the user
    /// cannot see is not much of a record.
    /// </summary>
    [Authorize]
    [HttpGet("me/consents")]
    public async Task<IActionResult> MyConsents()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var consents = await consentRepository.GetByUserIdAsync(Guid.Parse(userId));

        return Ok(consents.Select(c => new ConsentRecordDto(
            c.ConsentKey, c.AgreementVersion, c.Locale, c.CollectionSurface,
            c.Accepted, c.AcceptedAt, c.WithdrawnAt)));
    }

    /// <summary>
    /// POPIA s11(2)(b) — consent may be withdrawn. The required Part D acceptances are the
    /// contract itself, so withdrawing those means closing the account rather than staying on
    /// without them; that is spelled out in the response.
    /// </summary>
    [Authorize]
    [HttpPost("me/consents/{consentKey}/withdraw")]
    public async Task<IActionResult> WithdrawConsent(string consentKey)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        if (ConsentKeys.Required.Contains(consentKey))
            return BadRequest(new
            {
                error = "This acceptance is part of the agreement itself and cannot be withdrawn "
                        + "while the account is open. To withdraw it, close your account — "
                        + "contact the Information Officer."
            });

        await consentRepository.WithdrawAsync(Guid.Parse(userId), consentKey);
        return NoContent();
    }

    /// <summary>Clause 4 / POPIA s24 — the account holder correcting their own details.</summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new UpdateProfileCommand(
            userId.Value, request.FullName, request.Email));

        return Ok(result);
    }

    /// <summary>
    /// Changing a password signs every other session out, so it hands back a new token pair —
    /// otherwise the user would be signed out by their own password change.
    /// </summary>
    [Authorize]
    [HttpPut("me/password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new ChangePasswordCommand(
            userId.Value, request.CurrentPassword, request.NewPassword));

        return Ok(result);
    }

    /// <summary>
    /// Clause 29 / POPIA s24. Immediate and irreversible. Reports the account submitted are kept
    /// but unlinked (clause 29.2), and the email address is released, so the same person can
    /// sign up again afterwards with a clean account.
    /// </summary>
    [Authorize]
    [HttpDelete("me")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> DeleteMe([FromBody] DeleteAccountRequest request)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await mediator.Send(new DeleteAccountCommand(
            userId.Value, request.Password, request.Reason));

        return Ok(result);
    }

    /// <summary>
    /// Revokes the stored refresh token. The client clears its own storage either way; this is
    /// what stops a stolen refresh token outliving the sign-out.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized();

        await mediator.Send(new LogoutCommand(userId.Value));
        return NoContent();
    }

    private Guid? CurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private string? ClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
