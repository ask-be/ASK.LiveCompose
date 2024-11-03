using System.Collections;

namespace ASK.LiveCompose.Services;

public interface IDockerComposeService
{
    Task UpdateProjectAsync(string projectKey,
        string? service,
        IReadOnlyDictionary<string,string> environmentVariables,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task GetProjectServiceLogs(
        string projectKey,
        string? serviceName,
        bool includeTimeStamp,
        string? tail,
        string? since,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task<string> GetProjectAsync(string projectKey, CancellationToken cancellationToken);
    IReadOnlyDictionary<string,string> Projects { get; }
}