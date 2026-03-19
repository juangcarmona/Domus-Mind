namespace DomusMind.Contracts.Tasks;

public sealed record RescheduleTaskRequest(string? DueDate, string? DueTime, string? Title);
