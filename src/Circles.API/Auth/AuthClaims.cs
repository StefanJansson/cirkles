namespace Circles.API.Auth;

/// <summary>
/// Canonical claim type names carried inside issued JWTs. Kept in one place so
/// token creation (LoginEndpoint) and token reading (endpoints resolving the
/// caller) never drift apart.
/// </summary>
public static class AuthClaims
{
    /// <summary>The UserAccount id — the authentication identity.</summary>
    public const string UserAccountId = "uid";

    /// <summary>The linked Person id, when the account is linked to a person.</summary>
    public const string PersonId = "pid";

    public const string Email = "email";
}
