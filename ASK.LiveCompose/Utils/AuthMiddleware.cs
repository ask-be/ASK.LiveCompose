/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.Text;
using ASK.LiveCompose.Services;

namespace ASK.LiveCompose.Utils;

public class AuthMiddleware(ILogger<AuthMiddleware> logger, IDockerComposeService dockerComposeService) : IMiddleware
{
    private const string AuthHeaderName = "X-Auth-Token";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var projectName = context.Request.Path.Value?.Split('/')[2];
        if (projectName != null)
        {
            var projectToken = dockerComposeService.GetProjectToken(projectName);

            if (context.Request.Headers.TryGetValue(AuthHeaderName, out var requestToken) && projectToken == requestToken.FirstOrDefault())
            {
                logger.LogInformation("Request for project {ProjectName} authenticated successfully", projectName);
                await next(context);
                return;
            }

            logger.LogInformation("Request unauthenticated");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"Missing or invalid X-Auth-Token Header for project {projectName}"));
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}