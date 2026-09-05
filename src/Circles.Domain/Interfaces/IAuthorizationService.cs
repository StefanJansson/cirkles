using Circles.Domain.Enums;

namespace Circles.Domain.Interfaces;

/// <summary>
/// Answers authorization questions by deriving access from the domain model:
/// Person → active membership OR derived relationship access → circle → role →
/// permission → resource.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// True if the person can access the circle at the given moment, either
    /// through an active direct membership OR through derived access (e.g. a
    /// guardian of a child who is an active member of that circle).
    /// </summary>
    Task<bool> CanPersonAccessCircleAsync(Guid personId, Guid circleId, DateTime? at = null);

    /// <summary>
    /// Returns the set of permissions the person effectively has in the circle,
    /// combining permissions from all active direct memberships and any derived
    /// (e.g. guardian) access.
    /// </summary>
    Task<IReadOnlyCollection<PermissionType>> GetPersonPermissionsInCircleAsync(
        Guid personId, Guid circleId, DateTime? at = null);

    /// <summary>
    /// Returns every circle the person can access at the given moment, including
    /// circles reachable only through derived access.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetAccessibleCircleIdsAsync(Guid personId, DateTime? at = null);
}
