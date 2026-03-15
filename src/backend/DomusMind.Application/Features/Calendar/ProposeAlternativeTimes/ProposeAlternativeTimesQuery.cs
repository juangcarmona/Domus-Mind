using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.ProposeAlternativeTimes;

public sealed record ProposeAlternativeTimesQuery(
    Guid FamilyId,
    Guid EventId,
    int SuggestionCount,
    Guid RequestedByUserId) : IQuery<ProposeAlternativeTimesResponse>;
