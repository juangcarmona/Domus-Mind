using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetEnrichedTimeline;

public sealed record GetEnrichedTimelineQuery(
    Guid FamilyId,
    IReadOnlyCollection<string>? TypeFilter,
    Guid? MemberFilter,
    DateTime? From,
    DateTime? To,
    IReadOnlyCollection<string>? StatusFilter,
    Guid RequestedByUserId) : IQuery<EnrichedTimelineResponse>;
