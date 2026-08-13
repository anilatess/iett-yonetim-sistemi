using System.Globalization;
using Microsoft.Extensions.Options;

namespace IETT.Business.DeadlineReminders;

public sealed class BusinessDayCalculator : IBusinessDayCalculator
{
    private readonly HashSet<(int Month, int Day)> _annualHolidays;
    private readonly HashSet<DateOnly> _fullDayHolidays;

    public BusinessDayCalculator(IOptions<InvestigationDeadlineOptions> options)
    {
        var value = options.Value;
        TimeZone = TimeZoneResolver.Resolve(value.TimeZoneId, value.WindowsTimeZoneId);
        _annualHolidays = value.AnnualFullDayHolidays
            .Select(x => DateOnly.ParseExact($"2000-{x}", "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Select(x => (x.Month, x.Day)).ToHashSet();
        _fullDayHolidays = value.FullDayHolidays
            .Select(x => DateOnly.ParseExact(x, "yyyy-MM-dd", CultureInfo.InvariantCulture)).ToHashSet();
    }

    public TimeZoneInfo TimeZone { get; }

    public bool IsBusinessDay(DateOnly date) =>
        date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
        && !_annualHolidays.Contains((date.Month, date.Day))
        && !_fullDayHolidays.Contains(date);

    public DateOnly GetFinalBusinessDate(DateTime createdDate, int businessDayCount)
    {
        if (businessDayCount <= 0) throw new ArgumentOutOfRangeException(nameof(businessDayCount));

        var date = DateOnly.FromDateTime(ToLocalTime(createdDate));
        var counted = 0;
        while (true)
        {
            if (IsBusinessDay(date) && ++counted == businessDayCount) return date;
            date = date.AddDays(1);
        }
    }

    public DateTime ToLocalTime(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(value, TimeZone),
        // SQL datetime2 loses Kind. Existing CreatedDate values were written as local wall-clock values.
        _ => DateTime.SpecifyKind(value, DateTimeKind.Unspecified)
    };
}
