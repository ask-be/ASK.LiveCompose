/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

[Route("/projects")]
public class UpProjectsEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<BaseProjectInput>.WithoutResult
{
    [HttpPost("{projectName}/update")] // For compatibility purpose
    [HttpPost("{projectName}/up")]
    public override async Task HandleAsync(BaseProjectInput request, CancellationToken cancellationToken = new CancellationToken())
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
        await dockerComposeService.UpProjectAsync(
            request.ProjectName,
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