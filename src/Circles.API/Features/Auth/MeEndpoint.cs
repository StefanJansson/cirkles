using Circles.API.Auth;
using Circles.Application.Authentication;
using FastEndpoints;

namespace Circles.API.Features.Auth;

/// <summary>
/// GET /api/auth/me — returns the currently authenticated caller resolved from
/// the bearer token. Requires a valid JWT.
/// </summary>
public class MeEndpoint : EndpointWithoutRequest<MeResponse>
{
    private readonly AuthService _auth;

    public MeEndpoint(AuthService auth) => _auth = auth;

    public override void Configure()
    {
        Get("/api/auth/me");
        Description(b => b.WithTags("Auth"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var accountId = User.GetUserAccountId();
        if (accountId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var account = await _auth.GetAccountAsync(accountId.Value, ct);
        if (account is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(new MeResponse(
            account.Id,
            account.Email,
            account.PersonId,
            account.Person?.FullName,
            account.PersonId is not null), ct);
    }
}
