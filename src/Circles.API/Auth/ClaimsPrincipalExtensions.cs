using System.Security.Claims;

namespace Circles.API.Auth;

/// <summary>
/// Helpers for reading Circles identity claims off the authenticated caller.
/// Centralised so endpoints never hardcode claim names.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserAccountId(this ClaimsPrincipal user) =>
        TryGuid(user.FindFirstValue(AuthClaims.UserAccountId));

    public static Guid? GetPersonId(this ClaimsPrincipal user) =>
        TryGuid(user.FindFirstValue(AuthClaims.PersonId));

    private static Guid? TryGuid(string? value) =>
        Guid.TryParse(value, out var g) ? g : null;
}
