using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.GetMemberAgenda;

public sealed record GetMemberAgendaQuery(
    Guid FamilyId,
    Guid MemberId,
    string? From,
    string? To,
    Guid RequestedByUserId)
    : IQuery<MemberAgendaResponse>;
