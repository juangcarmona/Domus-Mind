namespace DomusMind.Contracts.Tasks;

public sealed record RescheduleTaskResponse(
    Guid TaskId,
    DateTime? NewDueDate,
    string Status);
