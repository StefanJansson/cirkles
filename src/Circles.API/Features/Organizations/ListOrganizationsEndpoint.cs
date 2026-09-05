using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Organizations;

/// <summary>GET /api/organizations — list all organizations.</summary>
public class ListOrganizationsEndpoint : EndpointWithoutRequest<List<OrganizationDto>>
{
    private readonly CirclesQueryService _svc;

    public ListOrganizationsEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
        Description(b => b.WithTags("Organizations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(await _svc.GetOrganizationsAsync(), ct);
    }
}
