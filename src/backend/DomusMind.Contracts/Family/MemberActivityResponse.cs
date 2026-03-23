namespace DomusMind.Contracts.Family;

public sealed record MemberCalendarActivity(
    Guid EventId,
    string Title,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string Status);

public sealed record MemberTaskActivity(
    Guid TaskId,
    string Title,
    string? DueDate,
    string? DueTime,
    string Status);

public sealed record MemberActivityResponse(
    Guid MemberId,
    string MemberName,
    IReadOnlyCollection<MemberCalendarActivity> CalendarEvents,
    IReadOnlyCollection<MemberTaskActivity> Tasks);
