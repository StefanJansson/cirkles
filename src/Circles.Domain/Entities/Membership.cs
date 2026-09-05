using Circles.Domain.Enums;

namespace Circles.Domain.Entities;

/// <summary>
/// A time-based association of a <see cref="Person"/> with a <see cref="Circle"/>
/// in a specific <see cref="MembershipRole"/>.
///
/// Membership is a distinct concept from both Person and UserAccount. It answers
/// "what does this human belong to, and in what role?".
///
/// IMPORTANT: Memberships are NEVER soft-deleted or hard-deleted. When a
/// membership ends, <see cref="ValidUntil"/> is set to the end date and the row
/// is kept, so historical membership remains fully representable.
/// </summary>
public class Membership
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PersonId { get; set; }
    public Person? Person { get; set; }

    public Guid CircleId { get; set; }
    public Circle? Circle { get; set; }

    public MembershipRole Role { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    // Null while active. Set (never deleted) when the membership ends.
    public DateTime? ValidUntil { get; set; }

    /// <summary>True if the membership is active at the given moment.</summary>
    public bool IsActiveAt(DateTime at) =>
        ValidFrom <= at && (ValidUntil == null || ValidUntil > at);
}
