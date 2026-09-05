using Circles.API.Auth;
using Circles.Application.Authentication;
using FastEndpoints;

namespace Circles.API.Features.Auth;

public record LoginRequest(string Email, string Password);

/// <summary>
/// POST /api/auth/login — password login. Returns a bearer token on success,
/// a generic 401 on any failure (no distinction between unknown email and wrong
/// password, to avoid account enumeration).
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequest, AuthTokenResponse>
{
    private readonly AuthService _auth;
    private readonly TokenService _tokens;

    public LoginEndpoint(AuthService auth, TokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Description(b => b.WithTags("Auth"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = await _auth.ValidateCredentialsAsync(req.Email, req.Password, ct);
        if (account is null)
        {
            AddError("Fel e-postadress eller lösenord.");
            await Send.ErrorsAsync(401, ct);
            return;
        }

        // Reload with the linked person so the response can include the name.
        account = await _auth.GetAccountAsync(account.Id, ct) ?? account;
        var (token, expiresAt) = _tokens.CreateToken(account);
        await Send.OkAsync(new AuthTokenResponse(
            token, expiresAt, account.Id, account.PersonId, account.Email,
            account.Person?.FullName), ct);
    }
}
