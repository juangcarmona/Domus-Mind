using DomusMind.Contracts.Calendar;

namespace DomusMind.Contracts.Family;

public sealed record WeeklyGridEventItem(
    Guid EventId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    string Status,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record WeeklyGridTaskItem(
    Guid TaskId,
    string Title,
    DateTime? DueDate,
    string Status);

public sealed record WeeklyGridCell(
    DateTime Date,
    IReadOnlyCollection<WeeklyGridEventItem> Events,
    IReadOnlyCollection<WeeklyGridTaskItem> Tasks,
    IReadOnlyCollection<WeeklyGridRoutineItem> Routines);

public sealed record WeeklyGridMember(
    Guid MemberId,
    string Name,
    string Role,
    IReadOnlyCollection<WeeklyGridCell> Cells);

public sealed record WeeklyGridRoutineItem(
    Guid RoutineId,
    string Name,
    string Kind,
    string Color,
    string Frequency,
    TimeOnly? Time,
    string Scope);

public sealed record WeeklyGridResponse(
    DateTime WeekStart,
    DateTime WeekEnd,
    IReadOnlyCollection<WeeklyGridMember> Members,
    IReadOnlyCollection<WeeklyGridCell> SharedCells);