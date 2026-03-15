using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Calendar;

namespace DomusMind.Application.Features.Calendar.ScheduleEvent;

public sealed record ScheduleEventCommand(
    string Title,
    Guid FamilyId,
    DateTime StartTime,
    DateTime? EndTime,
    string? Description,
    Guid RequestedByUserId)
    : ICommand<ScheduleEventResponse>;
