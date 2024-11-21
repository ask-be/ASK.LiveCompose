/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.Collections;

namespace ASK.LiveCompose.Services;

public interface IDockerComposeService
{
    Task PullProjectAsync(
        string projectName,
        string? service,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task UpProjectAsync(
        string projectName,
        string? service,
        IReadOnlyDictionary<string,string> environmentVariables,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task DownProjectAsync(
        string projectName,
        string? service,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task GetProjectServiceLogs(
        string projectName,
        string? serviceName,
        bool includeTimeStamp,
        string? tail,
        string? since,
        Action<string> writeLogLine,
        CancellationToken cancellationToken);

    Task<string> GetProjectAsync(string projectName, CancellationToken cancellationToken);

    void PrintProjectTokens();

    string? GetProjectToken(string projectName);
}