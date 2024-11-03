using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

public class GetProjectLogsEndPointInput
{
    [FromRoute(Name = "projectId")]
    public required string ProjectId { get; set; }

    [FromRoute(Name = "serviceName")]
    public string? ServiceName { get; set; }

    [FromQuery(Name = "t")]
    public bool IncludeTimStamp { get; set; } = false;

    [FromQuery(Name = "n")]
    public string? Tail { get; set; }

    [FromQuery(Name = "since")]
    public string? Since { get; set; }
}

[Route("/projects")]
public class GetProjectLogsEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<GetProjectLogsEndPointInput>.WithoutResult
{
    [HttpGet("{projectId}/logs")]
    [HttpGet("{projectId}/services/{serviceName}/logs")]
    public override async Task HandleAsync(GetProjectLogsEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var g = HttpContext.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = "text/plain; charset=utf-8";

        await g.StartAsync(cancellationToken);
        await dockerComposeService.GetProjectServiceLogs(
            request.ProjectId,
            request.ServiceName,
            request.IncludeTimStamp,
            request.Tail,
            request.Since,
            x =>
        {
            HttpContext.Response.WriteAsync(x + '\n', cancellationToken).Wait(cancellationToken);
            HttpContext.Response.Body.FlushAsync(cancellationToken).Wait(cancellationToken);
        }, cancellationToken);

        await g.CompleteAsync();
    }
}