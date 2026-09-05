using Circles.Application.DTOs;
using Circles.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Circles.API.Controllers;

[ApiController]
[Route("api/circles")]
public class CirclesController : ControllerBase
{
    private readonly CirclesQueryService _svc;

    public CirclesController(CirclesQueryService svc) => _svc = svc;

    /// <summary>The active members of a circle (historical members are excluded).</summary>
    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<MemberDto>>> GetMembers(Guid id)
    {
        if (!await _svc.CircleExistsAsync(id)) return NotFound();
        return Ok(await _svc.GetActiveMembersAsync(id));
    }
}
