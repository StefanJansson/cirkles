using Circles.Domain.Enums;

namespace Circles.Infrastructure.Seeding;

/// <summary>
/// The canonical mapping of roles to the permissions they grant. Used to seed the
/// <c>role_permissions</c> table. Everyone with any role can at least read posts,
/// comment and view the member list; higher roles add administrative capabilities.
/// </summary>
public static class RolePermissionMap
{
    public static readonly IReadOnlyDictionary<MembershipRole, PermissionType[]> Map =
        new Dictionary<MembershipRole, PermissionType[]>
        {
            [MembershipRole.Player] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.ViewMemberList
            },
            [MembershipRole.Guardian] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.ViewMemberList
            },
            [MembershipRole.Member] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.CreateDiscussion, PermissionType.CreatePoll,
                PermissionType.ViewMemberList
            },
            [MembershipRole.Coach] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.CreateDiscussion, PermissionType.CreatePoll,
                PermissionType.CreateTask, PermissionType.PublishAnnouncements,
                PermissionType.ViewMemberList, PermissionType.ViewHistoricalInfo
            },
            [MembershipRole.Leader] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.CreateDiscussion, PermissionType.CreatePoll,
                PermissionType.CreateTask, PermissionType.PublishAnnouncements,
                PermissionType.AdministerMembers, PermissionType.ViewMemberList,
                PermissionType.ViewHistoricalInfo
            },
            [MembershipRole.Administrator] = new[]
            {
                PermissionType.ReadPosts, PermissionType.Comment, PermissionType.Vote,
                PermissionType.CreateDiscussion, PermissionType.CreatePoll,
                PermissionType.CreateTask, PermissionType.PublishAnnouncements,
                PermissionType.AdministerMembers, PermissionType.ViewMemberList,
                PermissionType.ViewHistoricalInfo
            }
        };

    /// <summary>
    /// The permissions a guardian receives on a circle through DERIVED access
    /// (because their child is a member). This is intentionally read-only.
    /// </summary>
    public static readonly PermissionType[] DerivedGuardianPermissions = new[]
    {
        PermissionType.ReadPosts,
        PermissionType.ViewMemberList
    };
}
