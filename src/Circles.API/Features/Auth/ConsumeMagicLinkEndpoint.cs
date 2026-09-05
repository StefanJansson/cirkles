using Circles.API.Auth;
using Circles.Application.Authentication;
using FastEndpoints;

namespace Circles.API.Features.Auth;

public record ConsumeMagicLinkRequest(string Token);

/// <summary>
/// POST /api/auth/magic-link/consume — redeems a single-use magic link token and
/// returns a bearer token. The link token is invalidated on use.
/// </summary>
public class ConsumeMagicLinkEndpoint : Endpoint<ConsumeMagicLinkRequest, AuthTokenResponse>
{
    private readonly AuthService _auth;
    private readonly TokenService _tokens;

    public ConsumeMagicLinkEndpoint(AuthService auth, TokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    public override void Configure()
    {
        Post("/api/auth/magic-link/consume");
        AllowAnonymous();
        Description(b => b.WithTags("Auth"));
    }

    public override async Task HandleAsync(ConsumeMagicLinkRequest req, CancellationToken ct)
    {
        var account = await _auth.ConsumeMagicLinkAsync(req.Token, ct);
        if (account is null)
        {
            AddError("Länken är ogiltig eller har gått ut.");
            await Send.ErrorsAsync(401, ct);
            return;
        }

        account = await _auth.GetAccountAsync(account.Id, ct) ?? account;
        var (token, expiresAt) = _tokens.CreateToken(account);
        await Send.OkAsync(new AuthTokenResponse(
            token, expiresAt, account.Id, account.PersonId, account.Email,
            account.Person?.FullName), ct);
    }
}
