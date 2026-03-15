namespace DomusMind.Contracts.Family;

public sealed record MemberCalendarActivity(
    Guid EventId,
    string Title,
    DateTime StartTime,
    DateTime? EndTime,
    string Status);

public sealed record MemberTaskActivity(
    Guid TaskId,
    string Title,
    DateTime? DueDate,
    string Status);

public sealed record MemberResponsibilityActivity(
    Guid DomainId,
    string DomainName,
    string Role);

public sealed record MemberActivityResponse(
    Guid MemberId,
    string MemberName,
    IReadOnlyCollection<MemberCalendarActivity> CalendarEvents,
    IReadOnlyCollection<MemberTaskActivity> Tasks,
    IReadOnlyCollection<MemberResponsibilityActivity> Responsibilities);
