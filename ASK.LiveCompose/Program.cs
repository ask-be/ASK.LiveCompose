using ASK.LiveCompose.Services;
using Microsoft.AspNetCore.Mvc;

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
builder.Services.Configure<DockerComposeServiceConfig>(builder.Configuration.GetSection("LiveCompose"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IDockerComposeService, DockerComposeService>();

var app = builder.Build();

var dockerComposeService = app.Services.GetRequiredService<IDockerComposeService>();
foreach (var project in dockerComposeService.Projects)
{
    Console.WriteLine($"{project.Value} => {project.Key}");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();