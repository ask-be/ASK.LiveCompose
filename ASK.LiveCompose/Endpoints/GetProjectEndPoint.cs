/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.ComponentModel.DataAnnotations;
using Ardalis.ApiEndpoints;
using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

[Route("/projects")]
public class GetProjectEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<BaseProjectInput>.WithActionResult
{
    [HttpGet("{projectName}")]
    [HttpGet("{projectName}/services/{service}")]
    public override async Task<ActionResult> HandleAsync(BaseProjectInput request, CancellationToken cancellationToken = new())
    {
        var result = await dockerComposeService.GetProjectDefinitionAsync(request.ProjectName, request.ServiceName, cancellationToken);
        return Ok(result);
    }
}