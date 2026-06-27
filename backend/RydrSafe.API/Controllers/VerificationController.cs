using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.Features.Verification.Commands;
using RydrSafe.Application.Features.Verification.Queries;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController(IMediator mediator) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        IFormFile image1,
        IFormFile? image2 = null,
        IFormFile? image3 = null)
    {
        ValidateFile(image1);
        if (image2 is not null) ValidateFile(image2);
        if (image3 is not null) ValidateFile(image3);

        var userId = GetUserId();

        var result = await mediator.Send(new UploadVerificationCommand(
            image1.OpenReadStream(),
            image2?.OpenReadStream(),
            image3?.OpenReadStream(),
            userId));

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetVerificationHistoryQuery(userId, page, pageSize));
        return Ok(result);
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw new InvalidOperationException($"File {file.FileName} exceeds the 10MB limit.");

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
}
