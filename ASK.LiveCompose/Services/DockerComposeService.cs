using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ASK.LiveCompose.Services;

public class DockerComposeServiceConfig
{
    public required string BasePath { get; set; }
    public required string Key { get; set; }
}

public class DockerComposeService : IDockerComposeService
{
    private const string DockerApplicationName = "docker";
    private readonly ILogger<DockerComposeService> _logger;
    private readonly DockerComposeServiceConfig _config;
    private readonly Dictionary<string, string> _projects = new();

    public IReadOnlyDictionary<string,string> Projects => _projects;

    public DockerComposeService(ILogger<DockerComposeService> logger, IOptions<DockerComposeServiceConfig> config)
    {
        _logger = logger;
        _config = config.Value;

        if (string.IsNullOrEmpty(_config.Key))
        {
            _logger.LogWarning("Using Default key");
            _config.Key = "1234567890abcdefgh";
        }

        if (string.IsNullOrEmpty(_config.BasePath))
        {
            _logger.LogInformation("Using default Base path : /projects");
            _config.BasePath = "/projects";
        }

        _logger.LogInformation("Loading projects from {Directory}",config.Value.BasePath);

        if (!Directory.Exists(config.Value.BasePath))
        {
            _logger.LogError("Directory {Directory} does not exist", config.Value.BasePath);
        }
        else
        {
            foreach (var projectPath in Directory.GetDirectories(_config.BasePath).OrderBy(x => x))
            {
                _projects.Add(ComputeProjectKey(projectPath),projectPath);
            }
        }
    }

    public async Task UpdateProjectAsync(string projectKey,
        string? service,
        IReadOnlyDictionary<string,string> environmentVariables,
        Action<string> writeLogLine,
        CancellationToken cancellationToken)
    {
        var projectPath = GetProjectPath(projectKey);
        await UpdateEnvironmentVariables(projectPath, environmentVariables);
        await DockerComposePullAsync(projectPath, service, writeLogLine, cancellationToken);
        await DockerComposeUpAsync(projectPath, service, writeLogLine, cancellationToken);
    }

    public async Task GetProjectServiceLogs(
        string projectKey,
        string? serviceName,
        bool includeTimeStamp,
        string? tail,
        string? since,
        Action<string> writeLogLine,
        CancellationToken cancellationToken)
    {
        var projectPath = GetProjectPath(projectKey);

        var parameters = new StringBuilder("compose logs -f --no-color --no-log-prefix");
        if (includeTimeStamp)
        {
            parameters.Append(" -t");
        }

        if (!string.IsNullOrEmpty(tail))
        {
            parameters.Append($" -n {tail}");
        }

        if (!string.IsNullOrEmpty(since))
        {
            parameters.Append($" --since {since}");
        }

        if (!string.IsNullOrEmpty(serviceName))
        {
            parameters.Append($" {serviceName}");
        }

        await ExecuteDockerComposeCommandAsync(projectPath, DockerApplicationName, parameters.ToString(), writeLogLine, cancellationToken);
    }

    public async Task<string> GetProjectAsync(string projectKey, CancellationToken cancellationToken)
    {
        var projectPath = GetProjectPath(projectKey);
        return await DockerComposePsAsync(projectPath);
    }

    private async Task UpdateEnvironmentVariables(string project, IReadOnlyDictionary<string, string> environmentVariables)
    {
        if(environmentVariables.Count ==0)
            return;

        var fullEnvFileName = Path.Combine(project, ".env");

        if(!File.Exists(fullEnvFileName))
            throw new FileNotFoundException($"Environment variables file '{fullEnvFileName}' not found");

        var lines = await File.ReadAllLinesAsync(fullEnvFileName);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();

            var tokens = line.Split('=',2);
            if (tokens.Length < 2 || line.StartsWith('#'))
                continue; // Line is blank or start with a #, ignore

            var name = tokens[0].Trim();

            if (environmentVariables.TryGetValue(name, out var value))
            {
                if (tokens[1] != value)
                {
                    _logger.LogInformation("Updating environment variable {Name}", name);
                    lines[index] = name + "=" + value;
                }
                else
                {
                    _logger.LogInformation("Environment variable {Name} is unchanged", name);
                }
            }
        }

        await File.WriteAllLinesAsync(Path.Combine(project, ".env"), lines);
    }

    private async Task DockerComposePullAsync(
        string projectPath,
        string? service,
        Action<string> writeLogLine,
        CancellationToken cancellationToken)
    {
        await ExecuteDockerComposeCommandAsync(projectPath, DockerApplicationName, $"compose pull {service}", writeLogLine, cancellationToken);
    }

    private async Task DockerComposeUpAsync(
        string projectPath,
        string? service,
        Action<string> writeLogLine,
        CancellationToken cancellationToken)
    {
        await ExecuteDockerComposeCommandAsync(projectPath, DockerApplicationName, $"compose up {service} -d", writeLogLine, cancellationToken);
    }

    private async Task<string> DockerComposePsAsync(string projectPath)
    {
        var result = new StringBuilder();
        await ExecuteDockerComposeCommandAsync(projectPath, DockerApplicationName, "compose ps --format table", x => result.AppendLine(x));
        return result.ToString();
    }

    private async Task ExecuteDockerComposeCommandAsync(
        string projectPath,
        string command,
        string arguments,
        Action<string> outputLogLine,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var outputEventHandler = new DataReceivedEventHandler((_, e) =>
        {
            if (e.Data == null)
                return;

            try
            {
                _logger.LogDebug("Command Output : {Line}",e.Data);
                outputLogLine(e.Data);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"ERROR : Writing output");
            }
        });

        process.ErrorDataReceived += outputEventHandler;
        process.OutputDataReceived += outputEventHandler;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _logger.LogInformation("Docker Command Started '{FileName} {Arguments}'",process.StartInfo.FileName,process.StartInfo.Arguments);
        while (!process.HasExited && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, CancellationToken.None);
        }

        if (!process.HasExited)
        {
            process.Kill();
        }

        _logger.LogInformation("Docker Command Terminated");
    }

    private string ComputeProjectKey(string projectPath)
    {
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(_config.Key + ":" + projectPath))).ToString("N");
    }

    private string GetProjectPath(string projectKey)
    {
        if (!_projects.TryGetValue(projectKey, out var projectPath))
        {
            throw new ApplicationException($"Project {projectKey} not found");
        }

        return projectPath;
    }
}