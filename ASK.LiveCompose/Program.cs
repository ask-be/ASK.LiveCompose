/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.Threading.RateLimiting;
using ASK.LiveCompose.Configuration;
using ASK.LiveCompose.Services;
using ASK.LiveCompose.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
       .AddEnvironmentVariables("ASK_");

// Add services to the container.
builder.Services.AddLogging(x =>
    x.ClearProviders().AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy/MM/dd HH:mm:ss ";
    }));

builder.Services.AddOptions();
builder.Services.Configure<LiveComposeConfig>(builder.Configuration.GetSection("LiveCompose"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IDockerComposeService, DockerComposeService>();
builder.Services.AddTransient<AuthMiddleware>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id);
    };
});
builder.Services.AddExceptionHandler<DefaultExceptionHandler>();

var options = new LiveComposeConfig();
builder.Configuration.GetSection("LiveCompose").Bind(options);

if(options.EnableRateLimit)
{
    builder.Services.AddRateLimiter(x =>
    {
        x.AddFixedWindowLimiter("fixed",
            y =>
            {
                y.PermitLimit = options.RateLimit;
                y.Window = TimeSpan.FromSeconds(options.RateDelaySecond);
                y.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                y.QueueLimit = options.RateLimitQueueSize;
            });
    });
}

var app = builder.Build();

app.Services.GetRequiredService<IDockerComposeService>().PrintProjectTokens();

if (options.EnableRateLimit)
{
    app.UseRateLimiter();
}

app.UseForwardedHeaders();

app.UseExceptionHandler();

app.UseMiddleware<AuthMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

var controllerActionEndpointConventionBuilder = app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (options.EnableRateLimit)
{
    logger.LogInformation("Rate Limiting is ENABLED (max {RateLimit} requests every {RateDelaySecond} seconds with a queue of {RateLimitQueueSize})",options.RateLimit,options.RateDelaySecond, options.RateLimitQueueSize);
    controllerActionEndpointConventionBuilder.RequireRateLimiting("fixed");
}
else
{
    logger.LogWarning("Rate Limiting is DISABLED");
}

await app.RunAsync();