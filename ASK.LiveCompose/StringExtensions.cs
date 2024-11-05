using System.Globalization;
using System.Text.RegularExpressions;

namespace ASK.LiveCompose;

public static partial class StringExtensions
{
    public static bool IsValidateServiceOrProjectName(this string serviceName)
    {
        return ValidateServiceOrProjectName().IsMatch(serviceName);
    }

    public static bool IsValidSinceValue(this string since)
    {
        return DateTime.TryParse(since, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) || RelativeTime().IsMatch(since);
    }

    [GeneratedRegex(@"\d+\w")]
    private static partial Regex RelativeTime();

    [GeneratedRegex(@"^[a-zA-Z]([-a-zA-Z0-9]*[a-zA-Z0-9])?$")]
    private static partial Regex ValidateServiceOrProjectName();
}