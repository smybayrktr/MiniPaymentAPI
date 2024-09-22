namespace Payment.Application.Interfaces;

public interface ITimeZoneService
{
    DateTime ConvertUtcToLocalTime(DateTime utcTime);
    DateTime ConvertLocalTimeToUtc(DateTime localTime);
    DateTime GetCurrentLocalTime();
    DateTime GetCurrentUtcTime();
    DateTime ParseToLocalTime(string dateTimeString, string format, IFormatProvider provider = null);
    DateTime ParseToLocalTime(string dateTimeString, IFormatProvider provider = null);
}