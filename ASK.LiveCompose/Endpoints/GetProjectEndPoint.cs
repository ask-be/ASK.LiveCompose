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

public class GetProjectsEndPointInput : IValidatableObject
{
    [FromRoute(Name = "projectName")]
    public required string ProjectName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!ProjectName.IsValidateServiceOrProjectName())
            yield return new ValidationResult("Invalid Project Id");
    }
}

[Route("/projects")]
public class GetProjectEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<GetProjectsEndPointInput>.WithActionResult
{
    [HttpGet("{projectName}")]
    public override async Task<ActionResult> HandleAsync(GetProjectsEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await dockerComposeService.GetProjectAsync(request.ProjectName, cancellationToken);
        return Ok(result);
    }
}