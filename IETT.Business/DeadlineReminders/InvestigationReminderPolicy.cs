using Microsoft.Extensions.Options;

namespace IETT.Business.DeadlineReminders;

public enum InvestigationReminderType { FinalBusinessDay, Overdue }

public interface IInvestigationReminderPolicy
{
    InvestigationReminderType? GetReminderType(
        DateTime complaintCreatedDate,
        DateTime? lastFinalDayReminderSentAtUtc,
        DateTime? lastOverdueReminderSentAtUtc,
        DateTime utcNow);
    DateOnly GetFinalBusinessDate(DateTime complaintCreatedDate);
}

public sealed class InvestigationReminderPolicy : IInvestigationReminderPolicy
{
    private readonly IBusinessDayCalculator _calculator;
    private readonly int _businessDayCount;

    public InvestigationReminderPolicy(
        IBusinessDayCalculator calculator,
        IOptions<InvestigationDeadlineOptions> options)
    {
        _calculator = calculator;
        _businessDayCount = options.Value.BusinessDayCount;
    }

    public DateOnly GetFinalBusinessDate(DateTime complaintCreatedDate) =>
        _calculator.GetFinalBusinessDate(complaintCreatedDate, _businessDayCount);

    public InvestigationReminderType? GetReminderType(
        DateTime complaintCreatedDate,
        DateTime? lastFinalDayReminderSentAtUtc,
        DateTime? lastOverdueReminderSentAtUtc,
        DateTime utcNow)
    {
        utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        var localNow = _calculator.ToLocalTime(utcNow);
        var today = DateOnly.FromDateTime(localNow);
        var finalDate = GetFinalBusinessDate(complaintCreatedDate);

        if (today == finalDate)
        {
            if (!lastFinalDayReminderSentAtUtc.HasValue) return InvestigationReminderType.FinalBusinessDay;
            var lastLocal = _calculator.ToLocalTime(
                DateTime.SpecifyKind(lastFinalDayReminderSentAtUtc.Value, DateTimeKind.Utc));
            return lastLocal.Date == localNow.Date && lastLocal.Hour == localNow.Hour
                ? null : InvestigationReminderType.FinalBusinessDay;
        }

        if (today > finalDate)
        {
            if (!lastOverdueReminderSentAtUtc.HasValue) return InvestigationReminderType.Overdue;
            var lastUtc = DateTime.SpecifyKind(lastOverdueReminderSentAtUtc.Value, DateTimeKind.Utc);
            return utcNow - lastUtc >= TimeSpan.FromMinutes(5)
                ? InvestigationReminderType.Overdue : null;
        }

        return null;
    }
}
