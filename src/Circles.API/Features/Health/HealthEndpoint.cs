using FastEndpoints;

namespace Circles.API.Features.Health;

/// <summary>GET /health — simple liveness check.</summary>
public class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(b => b.WithTags("Health"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new { status = "healthy", time = DateTime.UtcNow }, ct);
    }
}
