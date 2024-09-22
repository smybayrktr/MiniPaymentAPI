using System.Globalization;
using Payment.Application.Interfaces;

public class TimeZoneService: ITimeZoneService
{
    public TimeZoneInfo GetLocalTimeZone()
    {
        return TimeZoneInfo.Local;
    }

    public DateTime ConvertUtcToLocalTime(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The DateTime object provided must be UTC.", nameof(utcTime));
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, GetLocalTimeZone());
    }

    public DateTime ConvertLocalTimeToUtc(DateTime localTime)
    {
        if (localTime.Kind != DateTimeKind.Local && localTime.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException("The DateTime object provided must be Local or Unspecified.", nameof(localTime));
        }

        return TimeZoneInfo.ConvertTimeToUtc(localTime, GetLocalTimeZone());
    }

    public DateTime GetCurrentLocalTime()
    {
        return DateTime.Now.ToLocalTime();
    }

    public DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    public DateTime ParseToLocalTime(string dateTimeString, string format, IFormatProvider provider = null)
    {
        if (provider == null)
        {
            provider = CultureInfo.CurrentCulture;
        }

        if (DateTime.TryParseExact(dateTimeString, format, provider, DateTimeStyles.None, out DateTime parsedDateTime))
        {
            if (parsedDateTime.Kind == DateTimeKind.Utc)
            {
                return ConvertUtcToLocalTime(parsedDateTime);
            }

            if (parsedDateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Local);
            }

            return parsedDateTime;
        }

        throw new FormatException("The provided date and time string is not in the specified format.");
    }

    public DateTime ParseToLocalTime(string dateTimeString, IFormatProvider provider = null)
    {
        if (provider == null)
        {
            provider = CultureInfo.CurrentCulture;
        }

        if (DateTime.TryParse(dateTimeString, provider, DateTimeStyles.AssumeLocal, out DateTime parsedDateTime))
        {
            return parsedDateTime;
        }

        throw new FormatException("The provided date and time string is not in a valid format.");
    }
}
