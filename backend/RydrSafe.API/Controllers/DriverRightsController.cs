using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.DriverRights.Commands;
using RydrSafe.Application.Features.DriverRights.Queries;
using RydrSafe.Domain.Enums;

namespace RydrSafe.API.Controllers;

/// <summary>
/// Part C. The surface drivers use, and they are not our users — so the self-check, appeal and
/// right-of-reply routes are unauthenticated by necessity. Each one is rate limited, logs the
/// attempt, and answers identically whether or not a record exists.
/// </summary>
[ApiController]
[Route("api/driver-rights")]
public class DriverRightsController(IMediator mediator) : ControllerBase
{
    /// <summary>Clause 34 (POPIA s23) — what do you hold about me?</summary>
    [HttpPost("self-check")]
    [AllowAnonymous]
    [EnableRateLimiting("driver-public")]
    public async Task<IActionResult> SelfCheck([FromBody] DriverSelfCheckRequest body)
    {
        var result = await mediator.Send(new DriverSelfCheckQuery(
            body.RegistrationNumber, body.DriverName, body.ContactEmail, ClientIp()));

        return Ok(result);
    }

    /// <summary>Clause 35 — lodge an appeal. Suspends the driver's public status immediately.</summary>
    [HttpPost("appeals")]
    [AllowAnonymous]
    [EnableRateLimiting("driver-public")]
    public async Task<IActionResult> CreateAppeal([FromBody] CreateAppealRequest body)
    {
        var id = await mediator.Send(new CreateAppealCommand(
            body.RegistrationNumber, body.Grounds, body.Detail,
            body.ContactEmail, body.ContactPhone, body.IdentityEvidenceNote, ClientIp()));

        return Ok(new
        {
            id,
            message = "Your appeal has been received. While it is open, the public status for this "
                      + "record is suspended. We will respond within 30 days (clause 35.2)."
        });
    }

    /// <summary>Clause 6.5 — answer a proposed status change.</summary>
    [HttpPost("right-of-reply")]
    [AllowAnonymous]
    [EnableRateLimiting("driver-public")]
    public async Task<IActionResult> RightOfReply([FromBody] RightOfReplyRequest body)
    {
        await mediator.Send(new SubmitRightOfReplyCommand(
            body.RegistrationNumber, body.Response, body.ContactEmail, ClientIp()));

        return Ok(new { message = "Your response has been recorded and will be considered by a moderator." });
    }

    // ---- Moderator surface ----

    [HttpGet("appeals")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetAppeals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) return BadRequest("page must be at least 1.");
        if (pageSize is < 1 or > 100) return BadRequest("pageSize must be between 1 and 100.");

        return Ok(await mediator.Send(new GetAppealsQuery(page, pageSize)));
    }

    [HttpPut("appeals/{id:guid}")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> ResolveAppeal(Guid id, [FromBody] ResolveAppealRequest body)
    {
        if (!Enum.TryParse<AppealStatus>(body.Status, out var status))
            return BadRequest("Status must be UnderReview, Upheld or Dismissed.");

        await mediator.Send(new ResolveAppealCommand(
            id, status, body.Outcome, GetUserId(),
            body.ReviewedRiskScore, body.ReviewedDriverResponse, body.ReviewedScoringLogic));

        return NoContent();
    }

    /// <summary>Clause 6.5 — send the notice that must precede an adverse status change.</summary>
    [HttpPost("drivers/{driverId:guid}/offer-right-of-reply")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> OfferRightOfReply(Guid driverId, [FromQuery] string proposedStatus)
    {
        if (!Enum.TryParse<DriverStatus>(proposedStatus, out var status))
            return BadRequest("proposedStatus must be a valid driver status.");

        await mediator.Send(new OfferRightOfReplyCommand(driverId, GetUserId(), status));
        return NoContent();
    }

    /// <summary>Clause 7.3 — the only route that writes a driver's public status.</summary>
    [HttpPut("drivers/{driverId:guid}/status")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> SetStatus(Guid driverId, [FromBody] SetDriverStatusRequest body)
    {
        if (!Enum.TryParse<DriverStatus>(body.Status, out var status))
            return BadRequest("Status must be Safe, UnderReview, Flagged or HighRisk.");

        await mediator.Send(new SetDriverStatusCommand(
            driverId, status, GetUserId(), body.Reason,
            body.ReviewedRiskScore, body.ReviewedDriverResponse, body.ReviewedScoringLogic));

        return NoContent();
    }

    /// <summary>What the scoring policy suggests. A recommendation to a moderator, never applied automatically.</summary>
    [HttpGet("drivers/{driverId:guid}/suggested-status")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> SuggestedStatus(Guid driverId)
    {
        var (score, suggested) = await mediator.Send(new GetSuggestedDriverStatusQuery(driverId));
        return Ok(new { riskScore = score, suggestedStatus = suggested });
    }

    /// <summary>
    /// Honours X-Forwarded-For so the rate limit keys on the real client rather than on Render's
    /// proxy, which would otherwise make every visitor share one bucket.
    /// </summary>
    private string? ClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }
}

public record SetDriverStatusRequest(
    string Status,
    string Reason,
    bool ReviewedRiskScore = false,
    bool ReviewedDriverResponse = false,
    bool ReviewedScoringLogic = false);
