using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.GetWeeklyGrid;

public sealed record GetWeeklyGridQuery(
    Guid FamilyId,
    DateOnly WeekStart,
    Guid RequestedByUserId) : IQuery<WeeklyGridResponse>;
