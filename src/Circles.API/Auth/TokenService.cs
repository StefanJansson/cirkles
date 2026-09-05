using Circles.Domain.Entities;
using FastEndpoints.Security;

namespace Circles.API.Auth;

/// <summary>
/// Issues signed JWTs for authenticated accounts. This is the ONLY place tokens
/// are minted; the AuthService (Application layer) verifies identities but knows
/// nothing about JWTs, keeping transport concerns in the API layer.
/// </summary>
public class TokenService
{
    private readonly string _signingKey;
    private readonly TimeSpan _lifetime;

    public TokenService(IConfiguration config)
    {
        _signingKey = config["Auth:JwtSigningKey"]
            ?? throw new InvalidOperationException("Auth:JwtSigningKey is not configured.");
        _lifetime = TimeSpan.FromHours(
            config.GetValue<double?>("Auth:TokenLifetimeHours") ?? 12);
    }

    /// <summary>Creates a bearer token carrying the account/person identity claims.</summary>
    public (string Token, DateTime ExpiresAt) CreateToken(UserAccount account)
    {
        var expiresAt = DateTime.UtcNow.Add(_lifetime);
        var token = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = _signingKey;
            o.ExpireAt = expiresAt;
            o.User.Claims.Add((AuthClaims.UserAccountId, account.Id.ToString()));
            o.User.Claims.Add((AuthClaims.Email, account.Email));
            if (account.PersonId is { } pid)
                o.User.Claims.Add((AuthClaims.PersonId, pid.ToString()));
        });
        return (token, expiresAt);
    }
}
