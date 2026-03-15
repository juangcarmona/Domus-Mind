using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetHouseholdTimeline;

public sealed record GetHouseholdTimelineQuery(
    Guid FamilyId,
    Guid RequestedByUserId)
    : IQuery<HouseholdTimelineResponse>;
