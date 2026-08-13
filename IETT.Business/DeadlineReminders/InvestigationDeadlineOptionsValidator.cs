using System.Globalization;
using Microsoft.Extensions.Options;

namespace IETT.Business.DeadlineReminders;

public sealed class InvestigationDeadlineOptionsValidator
    : IValidateOptions<InvestigationDeadlineOptions>
{
    public ValidateOptionsResult Validate(string? name, InvestigationDeadlineOptions options)
    {
        var failures = new List<string>();
        try { TimeZoneResolver.Resolve(options.TimeZoneId, options.WindowsTimeZoneId); }
        catch (InvalidOperationException exception) { failures.Add(exception.Message); }

        if (options.BusinessDayCount <= 0)
            failures.Add("InvestigationDeadlineReminders:BusinessDayCount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(options.ScanCron))
            failures.Add("InvestigationDeadlineReminders:ScanCron must not be empty.");
        if (string.IsNullOrWhiteSpace(options.HalfDayHolidayPolicy))
            failures.Add("InvestigationDeadlineReminders:HalfDayHolidayPolicy must document the day-based policy.");

        foreach (var value in options.AnnualFullDayHolidays)
        {
            if (!DateOnly.TryParseExact($"2000-{value}", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _))
                failures.Add($"Invalid annual full-day holiday '{value}'. Expected MM-dd.");
        }
        foreach (var value in options.FullDayHolidays)
        {
            if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _))
                failures.Add($"Invalid full-day holiday '{value}'. Expected yyyy-MM-dd.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
