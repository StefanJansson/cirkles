using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Circles;

/// <summary>Route parameters for the circle member listing.</summary>
public class GetCircleMembersRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// GET /api/circles/{id}/members — the active members of a circle. Historical
/// (expired) memberships are excluded here but remain in the store.
/// </summary>
public class GetCircleMembersEndpoint : Endpoint<GetCircleMembersRequest, List<MemberDto>>
{
    private readonly CirclesQueryService _svc;

    public GetCircleMembersEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/circles/{id}/members");
        Description(b => b.WithTags("Circles"));
    }

    public override async Task HandleAsync(GetCircleMembersRequest req, CancellationToken ct)
    {
        if (!await _svc.CircleExistsAsync(req.Id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(await _svc.GetActiveMembersAsync(req.Id), ct);
    }
}
