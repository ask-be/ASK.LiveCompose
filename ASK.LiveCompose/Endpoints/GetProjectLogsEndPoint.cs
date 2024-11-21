/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.ComponentModel.DataAnnotations;
using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

public class GetProjectLogsEndPointInput : BaseProjectInput
{
    [FromQuery(Name = "t")]
    public bool IncludeTimStamp { get; set; } = false;

    [FromQuery(Name = "n")]
    public string? Tail { get; set; }

    [FromQuery(Name = "since")]
    public string? Since { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach(var err in base.Validate(validationContext))
            yield return err;

        if(Tail is not null && !Tail.Equals("ALL", StringComparison.CurrentCultureIgnoreCase) && !int.TryParse(Tail, out _))
            yield return new ValidationResult("Tail is invalid, must be a number or 'All'");
        if(Since is not null && !Since.IsValidSinceValue())
            yield return new ValidationResult("Since is invalid");
    }
}

[Route("/projects")]
public class GetProjectLogsEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<GetProjectLogsEndPointInput>.WithoutResult
{
    [HttpGet("{projectName}/logs")]
    [HttpGet("{projectName}/services/{service}/logs")]
    public override async Task HandleAsync(GetProjectLogsEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var g = HttpContext.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = "text/plain; charset=utf-8";

        await g.StartAsync(cancellationToken);
        await dockerComposeService.GetProjectServiceLogs(
            request.ProjectName,
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