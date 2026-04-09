using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.SyncMemberExternalCalendarConnections;

public sealed record SyncMemberExternalCalendarConnectionsCommand(
    Guid FamilyId,
    Guid MemberId,
    string Reason,
    Guid RequestedByUserId)
    : ICommand<SyncMemberExternalCalendarConnectionsResponse>;
