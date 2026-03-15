namespace DomusMind.Contracts.Tasks;

public sealed record ReassignTaskRequest(Guid NewAssigneeId);

public sealed record ReassignTaskResponse(
    Guid TaskId,
    Guid? PreviousAssigneeId,
    Guid NewAssigneeId);
