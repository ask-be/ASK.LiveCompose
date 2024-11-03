using System.Text.Json.Nodes;
using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

public class GetProjectsEndPointInput
{
    [FromRoute(Name = "projectId")]
    public required string ProjectId { get; set; }
}

[Route("/projects")]
public class GetProjectEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<GetProjectsEndPointInput>.WithActionResult
{
    [HttpGet("{projectId}")]
    public override async Task<ActionResult> HandleAsync(GetProjectsEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await dockerComposeService.GetProjectAsync(request.ProjectId, cancellationToken);
        return Ok(result);
    }
}