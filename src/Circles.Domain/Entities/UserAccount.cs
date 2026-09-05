namespace Circles.Domain.Entities;

/// <summary>
/// Represents an authentication identity — i.e. login credentials.
///
/// A UserAccount is NOT a person and NOT a membership. It is only the means by
/// which a human authenticates. It links to a <see cref="Person"/> via the
/// nullable <see cref="PersonId"/> so that, once logged in, the system can
/// resolve the human being and, from there, derive memberships and permissions.
/// </summary>
public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    // The person this account authenticates as. Nullable because an account
    // could, in principle, be provisioned before being linked to a person.
    public Guid? PersonId { get; set; }

    public Person? Person { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
