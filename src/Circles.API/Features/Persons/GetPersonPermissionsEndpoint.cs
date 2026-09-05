using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Persons;

/// <summary>Route parameters for the person-in-circle permission lookup.</summary>
public class GetPersonPermissionsRequest
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
}

/// <summary>
/// GET /api/persons/{id}/permissions/{circleId} — the permissions a person has
/// in a given circle (from direct membership and/or derived access).
/// </summary>
public class GetPersonPermissionsEndpoint : Endpoint<GetPersonPermissionsRequest, PermissionsDto>
{
    private readonly CirclesQueryService _svc;

    public GetPersonPermissionsEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/persons/{id}/permissions/{circleId}");
        AllowAnonymous();
        Description(b => b.WithTags("Persons"));
    }

    public override async Task HandleAsync(GetPersonPermissionsRequest req, CancellationToken ct)
    {
        if (!await _svc.PersonExistsAsync(req.Id) || !await _svc.CircleExistsAsync(req.CircleId))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(await _svc.GetPermissionsAsync(req.Id, req.CircleId), ct);
    }
}
