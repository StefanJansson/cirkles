namespace Circles.Domain.Entities;

/// <summary>
/// A single-use, time-limited token used for passwordless ("magic link") login.
///
/// This is how a guardian who never sets a password can still sign in: they
/// request a link by email, we mint a token here, and consuming a valid,
/// unexpired, unconsumed token authenticates the linked <see cref="UserAccount"/>.
///
/// Tokens are transient by nature — unlike memberships and relationships they
/// carry no historical meaning, so they may be pruned once expired/consumed.
/// </summary>
public class MagicLinkToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The opaque secret sent to the user (looked up on consume).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>The account this token authenticates.</summary>
    public Guid UserAccountId { get; set; }

    public UserAccount? UserAccount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is redeemed; a consumed token cannot be reused.</summary>
    public DateTime? ConsumedAt { get; set; }

    public bool IsRedeemableAt(DateTime moment) =>
        ConsumedAt is null && moment < ExpiresAt;
}
