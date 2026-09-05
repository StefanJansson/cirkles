using Circles.API.Auth;
using Circles.Application.Authentication;
using FastEndpoints;
using FluentValidation;

namespace Circles.API.Features.Auth;

public record RegisterRequest(string Email, string Password, Guid? PersonId);

public class RegisterValidator : Validator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("En giltig e-postadress krävs.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Lösenordet måste vara minst 8 tecken.");
    }
}

/// <summary>
/// POST /api/auth/register — onboarding. Creates a UserAccount and optionally
/// links it to an existing Person (people, including children, exist first; an
/// account is claimed for one of them). Returns a ready-to-use bearer token.
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequest, AuthTokenResponse>
{
    private readonly AuthService _auth;
    private readonly TokenService _tokens;

    public RegisterEndpoint(AuthService auth, TokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Description(b => b.WithTags("Auth"));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(req.Email, req.Password, req.PersonId, ct);
        if (!result.Succeeded || result.Account is null)
        {
            AddError(result.Error ?? "Registreringen misslyckades.");
            await Send.ErrorsAsync(409, ct);
            return;
        }

        var account = await _auth.GetAccountAsync(result.Account.Id, ct) ?? result.Account;
        var (token, expiresAt) = _tokens.CreateToken(account);
        await Send.OkAsync(new AuthTokenResponse(
            token, expiresAt, account.Id, account.PersonId, account.Email,
            account.Person?.FullName), ct);
    }
}
