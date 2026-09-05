namespace Circles.Domain.Interfaces;

/// <summary>
/// Abstracts password hashing so the credential-storage strategy (currently
/// BCrypt) can change without touching application or API code.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces a salted, one-way hash of a plaintext password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plaintext password against a stored hash.</summary>
    bool Verify(string password, string hash);
}
