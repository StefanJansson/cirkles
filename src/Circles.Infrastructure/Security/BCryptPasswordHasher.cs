using Circles.Domain.Interfaces;

namespace Circles.Infrastructure.Security;

/// <summary>
/// BCrypt-based implementation of <see cref="IPasswordHasher"/>. BCrypt embeds
/// the salt and work factor in the hash string, so no separate salt column is
/// needed and the cost can be raised over time without a schema change.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Stored value isn't a valid BCrypt hash (e.g. legacy demo data).
            return false;
        }
    }
}
