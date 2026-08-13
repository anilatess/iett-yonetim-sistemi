namespace IETT.Business.DeadlineReminders;

public sealed class InvestigationDeadlineOptions
{
    public const string SectionName = "InvestigationDeadlineReminders";

    public string TimeZoneId { get; set; } = "Europe/Istanbul";
    public string WindowsTimeZoneId { get; set; } = "Turkey Standard Time";
    public int BusinessDayCount { get; set; } = 3;
    public string ScanCron { get; set; } = "*/5 * * * *";
    public List<string> AnnualFullDayHolidays { get; set; } = [];
    public List<string> FullDayHolidays { get; set; } = [];
    public string HalfDayHolidayPolicy { get; set; } = string.Empty;
}
