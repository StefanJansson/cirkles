using System.Security.Claims;
using Circles.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Circles.Web.Auth;

/// <summary>
/// Builds the <see cref="ClaimsPrincipal"/> stored in the auth cookie and
/// exposes typed helpers for reading it back inside components.
/// </summary>
public static class CookieClaims
{
    public const string PersonIdClaim = "pid";

    public static ClaimsPrincipal Build(UserAccount account)
    {
        var person = account.Person;
        var displayName = person is not null ? person.FullName : account.Email;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Email, account.Email),
        };

        if (account.PersonId is { } pid)
            claims.Add(new Claim(PersonIdClaim, pid.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}

/// <summary>Typed accessors for the Circles claims on a signed-in principal.</summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserAccountId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static Guid? GetPersonId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(CookieClaims.PersonIdClaim);
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static string? GetFullName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name);

    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email);

    /// <summary>The first name only, for greetings.</summary>
    public static string? GetFirstName(this ClaimsPrincipal user)
    {
        var full = user.GetFullName();
        if (string.IsNullOrWhiteSpace(full)) return null;
        return full.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } parts
            ? parts[0]
            : null;
    }
}
