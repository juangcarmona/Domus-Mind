using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.SuggestEventParticipants;

public sealed record SuggestEventParticipantsQuery(
    Guid FamilyId,
    Guid EventId,
    Guid RequestedByUserId) : IQuery<SuggestEventParticipantsResponse>;
