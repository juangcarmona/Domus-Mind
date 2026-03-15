namespace DomusMind.Contracts.Calendar;

public sealed record ScheduleEventRequest(
    string Title,
    Guid FamilyId,
    DateTime StartTime,
    DateTime? EndTime,
    string? Description);
