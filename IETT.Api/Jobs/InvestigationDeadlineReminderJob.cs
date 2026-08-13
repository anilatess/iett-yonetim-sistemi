using Hangfire;
using IETT.Api.Hubs;
using IETT.Business.DeadlineReminders;
using IETT.DataAccess.Context;
using IETT.Entity.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IETT.Api.Jobs;

public sealed class InvestigationDeadlineReminderJob
{
    private readonly IETTDbContext _context;
    private readonly IInvestigationReminderPolicy _policy;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InvestigationDeadlineReminderJob> _logger;

    public InvestigationDeadlineReminderJob(
        IETTDbContext context,
        IInvestigationReminderPolicy policy,
        IHubContext<NotificationHub> hubContext,
        TimeProvider timeProvider,
        ILogger<InvestigationDeadlineReminderJob> logger)
    {
        _context = context;
        _policy = policy;
        _hubContext = hubContext;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 240)]
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync()
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var candidates = await _context.Investigations
            .AsNoTracking()
            .Where(x => x.ProcessStatus != InvestigationProcessStatus.Completed
                && x.ClosedDate == null
                && x.Inspector.UserId > 0)
            .Select(x => new
            {
                InvestigationId = x.Id,
                x.ComplaintId,
                x.Complaint.TrackingCode,
                ComplaintCreatedDate = x.Complaint.CreatedDate,
                InspectorUserId = x.Inspector.UserId,
                x.LastFinalDayReminderSentAt,
                x.LastOverdueReminderSentAt
            })
            .ToListAsync();

        foreach (var candidate in candidates)
        {
            var reminderType = _policy.GetReminderType(
                candidate.ComplaintCreatedDate,
                candidate.LastFinalDayReminderSentAt,
                candidate.LastOverdueReminderSentAt,
                utcNow);
            if (!reminderType.HasValue) continue;

            try
            {
                var updated = reminderType == InvestigationReminderType.FinalBusinessDay
                    ? await MarkFinalDayReminderAsync(candidate.InvestigationId,
                        candidate.LastFinalDayReminderSentAt, utcNow)
                    : await MarkOverdueReminderAsync(candidate.InvestigationId,
                        candidate.LastOverdueReminderSentAt, utcNow);
                if (updated == 0) continue;

                var typeName = reminderType.Value.ToString();
                var message = reminderType == InvestigationReminderType.FinalBusinessDay
                    ? $"{candidate.TrackingCode} takip numaralı şikâyetin sonuçlandırılması için son iş günüdür."
                    : $"{candidate.TrackingCode} takip numaralı şikâyetin sonuçlandırma süresi aşılmıştır.";

                try
                {
                    await _hubContext.Clients
                        .Group(NotificationGroupNames.ForInspectorUser(candidate.InspectorUserId))
                        .SendAsync("InvestigationDeadlineReminder", new
                        {
                            investigationId = candidate.InvestigationId,
                            complaintId = candidate.ComplaintId,
                            trackingCode = candidate.TrackingCode,
                            reminderType = typeName,
                            finalBusinessDate = _policy.GetFinalBusinessDate(candidate.ComplaintCreatedDate)
                                .ToString("yyyy-MM-dd"),
                            sentAt = utcNow,
                            message
                        });
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception,
                        "Investigation deadline SignalR notification failed. InvestigationId: {InvestigationId}, TrackingCode: {TrackingCode}, TargetUserId: {TargetUserId}",
                        candidate.InvestigationId, candidate.TrackingCode, candidate.InspectorUserId);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Investigation deadline reminder processing failed. InvestigationId: {InvestigationId}, TrackingCode: {TrackingCode}, TargetUserId: {TargetUserId}",
                    candidate.InvestigationId, candidate.TrackingCode, candidate.InspectorUserId);
            }
        }
    }

    private Task<int> MarkFinalDayReminderAsync(int id, DateTime? expectedMarker, DateTime utcNow) =>
        _context.Investigations
            .Where(x => x.Id == id && x.ProcessStatus != InvestigationProcessStatus.Completed
                && x.ClosedDate == null && x.LastFinalDayReminderSentAt == expectedMarker)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LastFinalDayReminderSentAt, utcNow));

    private Task<int> MarkOverdueReminderAsync(int id, DateTime? expectedMarker, DateTime utcNow) =>
        _context.Investigations
            .Where(x => x.Id == id && x.ProcessStatus != InvestigationProcessStatus.Completed
                && x.ClosedDate == null && x.LastOverdueReminderSentAt == expectedMarker)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.LastOverdueReminderSentAt, utcNow));
}
