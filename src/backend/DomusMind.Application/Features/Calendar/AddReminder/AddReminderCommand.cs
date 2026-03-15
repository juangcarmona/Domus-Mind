using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.AddReminder;

public sealed record AddReminderCommand(
    Guid CalendarEventId,
    int MinutesBefore,
    Guid RequestedByUserId)
    : ICommand<AddReminderResponse>;
