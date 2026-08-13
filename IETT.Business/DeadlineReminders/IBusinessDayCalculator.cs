namespace IETT.Business.DeadlineReminders;

public interface IBusinessDayCalculator
{
    TimeZoneInfo TimeZone { get; }
    bool IsBusinessDay(DateOnly date);
    DateOnly GetFinalBusinessDate(DateTime createdDate, int businessDayCount);
    DateTime ToLocalTime(DateTime value);
}
