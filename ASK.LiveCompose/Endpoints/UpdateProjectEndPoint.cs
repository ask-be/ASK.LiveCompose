using System.ComponentModel.DataAnnotations;
using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

public class UpdateProjectsEndPointInput : IValidatableObject
{
    [FromRoute(Name = "projectId")]
    public required string ProjectId { get; set; }

    [FromRoute(Name = "service")]
    public string? ServiceName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!Guid.TryParse(ProjectId, out _))
            yield return new ValidationResult("Invalid Project Id");
        if(ServiceName is not null && !ServiceName.IsValidateServiceName())
            yield return new ValidationResult("Service name is invalid");
    }
}

[Route("/projects")]
public class UpdateProjectsEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<UpdateProjectsEndPointInput>.WithoutResult
{
    [HttpPost("{projectId}/update")]
    [HttpPost("{projectId}/services/{service}/update")]
    public override async Task HandleAsync(UpdateProjectsEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var g = HttpContext.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

        var environmentVariables = HttpContext.Request.Query
                                              .Where(q => q.Key.StartsWith("ENV_"))
                                              .ToDictionary(
                                                  p => p.Key[4..],
                                                  p => p.Value.ToString());

        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = "text/plain; charset=utf-8";

        await g.StartAsync(cancellationToken);
        await dockerComposeService.UpdateProjectAsync(
            request.ProjectId,
            request.ServiceName,
            environmentVariables,
            x =>
            {
                if(x.Contains("Downloading") || x.Contains("Extracting"))
                    return;

                HttpContext.Response.WriteAsync(x + '\n', cancellationToken).Wait(cancellationToken);
                HttpContext.Response.Body.FlushAsync(cancellationToken).Wait(cancellationToken);
            }, cancellationToken);

        await g.CompleteAsync();
    }
}