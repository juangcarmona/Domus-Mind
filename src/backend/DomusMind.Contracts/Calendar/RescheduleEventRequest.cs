namespace DomusMind.Contracts.Calendar;

public sealed record RescheduleEventRequest(
    string Date,
    string? Time,
    string? EndDate,
    string? EndTime,
    string? Title,
    string? Description,
    string? Color);
