namespace DomusMind.Contracts.Tasks;

public sealed record AssignTaskResponse(
    Guid TaskId,
    Guid AssigneeId,
    string Status);
