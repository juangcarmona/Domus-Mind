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
    string Color,
    IReadOnlyCollection<ParticipantProjection> Participants,
    bool IsReadOnly = false,
    string? Source = null,
    string? ProviderLabel = null,
    string? OpenInProviderUrl = null,
    string? CalendarName = null,
    string? Location = null);

public sealed record WeeklyGridTaskItem(
    Guid TaskId,
    string Title,
    string? DueDate,
    string? DueTime,
    string Status,
    string Color);

public sealed record WeeklyGridListItem(
    Guid ListId,
    string ListName,
    string? Color,
    Guid ItemId,
    string Title,
    string? Note,
    bool Checked,
    bool Importance,
    string? DueDate,
    string? Reminder,
    string? Repeat,
    Guid? ItemAreaId,
    string? ItemAreaName,
    Guid? TargetMemberId,
    string? TargetMemberName);

public sealed record WeeklyGridCell(
    string Date,
    IReadOnlyCollection<WeeklyGridEventItem> Events,
    IReadOnlyCollection<WeeklyGridTaskItem> Tasks,
    IReadOnlyCollection<WeeklyGridRoutineItem> Routines,
    IReadOnlyCollection<WeeklyGridListItem> ListItems);

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
    string? EndTime,
    string Scope);

public sealed record WeeklyGridResponse(
    string WeekStart,
    string WeekEnd,
    IReadOnlyCollection<WeeklyGridMember> Members,
    IReadOnlyCollection<WeeklyGridCell> SharedCells);