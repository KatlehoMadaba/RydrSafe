using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.Features.DriverFollow.Commands;
using RydrSafe.Application.Features.DriverFollow.Queries;
using RydrSafe.Application.DTOs;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/drivers/{driverId:guid}/follow")]
[Authorize]
public class DriverFollowController(IMediator mediator) : ControllerBase
{
    [HttpGet("/api/follow/drivers")]
    public async Task<IActionResult> GetFollowedDrivers()
    {
        var result = await mediator.Send(new GetFollowedDriversQuery(GetUserId()));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(Guid driverId)
    {
        var isFollowing = await mediator.Send(new GetFollowStatusQuery(GetUserId(), driverId));
        return Ok(new { isFollowing });
    }

    [HttpPost]
    public async Task<IActionResult> Follow(Guid driverId)
    {
        await mediator.Send(new FollowDriverCommand(GetUserId(), driverId));
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Unfollow(Guid driverId)
    {
        await mediator.Send(new UnfollowDriverCommand(GetUserId(), driverId));
        return Ok();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }
}
