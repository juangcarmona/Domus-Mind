using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetFamilyPlans;

public sealed record GetFamilyPlansQuery(
    Guid FamilyId,
    Guid? MemberId,
    DateOnly? From,
    DateOnly? To,
    Guid RequestedByUserId) : IQuery<FamilyPlansResponse>;
