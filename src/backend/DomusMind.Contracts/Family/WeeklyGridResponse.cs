using DomusMind.Contracts.Calendar;

namespace DomusMind.Contracts.Family;

public sealed record WeeklyGridEventItem(
    Guid EventId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status,
    IReadOnlyCollection<ParticipantProjection> Participants);

public sealed record WeeklyGridTaskItem(
    Guid TaskId,
    string Title,
    string? DueDate,
    string? DueTime,
    string Status);

public sealed record WeeklyGridCell(
    string Date,
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
    string? Time,
    string Scope);

public sealed record WeeklyGridResponse(
    string WeekStart,
    string WeekEnd,
    IReadOnlyCollection<WeeklyGridMember> Members,
    IReadOnlyCollection<WeeklyGridCell> SharedCells);