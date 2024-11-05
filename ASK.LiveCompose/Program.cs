using System.Threading.RateLimiting;
using ASK.LiveCompose.Configuration;
using ASK.LiveCompose.Middlewares;
using ASK.LiveCompose.Services;
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