namespace Circles.Domain.Enums;

/// <summary>
/// The type of an explicit, time-based relationship between two people.
/// Relationships are first-class domain objects, not implicit links.
/// </summary>
public enum RelationshipType
{
    GuardianOf = 0,
    ChildOf = 1,
    LeaderOf = 2,
    ContactPerson = 3
}

/// <summary>
/// The kind of a circle. A circle is a scoped space owned by an organization.
/// </summary>
public enum CircleType
{
    Team = 0,
    Board = 1,
    Officials = 2,
    General = 3
}

/// <summary>
/// The role a person holds through a (time-based) membership in a circle.
/// </summary>
public enum MembershipRole
{
    Player = 0,
    Guardian = 1,
    Coach = 2,
    Leader = 3,
    Administrator = 4,
    Member = 5
}

/// <summary>
/// A concrete capability that can be granted within a circle.
/// </summary>
public enum PermissionType
{
    ReadPosts = 0,
    CreateDiscussion = 1,
    Comment = 2,
    CreatePoll = 3,
    Vote = 4,
    CreateTask = 5,
    AdministerMembers = 6,
    PublishAnnouncements = 7,
    ViewMemberList = 8,
    ViewHistoricalInfo = 9
}
