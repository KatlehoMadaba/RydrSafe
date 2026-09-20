using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Recommendations.Commands;
using RydrSafe.Application.Features.Recommendations.Queries;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecommendationRequest request)
    {
        var result = await mediator.Send(new CreateRecommendationCommand(
            GetUserId(), request.Category, request.Subject, request.Message));
        return Ok(result);
    }

    /// <summary>
    /// Only ever the caller's own recommendations — the user id comes from the token, never
    /// from the query string, so one passenger cannot read another's feedback.
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetMyRecommendationsQuery(GetUserId(), page, pageSize));
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }
}
