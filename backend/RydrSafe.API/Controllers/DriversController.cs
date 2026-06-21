using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydrSafe.Application.Features.Drivers.Queries;

namespace RydrSafe.API.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetDriversQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetDriverByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search query is required.");
        var result = await mediator.Send(new SearchDriversQuery(q));
        return Ok(result);
    }
}
