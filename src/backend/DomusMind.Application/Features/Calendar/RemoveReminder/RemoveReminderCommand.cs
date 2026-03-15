using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.RemoveReminder;

public sealed record RemoveReminderCommand(
    Guid CalendarEventId,
    int MinutesBefore,
    Guid RequestedByUserId)
    : ICommand<RemoveReminderResponse>;
