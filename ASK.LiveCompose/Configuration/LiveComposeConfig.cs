/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace ASK.LiveCompose.Configuration;

public class LiveComposeConfig
{
    public string? BasePath { get; set; }
    public string? Key { get; set; }

    public bool EnableRateLimit { get; set; }
    public int RateLimit { get; set; }
    public int RateLimitQueueSize { get; set; }
    public int RateDelaySecond { get; set; }
}