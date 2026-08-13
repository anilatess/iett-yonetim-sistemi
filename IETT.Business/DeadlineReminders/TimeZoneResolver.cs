namespace IETT.Business.DeadlineReminders;

public static class TimeZoneResolver
{
    public static TimeZoneInfo Resolve(string timeZoneId, string windowsTimeZoneId)
    {
        foreach (var id in new[] { timeZoneId, windowsTimeZoneId })
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        throw new InvalidOperationException(
            $"Investigation deadline time zone could not be resolved. Tried '{timeZoneId}' and '{windowsTimeZoneId}'.");
    }
}
