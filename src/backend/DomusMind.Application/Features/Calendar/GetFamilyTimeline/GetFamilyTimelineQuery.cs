using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetFamilyTimeline;

public sealed record GetFamilyTimelineQuery(
    Guid FamilyId,
    DateTime? From,
    DateTime? To,
    Guid RequestedByUserId)
    : IQuery<FamilyTimelineResponse>;
