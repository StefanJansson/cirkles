using Circles.Application.DTOs;
using Circles.Application.Services;
using FastEndpoints;

namespace Circles.API.Features.Persons;

/// <summary>
/// GET /api/persons — list all persons (with whether they have a user account).
/// </summary>
public class ListPersonsEndpoint : EndpointWithoutRequest<List<PersonDto>>
{
    private readonly CirclesQueryService _svc;

    public ListPersonsEndpoint(CirclesQueryService svc) => _svc = svc;

    public override void Configure()
    {
        Get("/api/persons");
        Description(b => b.WithTags("Persons"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(await _svc.GetPersonsAsync(), ct);
    }
}
