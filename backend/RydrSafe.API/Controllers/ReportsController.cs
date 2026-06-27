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
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request)
    {
        var userId = GetUserId();
        var id = await mediator.Send(new CreateReportCommand(
            request.DriverName, request.RegistrationNumber, userId,
            request.Category, request.Severity, request.Description, request.IncidentDate));
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
