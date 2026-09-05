using Circles.Domain.Enums;

namespace Circles.Domain.Entities;

/// <summary>
/// Maps a <see cref="MembershipRole"/> to a <see cref="PermissionType"/> it grants.
///
/// This is the role → permission part of the authorization chain:
/// Person → active membership (or derived access) → circle → role → permission.
/// Keeping it as data (a table) rather than hard-coded logic makes the permission
/// model auditable and adjustable without code changes.
/// </summary>
public class RolePermission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public MembershipRole Role { get; set; }

    public PermissionType Permission { get; set; }
}
