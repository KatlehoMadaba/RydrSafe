using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Auth.Commands;
using RydrSafe.Application.Features.Auth.Queries;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await mediator.Send(new RegisterCommand(request.FullName, request.Email, request.Password));
        return Ok(result);
    }

    [HttpPost("login")]
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
}
