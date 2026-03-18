namespace DomusMind.Contracts.Tasks;

public sealed record RescheduleTaskResponse(
    Guid TaskId,
    string? DueDate,
    string? DueTime,
    string Status);
