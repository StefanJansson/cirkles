using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Persons;

/// <summary>Route parameters for the person-scoped circle listing.</summary>
public class GetPersonCirclesRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// GET /api/persons/{id}/circles — all circles a person can access, including
/// circles reachable only through derived (e.g. guardian) access. Each entry is
/// flagged Direct or Derived.
/// </summary>
public class GetPersonCirclesEndpoint : Endpoint<GetPersonCirclesRequest, List<CircleAccessDto>>
{
    private readonly CirclesQueryService _svc;

    public GetPersonCirclesEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/persons/{id}/circles");
        AllowAnonymous();
        Description(b => b.WithTags("Persons"));
    }

    public override async Task HandleAsync(GetPersonCirclesRequest req, CancellationToken ct)
    {
        if (!await _svc.PersonExistsAsync(req.Id))
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(await _svc.GetAccessibleCirclesAsync(req.Id), cancellation: ct);
    }
}
