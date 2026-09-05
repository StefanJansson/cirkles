using Circles.Application.DTOs;
using Circles.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Circles.API.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonsController : ControllerBase
{
    private readonly CirclesQueryService _svc;

    public PersonsController(CirclesQueryService svc) => _svc = svc;

    /// <summary>List all persons (with whether they have a user account).</summary>
    [HttpGet]
    public async Task<ActionResult<List<PersonDto>>> GetAll()
        => Ok(await _svc.GetPersonsAsync());

    /// <summary>
    /// All circles a person can access — including circles reachable only through
    /// derived (e.g. guardian) access. Each entry is flagged Direct or Derived.
    /// </summary>
    [HttpGet("{id:guid}/circles")]
    public async Task<ActionResult<List<CircleAccessDto>>> GetAccessibleCircles(Guid id)
    {
        if (!await _svc.PersonExistsAsync(id)) return NotFound();
        return Ok(await _svc.GetAccessibleCirclesAsync(id));
    }

    /// <summary>The permissions a person has in a given circle.</summary>
    [HttpGet("{id:guid}/permissions/{circleId:guid}")]
    public async Task<ActionResult<PermissionsDto>> GetPermissions(Guid id, Guid circleId)
    {
        if (!await _svc.PersonExistsAsync(id)) return NotFound();
        if (!await _svc.CircleExistsAsync(circleId)) return NotFound();
        return Ok(await _svc.GetPermissionsAsync(id, circleId));
    }
}
