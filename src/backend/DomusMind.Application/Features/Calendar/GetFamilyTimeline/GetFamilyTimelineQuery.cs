using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetFamilyTimeline;

public sealed record GetFamilyTimelineQuery(
    Guid FamilyId,
    DateOnly? From,
    DateOnly? To,
    Guid RequestedByUserId)
    : IQuery<FamilyTimelineResponse>;
