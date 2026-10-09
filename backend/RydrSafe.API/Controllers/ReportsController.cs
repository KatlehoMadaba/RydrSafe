using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Reports.Commands;
using RydrSafe.Application.Features.Reports.Queries;
using RydrSafe.Domain.Enums;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("reporting")]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request)
    {
        var userId = GetUserId();

        // An unparseable value must not silently become an exact date — that would turn "some
        // time in 2026" into a specific day nobody claimed.
        if (!Enum.TryParse<IncidentDatePrecision>(
                string.IsNullOrWhiteSpace(request.IncidentDatePrecision)
                    ? nameof(IncidentDatePrecision.Day)
                    : request.IncidentDatePrecision,
                ignoreCase: true,
                out var precision))
            return BadRequest(new { error = "IncidentDatePrecision must be Day, Month or Year." });

        var id = await mediator.Send(new CreateReportCommand(
            request.DriverName, request.RegistrationNumber, userId,
            request.Category, request.Severity, request.Description, request.IncidentDate,
            request.ReportedToPolice, request.OfficialReference, request.DeviceFingerprint,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            request.IsAnonymous,
            precision));

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Moderation queue. Carries descriptions, so it is role-restricted.</summary>
    [HttpGet]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) return BadRequest("page must be at least 1.");
        if (pageSize is < 1 or > 100) return BadRequest("pageSize must be between 1 and 100.");

        var result = await mediator.Send(new GetReportsQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>The caller's own submissions.</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var result = await mediator.Send(new GetMyReportsQuery(GetUserId()));
        return Ok(result);
    }

    /// <summary>
    /// Returns the full report including its description and the reporter's name. Clause 6.4
    /// allows that only for the reporter themselves and for moderators — the handler enforces it.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetReportByIdQuery(id, GetUserId(), IsModerator()));
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>Clause 6.4 — the reduced view of a driver's corroborated reports. Any signed-in user.</summary>
    [HttpGet("driver/{driverId:guid}/public-summary")]
    public async Task<IActionResult> GetPublicSummary(Guid driverId)
    {
        var result = await mediator.Send(new GetPublicReportSummariesQuery(driverId));
        return Ok(result);
    }

    [HttpGet("{id:guid}/audits")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetAudits(Guid id)
    {
        var result = await mediator.Send(new GetReportAuditsQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ModerateReportRequest body)
    {
        await mediator.Send(new ModerateReportCommand(
            id, ReportStatus.Approved, GetUserId(), body.Reason,
            body.ReviewedReportContent, body.ReviewedDriverResponse, body.ReviewedRiskScore));

        return NoContent();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ModerateReportRequest body)
    {
        await mediator.Send(new ModerateReportCommand(
            id, ReportStatus.Rejected, GetUserId(), body.Reason,
            body.ReviewedReportContent, body.ReviewedDriverResponse, body.ReviewedRiskScore));

        return NoContent();
    }

    /// <summary>Clause 6.1 — reclassify an <c>Other</c> report between Category A and B.</summary>
    [HttpPut("{id:guid}/classification")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> Reclassify(Guid id, [FromBody] ReclassifyReportRequest body)
    {
        if (!Enum.TryParse<ReportClassification>(body.Classification, out var classification))
            return BadRequest("Classification must be CategoryA or CategoryB.");

        await mediator.Send(new ReclassifyReportCommand(id, classification, GetUserId(), body.Reason));
        return NoContent();
    }

    /// <summary>Clause 6.3(b).</summary>
    [HttpPut("{id:guid}/official-reference")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> VerifyOfficialReference(
        Guid id, [FromBody] VerifyOfficialReferenceRequest body)
    {
        await mediator.Send(new VerifyOfficialReferenceCommand(id, body.Verified, GetUserId(), body.Reason));
        return NoContent();
    }

    /// <summary>Clause 6.3(c).</summary>
    [HttpPut("{id:guid}/public-record")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> RecordPublicRecord(Guid id, [FromBody] VerifyPublicRecordRequest body)
    {
        await mediator.Send(new RecordPublicRecordSourceCommand(
            id, body.SourceType, body.SourceUrl, body.SourceReference, GetUserId(), body.Reason));

        return NoContent();
    }

    /// <summary>Clause 6.3 — pull a corroboration source that has been withdrawn or disproved.</summary>
    [HttpDelete("{id:guid}/corroboration-source")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> RevokeCorroborationSource(
        Guid id, [FromBody] ModerateReportRequest body)
    {
        await mediator.Send(new RevokeCorroborationSourceCommand(id, GetUserId(), body.Reason));
        return NoContent();
    }

    /// <summary>Clause 6.2 — a reporter withdrawing their own report.</summary>
    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] ModerateReportRequest body)
    {
        await mediator.Send(new WithdrawReportCommand(id, GetUserId(), body.Reason));
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }

    private bool IsModerator() => User.IsInRole("Moderator") || User.IsInRole("Admin");
}
