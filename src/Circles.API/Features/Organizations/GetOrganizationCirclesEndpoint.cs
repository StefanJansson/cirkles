using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Organizations;

/// <summary>Route parameters for the organization circle hierarchy.</summary>
public class GetOrganizationCirclesRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// GET /api/organizations/{id}/circles — the circle hierarchy (nested tree) for
/// an organization.
/// </summary>
public class GetOrganizationCirclesEndpoint : Endpoint<GetOrganizationCirclesRequest, List<CircleNodeDto>>
{
    private readonly CirclesQueryService _svc;

    public GetOrganizationCirclesEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/organizations/{id}/circles");
        AllowAnonymous();
        Description(b => b.WithTags("Organizations"));
    }

    public override async Task HandleAsync(GetOrganizationCirclesRequest req, CancellationToken ct)
    {
        if (!await _svc.OrganizationExistsAsync(req.Id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(await _svc.GetCircleHierarchyAsync(req.Id), ct);
    }
}
