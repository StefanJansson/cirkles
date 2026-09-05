using Circles.Application.DTOs;
using Circles.Domain.Interfaces;
using Circles.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Circles.Application.Services;

/// <summary>
/// Read-oriented application service backing the REST endpoints. Keeps EF queries
/// and DTO shaping out of the controllers.
/// </summary>
public class CirclesQueryService
{
    private readonly CirclesDbContext _db;
    private readonly IAuthorizationService _authz;

    public CirclesQueryService(CirclesDbContext db, IAuthorizationService authz)
    {
        _db = db;
        _authz = authz;
    }

    public async Task<List<PersonDto>> GetPersonsAsync()
    {
        return await _db.Persons
            .Include(p => p.UserAccount)
            .OrderBy(p => p.FirstName)
            .Select(p => new PersonDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.FirstName + " " + p.LastName,
                p.UserAccount != null,
                p.UserAccount != null ? p.UserAccount.Email : null))
            .ToListAsync();
    }

    public async Task<bool> PersonExistsAsync(Guid personId) =>
        await _db.Persons.AnyAsync(p => p.Id == personId);

    public async Task<bool> CircleExistsAsync(Guid circleId) =>
        await _db.Circles.AnyAsync(c => c.Id == circleId);

    /// <summary>
    /// All circles a person can access, flagged as Direct or Derived.
    /// </summary>
    public async Task<List<CircleAccessDto>> GetAccessibleCirclesAsync(Guid personId, DateTime? at = null)
    {
        var moment = at ?? DateTime.UtcNow;

        var directIds = (await _db.Memberships
            .Where(m => m.PersonId == personId
                        && m.ValidFrom <= moment
                        && (m.ValidUntil == null || m.ValidUntil > moment))
            .Select(m => m.CircleId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        var allIds = await _authz.GetAccessibleCircleIdsAsync(personId, moment);

        var circles = await _db.Circles
            .Where(c => allIds.Contains(c.Id))
            .ToListAsync();

        return circles
            .Select(c => new CircleAccessDto(
                c.Id, c.Name, c.Slug, c.Type, c.ParentCircleId,
                directIds.Contains(c.Id) ? "Direct" : "Derived"))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<PermissionsDto> GetPermissionsAsync(Guid personId, Guid circleId, DateTime? at = null)
    {
        var perms = await _authz.GetPersonPermissionsInCircleAsync(personId, circleId, at);
        return new PermissionsDto(
            personId,
            circleId,
            perms.Count > 0,
            perms.OrderBy(p => p.ToString()).ToList());
    }

    public async Task<List<OrganizationDto>> GetOrganizationsAsync()
    {
        return await _db.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto(o.Id, o.Name, o.Slug))
            .ToListAsync();
    }

    public async Task<bool> OrganizationExistsAsync(Guid orgId) =>
        await _db.Organizations.AnyAsync(o => o.Id == orgId);

    /// <summary>
    /// The circle hierarchy for an organization, as a nested tree.
    /// </summary>
    public async Task<List<CircleNodeDto>> GetCircleHierarchyAsync(Guid orgId)
    {
        var circles = await _db.Circles
            .Where(c => c.OrganizationId == orgId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var nodes = circles.ToDictionary(
            c => c.Id,
            c => new CircleNodeDto(c.Id, c.Name, c.Slug, c.Type, c.ParentCircleId, new List<CircleNodeDto>()));

        var roots = new List<CircleNodeDto>();
        foreach (var c in circles)
        {
            var node = nodes[c.Id];
            if (c.ParentCircleId != null && nodes.TryGetValue(c.ParentCircleId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }

    /// <summary>
    /// Active members of a circle (memberships valid at the given moment).
    /// Historical memberships are excluded here but remain in the store.
    /// </summary>
    public async Task<List<MemberDto>> GetActiveMembersAsync(Guid circleId, DateTime? at = null)
    {
        var moment = at ?? DateTime.UtcNow;
        return await _db.Memberships
            .Include(m => m.Person)
            .Where(m => m.CircleId == circleId
                        && m.ValidFrom <= moment
                        && (m.ValidUntil == null || m.ValidUntil > moment))
            .OrderBy(m => m.Person!.FirstName)
            .Select(m => new MemberDto(
                m.Id,
                m.PersonId,
                m.Person!.FirstName + " " + m.Person.LastName,
                m.Role,
                m.ValidFrom,
                m.ValidUntil))
            .ToListAsync();
    }
}
