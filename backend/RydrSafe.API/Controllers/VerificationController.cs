using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RydrSafe.Application.DTOs;
using RydrSafe.Application.Features.Verification.Commands;
using RydrSafe.Application.Features.Verification.Queries;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController(IMediator mediator) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// Reduced from 10MB. Three images at the old limit meant roughly 80MB of managed memory
    /// held per request once base64 encoding is counted, which a small container cannot survive
    /// under any concurrency. A phone screenshot is well under this.
    /// </summary>
    private const long MaxFileSize = 2 * 1024 * 1024;

    [HttpPost("upload")]
    [AllowAnonymous]
    [EnableRateLimiting("verification")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile image1,
        IFormFile? image2 = null,
        IFormFile? image3 = null)
    {
        ValidateFile(image1);
        if (image2 is not null) ValidateFile(image2);
        if (image3 is not null) ValidateFile(image3);

        var userId = GetUserIdOrNull();

        var result = await mediator.Send(new UploadVerificationCommand(
            image1.OpenReadStream(),
            image2?.OpenReadStream(),
            image3?.OpenReadStream(),
            userId));

        return Ok(result);
    }

    [HttpPost("manual")]
    [AllowAnonymous]
    [EnableRateLimiting("verification")]
    public async Task<IActionResult> Manual([FromBody] ManualVerificationRequest body)
    {
        var userId = GetUserIdOrNull();
        var result = await mediator.Send(new ManualVerificationCommand(
            body.RegistrationNumber,
            body.DriverName,
            body.PhoneNumber,
            userId));
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) return BadRequest("page must be at least 1.");
        if (pageSize < 1 || pageSize > 100) return BadRequest("pageSize must be between 1 and 100.");

        var userId = GetUserId();
        var result = await mediator.Send(new GetVerificationHistoryQuery(userId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetVerificationStatsQuery(userId));
        return Ok(result);
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw new InvalidOperationException(
                $"File {file.FileName} exceeds the {MaxFileSize / (1024 * 1024)}MB limit.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type {ext} is not supported. Use jpg, jpeg, png, or webp.");
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim);
    }

    // Verification is open to anonymous visitors; returns null when the caller is not logged in.
    private Guid? GetUserIdOrNull()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}
