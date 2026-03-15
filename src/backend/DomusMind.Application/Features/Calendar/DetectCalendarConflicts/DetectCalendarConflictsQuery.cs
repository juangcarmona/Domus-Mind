using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.DetectCalendarConflicts;

public sealed record DetectCalendarConflictsQuery(
    Guid FamilyId,
    DateTime From,
    DateTime? To,
    Guid RequestedByUserId) : IQuery<CalendarConflictsResponse>;
