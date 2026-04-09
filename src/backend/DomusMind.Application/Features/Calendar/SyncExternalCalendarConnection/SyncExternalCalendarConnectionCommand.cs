using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.SyncExternalCalendarConnection;

public sealed record SyncExternalCalendarConnectionCommand(
    Guid FamilyId,
    Guid MemberId,
    Guid ConnectionId,
    string Reason,
    Guid RequestedByUserId)
    : ICommand<SyncExternalCalendarConnectionResponse>;
