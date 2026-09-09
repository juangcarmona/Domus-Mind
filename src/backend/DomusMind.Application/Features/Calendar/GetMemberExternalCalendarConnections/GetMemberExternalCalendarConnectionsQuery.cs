using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetMemberExternalCalendarConnections;

public sealed record GetMemberExternalCalendarConnectionsQuery(
    Guid FamilyId,
    Guid MemberId,
    Guid RequestedByUserId)
    : IQuery<IReadOnlyCollection<ExternalCalendarConnectionSummaryResponse>>;
