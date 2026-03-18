namespace DomusMind.Contracts.Calendar;

/// <summary>
/// Request to schedule a new calendar event.
/// Date is required (ISO format: YYYY-MM-DD).
/// Time is optional (ISO format: HH:mm). When absent the event is date-only.
/// EndDate/EndTime define an optional range; both must be provided together.
/// </summary>
public sealed record ScheduleEventRequest(
    string Title,
    Guid FamilyId,
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string? Description);
