using Circles.Domain.Enums;
using Circles.Domain.Interfaces;
using Circles.Infrastructure.Persistence;
using Circles.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Circles.Application.Authorization;

/// <summary>
/// Derives authorization decisions purely from the domain model. Access is never
/// stored as a flag on a user; it is always computed from:
///
///   Person → active membership OR derived relationship access → circle → role →
///   permission → resource.
///
/// Derived access currently covers the guardian case: a guardian of a child who
/// is an active member of a circle automatically gains (read-only) access to that
/// circle, without any direct membership of their own.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly CirclesDbContext _db;

    public AuthorizationService(CirclesDbContext db) => _db = db;

    public async Task<bool> CanPersonAccessCircleAsync(Guid personId, Guid circleId, DateTime? at = null)
    {
        var accessible = await GetAccessibleCircleIdsAsync(personId, at);
        return accessible.Contains(circleId);
    }

    public async Task<IReadOnlyCollection<PermissionType>> GetPersonPermissionsInCircleAsync(
        Guid personId, Guid circleId, DateTime? at = null)
    {
        var moment = at ?? DateTime.UtcNow;
        var permissions = new HashSet<PermissionType>();

        // 1) Permissions from active DIRECT memberships in this circle.
        var activeRoles = await _db.Memberships
            .Where(m => m.PersonId == personId
                        && m.CircleId == circleId
                        && m.ValidFrom <= moment
                        && (m.ValidUntil == null || m.ValidUntil > moment))
            .Select(m => m.Role)
            .Distinct()
            .ToListAsync();

        if (activeRoles.Count > 0)
        {
            var rolePerms = await _db.RolePermissions
                .Where(rp => activeRoles.Contains(rp.Role))
                .Select(rp => rp.Permission)
                .ToListAsync();
            foreach (var p in rolePerms) permissions.Add(p);
        }

        // 2) Derived guardian access: if any child of this person is an active
        //    member of the circle, add the derived (read-only) permissions.
        if (await HasDerivedGuardianAccessAsync(personId, circleId, moment))
        {
            foreach (var p in RolePermissionMap.DerivedGuardianPermissions)
                permissions.Add(p);
        }

        return permissions;
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccessibleCircleIdsAsync(Guid personId, DateTime? at = null)
    {
        var moment = at ?? DateTime.UtcNow;
        var circleIds = new HashSet<Guid>();

        // Direct access: every circle where the person has an active membership.
        var direct = await _db.Memberships
            .Where(m => m.PersonId == personId
                        && m.ValidFrom <= moment
                        && (m.ValidUntil == null || m.ValidUntil > moment))
            .Select(m => m.CircleId)
            .Distinct()
            .ToListAsync();
        foreach (var id in direct) circleIds.Add(id);

        // Derived access: circles where a child (for whom this person is an
        // active guardian) has an active membership.
        var childIds = await ActiveChildIdsAsync(personId, moment);
        if (childIds.Count > 0)
        {
            var derived = await _db.Memberships
                .Where(m => childIds.Contains(m.PersonId)
                            && m.ValidFrom <= moment
                            && (m.ValidUntil == null || m.ValidUntil > moment))
                .Select(m => m.CircleId)
                .Distinct()
                .ToListAsync();
            foreach (var id in derived) circleIds.Add(id);
        }

        return circleIds;
    }

    private async Task<bool> HasDerivedGuardianAccessAsync(Guid personId, Guid circleId, DateTime moment)
    {
        var childIds = await ActiveChildIdsAsync(personId, moment);
        if (childIds.Count == 0) return false;

        return await _db.Memberships.AnyAsync(m =>
            childIds.Contains(m.PersonId)
            && m.CircleId == circleId
            && m.ValidFrom <= moment
            && (m.ValidUntil == null || m.ValidUntil > moment));
    }

    /// <summary>
    /// Person ids for whom <paramref name="personId"/> is an active guardian
    /// (via a GuardianOf relationship that is valid at <paramref name="moment"/>).
    /// </summary>
    private async Task<List<Guid>> ActiveChildIdsAsync(Guid personId, DateTime moment)
    {
        return await _db.Relationships
            .Where(r => r.FromPersonId == personId
                        && r.Type == RelationshipType.GuardianOf
                        && r.ValidFrom <= moment
                        && (r.ValidUntil == null || r.ValidUntil > moment))
            .Select(r => r.ToPersonId)
            .Distinct()
            .ToListAsync();
    }
}
