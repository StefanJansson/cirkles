using Circles.Application.DTOs;
using Circles.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Circles.API.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly CirclesQueryService _svc;

    public OrganizationsController(CirclesQueryService svc) => _svc = svc;

    /// <summary>List all organizations.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrganizationDto>>> GetAll()
        => Ok(await _svc.GetOrganizationsAsync());

    /// <summary>The circle hierarchy (nested tree) for an organization.</summary>
    [HttpGet("{id:guid}/circles")]
    public async Task<ActionResult<List<CircleNodeDto>>> GetCircles(Guid id)
    {
        if (!await _svc.OrganizationExistsAsync(id)) return NotFound();
        return Ok(await _svc.GetCircleHierarchyAsync(id));
    }
}
