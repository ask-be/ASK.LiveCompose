/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace ASK.LiveCompose.Configuration;

public class LiveComposeConfig
{
    /// <summary>
    /// If array contains at least one element, only project present enabled will be available
    /// </summary>
    public string[] ProjectsEnabled { get; set; } = [];
    
    /// <summary>
    /// If array contains at least one element, all projects present will not be available
    /// </summary>
    public string[] ProjectsDisabled { get; set; } = [];
    
    /// <summary>
    /// Base key to compote secret auth token
    /// </summary>
    public string? Key { get; set; }
    
    /// <summary>
    /// Enable Rate limiting
    /// </summary>
    public bool EnableRateLimit { get; set; }
    public int RateLimit { get; set; }
    public int RateLimitQueueSize { get; set; }
    public int RateDelaySecond { get; set; }
}