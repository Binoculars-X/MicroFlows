using NodaTime;
using NodaTime.TimeZones;
using System;
using System.Linq;

namespace MicroFlows.Application.Helpers;

public static class TimeZoneHelper
{
    public static DateTimeOffset? ConvertDateTimeToUtc(DateTime? source, string timeZone)
    {
        if (source == null)
        {
            return null;
        }

        var utcTimezone = "Etc/UTC";
        var local = ConvertDateTimeToDateTimeOffset(source, timeZone);
        var utcConverted = ConvertToTimeZone(local, utcTimezone);
        return utcConverted;
    }

    public static DateTimeOffset? ConvertDateTimeToDateTimeOffset(DateTime? source, string ianaTimeZone)
    {
        if (source == null)
        {
            return null;
        }

        if (ianaTimeZone.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A valid IANA time zone must be provided.");
        }

        var src = source.Value;
        var localDateTime = new LocalDateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, src.Second, src.Millisecond);
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaTimeZone);

        if (timeZone == null)
        {
            throw new ApplicationException($"Invalid IANA time zone [{ianaTimeZone}].");
        }

        var zonedDateTime = localDateTime.InZoneLeniently(timeZone);

        return zonedDateTime.ToDateTimeOffset();
    }

    public static DateTimeOffset? ConvertToTimeZone(DateTimeOffset? source, string ianaTimeZone)
    {
        if (source == null)
        {
            return null;
        }

        if (ianaTimeZone.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A valid IANA time zone must be provided.");
        }

        var timeZoneInfo = GetTimeZoneInfoByIanaTimeZoneId(ianaTimeZone);

        return TimeZoneInfo.ConvertTime(source.Value, timeZoneInfo);
    }

    public static TimeZoneInfo GetTimeZoneInfoByIanaTimeZoneId(string ianaTimeZone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimeZone))
        {
            throw new InvalidOperationException("A valid non-empty IANA Time Zone ID must be provided.");
        }

        var matchingTimeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id.EqualsIgnoreCase(ianaTimeZone));

        if (matchingTimeZoneInfo != null)
        {
            return matchingTimeZoneInfo;
        }

        var windowsTimeZoneId = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.FirstOrDefault(z => z.TzdbIds.Contains(ianaTimeZone, StringComparer.OrdinalIgnoreCase))?.WindowsId;

        if (windowsTimeZoneId.IsNullOrWhiteSpace())
        {
            throw new TimeZoneNotFoundException($"Invalid Time Zone [{ianaTimeZone}].");
        }

        matchingTimeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id.EqualsIgnoreCase(windowsTimeZoneId));

        if (matchingTimeZoneInfo == null)
        {
            throw new TimeZoneNotFoundException($"Failed to find a matching TimeZoneInfo for the [{ianaTimeZone}] IANA Time Zone.");
        }

        return matchingTimeZoneInfo;
    }

}

public static class StringExtensions
{

    public static bool EqualsIgnoreCase(this string? strA, string? strB)
    {
        if (strA == null)
        {
            return false;
        }

        return strA.Equals(strB, StringComparison.OrdinalIgnoreCase);
    }


    public static string IfNullOrWhiteSpace(this string? @this, string defaultValue) =>
        @this.IsNeitherNullNorWhiteSpace() ? @this! : defaultValue;


    public static bool IsIn(this string? strA, params string[] strings)
    {
        if (strA == null)
        {
            return false;
        }

        foreach (var s in strings)
        {
            if (strA.Equals(s))
            {
                return true;
            }
        }

        return false;
    }


    public static bool IsInIgnoreCase(this string? strA, params string[] strings)
    {
        if (strA == null)
        {
            return false;
        }

        foreach (var s in strings)
        {
            if (strA.EqualsIgnoreCase(s))
            {
                return true;
            }
        }

        return false;
    }


    public static bool IsNeitherNullNorWhiteSpace(this string? @this) =>
        !@this.IsNullOrWhiteSpace();


    public static bool IsNullOrEmpty(this string? @this) =>
        string.IsNullOrEmpty(@this);


    public static bool IsNullOrWhiteSpace(this string? @this) =>
        string.IsNullOrWhiteSpace(@this);

}
