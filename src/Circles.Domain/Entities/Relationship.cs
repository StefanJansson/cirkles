using Circles.Domain.Enums;

namespace Circles.Domain.Entities;

/// <summary>
/// An explicit, time-based relationship between two people.
///
/// Relationships are first-class domain objects rather than implicit fields on
/// Person. This makes them queryable, auditable and, crucially, allows access to
/// be <em>derived</em> from them — for example a guardian gaining read access to
/// the circles their child belongs to.
///
/// Like memberships, relationships are time-based: <see cref="ValidUntil"/> is
/// null while the relationship is active, and set (rather than deleted) once it
/// ends, so history remains representable.
/// </summary>
public class Relationship
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Source person (e.g. the guardian in a GuardianOf relationship).
    public Guid FromPersonId { get; set; }
    public Person? FromPerson { get; set; }

    // Target person (e.g. the child in a GuardianOf relationship).
    public Guid ToPersonId { get; set; }
    public Person? ToPerson { get; set; }

    public RelationshipType Type { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    // Null while active. Set (never deleted) when the relationship ends.
    public DateTime? ValidUntil { get; set; }

    /// <summary>True if the relationship is active at the given moment.</summary>
    public bool IsActiveAt(DateTime at) =>
        ValidFrom <= at && (ValidUntil == null || ValidUntil > at);
}
