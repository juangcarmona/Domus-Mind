using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.DisconnectExternalCalendarConnection;

public sealed record DisconnectExternalCalendarConnectionCommand(
    Guid FamilyId,
    Guid MemberId,
    Guid ConnectionId,
    Guid RequestedByUserId)
    : ICommand<bool>;
