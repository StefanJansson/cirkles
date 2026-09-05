namespace Circles.API.Features.Auth;

/// <summary>Standard response after a successful login/registration: a bearer token + who it belongs to.</summary>
public record AuthTokenResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserAccountId,
    Guid? PersonId,
    string Email,
    string? FullName);

/// <summary>Describes the currently authenticated caller (GET /api/auth/me).</summary>
public record MeResponse(
    Guid UserAccountId,
    string Email,
    Guid? PersonId,
    string? FullName,
    bool IsLinkedToPerson);
