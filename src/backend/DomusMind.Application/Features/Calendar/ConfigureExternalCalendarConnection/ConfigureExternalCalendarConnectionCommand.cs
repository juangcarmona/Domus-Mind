using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.ConfigureExternalCalendarConnection;

public sealed record ConfigureExternalCalendarConnectionCommand(
    Guid FamilyId,
    Guid MemberId,
    Guid ConnectionId,
    IReadOnlyCollection<(string CalendarId, string CalendarName, bool IsSelected, string? ColorHex)> SelectedCalendars,
    int ForwardHorizonDays,
    bool ScheduledRefreshEnabled,
    int ScheduledRefreshIntervalMinutes,
    Guid RequestedByUserId)
    : ICommand<ConfigureExternalCalendarConnectionResponse>;
