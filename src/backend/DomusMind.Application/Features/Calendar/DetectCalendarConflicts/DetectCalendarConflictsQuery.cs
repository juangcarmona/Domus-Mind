using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.DetectCalendarConflicts;

public sealed record DetectCalendarConflictsQuery(
    Guid FamilyId,
    DateOnly From,
    DateOnly? To,
    Guid RequestedByUserId) : IQuery<CalendarConflictsResponse>;
