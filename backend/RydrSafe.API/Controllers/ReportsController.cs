using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxEvidenceFileSize = 10 * 1024 * 1024; // 10MB

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateReportRequest request, IFormFileCollection? evidence)
    {
        if (evidence is { Count: > 3 })
            return BadRequest("A maximum of 3 evidence files may be attached.");

        var evidenceStreams = new List<(Stream Data, string FileName)>();
        if (evidence is not null)
        {
            foreach (var file in evidence)
            {
                if (file.Length > MaxEvidenceFileSize)
                    return BadRequest($"{file.FileName} exceeds the 10MB limit.");
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    return BadRequest($"File type {ext} is not supported. Use jpg, jpeg, png, or webp.");
                evidenceStreams.Add((file.OpenReadStream(), file.FileName));
            }
        }

        var userId = GetUserId();
        var id = await mediator.Send(new CreateReportCommand(
            request.DriverName, request.RegistrationNumber, userId,
            request.Category, request.Severity, request.Description, request.IncidentDate,
            evidenceStreams.Count > 0 ? evidenceStreams : null));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetReportsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetReportByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await mediator.Send(new UpdateReportStatusCommand(id, ReportStatus.Approved));
        return NoContent();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await mediator.Send(new UpdateReportStatusCommand(id, ReportStatus.Rejected));
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }
}
