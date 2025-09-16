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

public class GetProjectStatusEndPointInput : IValidatableObject
{
    [FromRoute(Name = "projectName")]
    public required string ProjectName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!ProjectName.IsValidProjectName())
            yield return new ValidationResult("Invalid Project Id");
    }
}

[Route("/projects")]
public class GetProjectStatusEndPoint(IDockerComposeService dockerComposeService) : EndpointBaseAsync.WithRequest<GetProjectStatusEndPointInput>.WithActionResult
{
    [HttpGet("{projectName}/ps")]
    [HttpGet("{projectName}/status")]
    public override async Task<ActionResult> HandleAsync(GetProjectStatusEndPointInput request, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await dockerComposeService.GetProjectStatusAsync(request.ProjectName, cancellationToken);
        return Ok(result);
    }
}