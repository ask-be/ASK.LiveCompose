/*
 * SPDX-FileCopyrightText: 2024 Vincent DARON <vincent@ask.be>
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

using System.Globalization;
using System.Text.RegularExpressions;

namespace ASK.LiveCompose;

public static partial class StringExtensions
{
    public static bool IsValidServiceName(this string serviceName)
    {
        return ValidateServiceName().IsMatch(serviceName);
    }
    public static bool IsValidProjectName(this string serviceName)
    {
        return ValidateProjectName().IsMatch(serviceName);
    }

    public static bool IsValidSinceValue(this string since)
    {
        return DateTime.TryParse(since, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) || RelativeTime().IsMatch(since);
    }

    [GeneratedRegex(@"\d+\w")]
    private static partial Regex RelativeTime();

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_.-]+$")]
    private static partial Regex ValidateServiceName();
    [GeneratedRegex("^[a-zA-Z0-9_.-]+$")]
    private static partial Regex ValidateProjectName();
}