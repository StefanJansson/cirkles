using Circles.Application.Authentication;
using FastEndpoints;

namespace Circles.API.Features.Auth;

public record RequestMagicLinkRequest(string Email);

/// <summary>
/// Response for a magic-link request. Always the same generic acknowledgement so
/// a caller cannot probe which emails have accounts. In this MVP the token is
/// echoed back in <see cref="DevToken"/> (populated only in Development) so the
/// flow can be exercised without a real email/SMS provider.
/// </summary>
public record RequestMagicLinkResponse(string Message, string? DevToken, string? DevLoginUrl);

/// <summary>
/// POST /api/auth/magic-link — passwordless login request. This is the path a
/// guardian who never set a password uses: they enter their email, receive a
/// single-use link out of band, and redeem it at /api/auth/magic-link/consume.
/// </summary>
public class RequestMagicLinkEndpoint : Endpoint<RequestMagicLinkRequest, RequestMagicLinkResponse>
{
    private readonly AuthService _auth;
    private readonly IWebHostEnvironment _env;

    public RequestMagicLinkEndpoint(AuthService auth, IWebHostEnvironment env)
    {
        _auth = auth;
        _env = env;
    }

    public override void Configure()
    {
        Post("/api/auth/magic-link");
        AllowAnonymous();
        Description(b => b.WithTags("Auth"));
    }

    public override async Task HandleAsync(RequestMagicLinkRequest req, CancellationToken ct)
    {
        var token = await _auth.CreateMagicLinkAsync(req.Email, ct);

        // In production the token would be emailed/SMS'd. We never reveal here
        // whether an account existed. For local development we surface the token
        // so the passwordless flow is testable end to end.
        const string message =
            "Om det finns ett konto med den e-postadressen har vi skickat en inloggningslänk.";

        if (_env.IsDevelopment() && token is not null)
        {
            await Send.OkAsync(new RequestMagicLinkResponse(
                message, token, $"/api/auth/magic-link/consume?token={token}"), ct);
            return;
        }

        await Send.OkAsync(new RequestMagicLinkResponse(message, null, null), ct);
    }
}
