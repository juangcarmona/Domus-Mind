using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetExternalCalendarConnectionDetail;

public sealed record GetExternalCalendarConnectionDetailQuery(
    Guid FamilyId,
    Guid MemberId,
    Guid ConnectionId,
    Guid RequestedByUserId)
    : IQuery<ExternalCalendarConnectionDetailResponse>;
